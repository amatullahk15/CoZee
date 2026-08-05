using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Sentis;

/// <summary>
/// Data model representing a detected furniture object.
/// Compatible with Unity 2022.3 LTS and Sentis 2.1.3.
/// </summary>
[Serializable]
public class DetectedFurniture
{
    public string ClassName;
    public float Confidence;
    public Rect BoundingBox; // Normalized [0.0 to 1.0] relative screen space
    public Vector3 WorldPosition;
    public bool HasWorldPosition;

    public DetectedFurniture(string className, float confidence, Rect boundingBox, Vector3 worldPosition = default, bool hasWorldPosition = false)
    {
        ClassName = className;
        Confidence = confidence;
        BoundingBox = boundingBox;
        WorldPosition = worldPosition;
        HasWorldPosition = hasWorldPosition;
    }

    public string DisplayLabel => $"{ClassName} {Mathf.RoundToInt(Confidence * 100)}%";
}

/// <summary>
/// RenoVisionDetector handles YOLOv8 ONNX object detection using Unity Sentis 2.1.3 API.
/// Captures AR camera feed via CPU stream, decodes bounding boxes, and estimates 3D world positions via AR raycasting.
/// </summary>
public class RenoVisionDetector : MonoBehaviour
{
    [Header("Model & Label Assets")]
    [Tooltip("Assign renovision.onnx ModelAsset from Assets/Models")]
    [SerializeField] private ModelAsset modelAsset;

    [Tooltip("Assign renovision.names TextAsset from Assets/Models")]
    [SerializeField] private TextAsset namesAsset;

    [Header("AR References")]
    [Tooltip("Reference to ARCameraManager (auto-assigned if unassigned)")]
    [SerializeField] private ARCameraManager arCameraManager;

    [Tooltip("Reference to ARRaycastManager for 3D world position estimation")]
    [SerializeField] private ARRaycastManager arRaycastManager;

    [Header("Inference Settings")]
    [Tooltip("Minimum confidence threshold for furniture detection (default 0.45)")]
    [SerializeField] private float confidenceThreshold = 0.45f;

    [Tooltip("Non-Maximum Suppression (NMS) IoU threshold")]
    [SerializeField] private float iouThreshold = 0.45f;

    [Tooltip("Inference interval in seconds (e.g., 0.75s)")]
    [SerializeField] private float inferenceIntervalSeconds = 0.75f;

    [Tooltip("Sentis execution engine backend")]
    [SerializeField] private BackendType backendType = BackendType.GPUCompute;

    // YOLOv8 Input Resolution (640x640 RGB)
    private const int InputWidth = 640;
    private const int InputHeight = 640;
    private const int InputChannels = 3;

    // Loaded furniture class names
    private string[] classNames = new string[]
    {
        "bed", "sofa", "chair", "table", "lamp", "tv",
        "laptop", "wardrobe", "window", "door", "potted plant", "photo frame"
    };

    // Sentis Model & Worker objects
    private Model runtimeModel;
    private Worker worker;

    // Texture buffer & state
    private Texture2D arCameraTexture;
    private RenderTexture preprocessRenderTexture;
    private Texture2D preprocessTexture2D;
    private bool isInferencing = false;
    private float lastInferenceTime = 0f;
    private List<DetectedFurniture> latestDetections = new List<DetectedFurniture>();
    private List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();

    // Events & Properties
    public event Action<List<DetectedFurniture>> OnFurnitureDetected;
    public List<DetectedFurniture> LatestDetections => latestDetections;
    public bool IsReady => worker != null;

    private void Start()
    {
        InitializeARReferences();
        LoadClassNames();
        InitializeSentisModel();
    }

    private void OnDestroy()
    {
        CleanupResources();
    }

    private void Update()
    {
        CaptureAndDetectFrame();
    }

    private void InitializeARReferences()
    {
        if (arCameraManager == null)
            arCameraManager = FindObjectOfType<ARCameraManager>();

        if (arRaycastManager == null)
            arRaycastManager = FindObjectOfType<ARRaycastManager>();
    }

    private void LoadClassNames()
    {
        if (namesAsset == null)
        {
            namesAsset = Resources.Load<TextAsset>("renovision");
            if (namesAsset == null)
                namesAsset = Resources.Load<TextAsset>("renovision.txt");
        }

#if UNITY_EDITOR
        if (namesAsset == null)
        {
            namesAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Models/renovision.names");
        }
#endif

        if (namesAsset != null && !string.IsNullOrEmpty(namesAsset.text))
        {
            string[] lines = namesAsset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
            {
                classNames = lines;
                Debug.Log($"[RenoVisionDetector] Loaded {classNames.Length} class names: {string.Join(", ", classNames)}");
            }
        }
    }

    private void InitializeSentisModel()
    {
        if (modelAsset == null)
        {
            modelAsset = Resources.Load<ModelAsset>("renovision");
        }

#if UNITY_EDITOR
        if (modelAsset == null)
        {
            modelAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<ModelAsset>("Assets/Models/renovision.onnx");
        }
#endif

        if (modelAsset == null)
        {
            Debug.LogWarning("[RenoVisionDetector] ModelAsset (renovision.onnx) is not assigned. Detection inactive.");
            return;
        }

        try
        {
            runtimeModel = ModelLoader.Load(modelAsset);

            try
            {
                worker = new Worker(runtimeModel, backendType);
                Debug.Log($"[RenoVisionDetector] Sentis worker initialized with backend: {backendType}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RenoVisionDetector] GPU worker creation failed ({ex.Message}). Falling back to CPU backend.");
                backendType = BackendType.CPU;
                worker = new Worker(runtimeModel, backendType);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RenoVisionDetector] Error initializing Sentis model: {ex.Message}");
        }
    }

    /// <summary>
    /// Frame Acquisition: Reads background CPU image stream via ARCameraManager.
    /// Strictly CPU-only acquisition; does NOT call Camera.Render or create fake textures.
    /// </summary>
    private void CaptureAndDetectFrame()
    {
        if (worker == null || isInferencing)
            return;

        if (Time.time - lastInferenceTime < inferenceIntervalSeconds)
            return;

        if (arCameraManager == null || !arCameraManager.enabled)
            return;

        if (!arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage cpuImage))
        {
            // If CPU image acquisition fails, do NOT create fake textures or run inference. Wait for next interval.
            return;
        }

        using (cpuImage)
        {
            // Convert using native CPU image resolution (do NOT upscale during Convert)
            if (arCameraTexture == null || arCameraTexture.width != cpuImage.width || arCameraTexture.height != cpuImage.height)
            {
                if (arCameraTexture != null) Destroy(arCameraTexture);
                arCameraTexture = new Texture2D(cpuImage.width, cpuImage.height, TextureFormat.RGBA32, false);
            }

            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, cpuImage.width, cpuImage.height),
                outputDimensions = new Vector2Int(cpuImage.width, cpuImage.height),
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.None
            };

            int rawDataSize = cpuImage.GetConvertedDataSize(conversionParams);
            if (rawDataSize <= 0)
                return;

            var rawData = new Unity.Collections.NativeArray<byte>(rawDataSize, Unity.Collections.Allocator.Temp);

            try
            {
                cpuImage.Convert(conversionParams, rawData);
                arCameraTexture.LoadRawTextureData(rawData);
                arCameraTexture.Apply();

                // Resize native texture to 640x640 model input resolution using Graphics.Blit
                if (preprocessRenderTexture == null)
                {
                    preprocessRenderTexture = new RenderTexture(InputWidth, InputHeight, 0, RenderTextureFormat.ARGB32);
                    preprocessRenderTexture.Create();
                }

                Graphics.Blit(arCameraTexture, preprocessRenderTexture);

                if (preprocessTexture2D == null)
                {
                    preprocessTexture2D = new Texture2D(InputWidth, InputHeight, TextureFormat.RGBA32, false);
                }

                RenderTexture previousActive = RenderTexture.active;
                RenderTexture.active = preprocessRenderTexture;
                preprocessTexture2D.ReadPixels(new Rect(0, 0, InputWidth, InputHeight), 0, 0);
                preprocessTexture2D.Apply();
                RenderTexture.active = previousActive;

                lastInferenceTime = Time.time;
                StartCoroutine(RunInferenceCoroutine(preprocessTexture2D));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RenoVisionDetector] Error converting XRCpuImage: {ex.Message}");
            }
            finally
            {
                rawData.Dispose();
            }
        }
    }

    /// <summary>
    /// Async coroutine executing Sentis pre-processing, inference, YOLOv8 output parsing, NMS, and tensor disposal.
    /// </summary>
    private IEnumerator RunInferenceCoroutine(Texture2D cameraTexture)
    {
        isInferencing = true;

        Color[] pixels = cameraTexture.GetPixels();
        Tensor<float> inputTensor = CreateInputTensor(pixels);

        yield return null; // Yield to keep main UI thread smooth

        try
        {
            worker.Schedule(inputTensor);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RenoVisionDetector] Sentis worker inference exception: {ex.Message}");
            inputTensor.Dispose();
            isInferencing = false;
            yield break;
        }

        yield return null;

        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;

        if (outputTensor != null)
        {
            outputTensor.CompleteAllPendingOperations();
            List<DetectedFurniture> rawDetections = ParseYOLOv8Output(outputTensor);
            List<DetectedFurniture> filteredDetections = ApplyNMS(rawDetections, iouThreshold);

            EstimateWorldPositions(filteredDetections);

            latestDetections = filteredDetections;
            OnFurnitureDetected?.Invoke(latestDetections);
        }

        inputTensor.Dispose();
        outputTensor?.Dispose();

        isInferencing = false;
    }

    /// <summary>
    /// Sentis 2.1.3 API: Creates a Tensor<float> of shape (1, 3, 640, 640) from float array.
    /// Normalizes RGB float channels to [0.0, 1.0].
    /// </summary>
    private Tensor<float> CreateInputTensor(Color[] pixels)
    {
        float[] tensorArray = new float[1 * InputChannels * InputHeight * InputWidth];
        int planeSize = InputHeight * InputWidth;

        for (int y = 0; y < InputHeight; y++)
        {
            for (int x = 0; x < InputWidth; x++)
            {
                int pixelIdx = y * InputWidth + x;
                Color pixel = pixels[pixelIdx];

                int tensorY = InputHeight - 1 - y; // Align bottom-left Unity texture Y coordinate
                int spatialIdx = tensorY * InputWidth + x;

                tensorArray[0 * planeSize + spatialIdx] = pixel.r;
                tensorArray[1 * planeSize + spatialIdx] = pixel.g;
                tensorArray[2 * planeSize + spatialIdx] = pixel.b;
            }
        }

        return new Tensor<float>(new TensorShape(1, InputChannels, InputHeight, InputWidth), tensorArray);
    }

    /// <summary>
    /// Parses YOLOv8 output tensor in Sentis 2.1.3 API.
    /// Applies Sigmoid activation to raw class logits and filters detections by confidenceThreshold (0.45).
    /// </summary>
    private List<DetectedFurniture> ParseYOLOv8Output(Tensor<float> outputTensor)
    {
        List<DetectedFurniture> detections = new List<DetectedFurniture>();
        TensorShape shape = outputTensor.shape;

        float[] data = outputTensor.DownloadToArray();

        int numClasses = classNames.Length;
        int numAttributes = 4 + numClasses;

        bool isTransposed = false;
        int numAnchors = 8400;

        if (shape.rank == 3)
        {
            if (shape[1] == numAttributes)
            {
                numAnchors = shape[2];
                isTransposed = false;
            }
            else if (shape[2] == numAttributes)
            {
                numAnchors = shape[1];
                isTransposed = true;
            }
            else
            {
                numAnchors = shape[2];
            }
        }

        float minRawScoreSeen = float.MaxValue;
        float maxRawScoreSeen = float.MinValue;
        bool hasLogits = false;

        for (int i = 0; i < numAnchors; i++)
        {
            for (int c = 0; c < numClasses; c++)
            {
                float val = !isTransposed
                    ? data[(4 + c) * numAnchors + i]
                    : data[i * numAttributes + (4 + c)];

                if (val < minRawScoreSeen) minRawScoreSeen = val;
                if (val > maxRawScoreSeen) maxRawScoreSeen = val;

                if (val < -0.01f || val > 1.01f)
                {
                    hasLogits = true;
                }
            }
        }

        Debug.Log($"[RenoVisionPipeline] Stage 7 - Total Candidate Anchors Evaluated: {numAnchors}");
        Debug.Log($"[RenoVisionPipeline] Stage 7 - Raw Class Scores Min: {minRawScoreSeen:F4}, Max: {maxRawScoreSeen:F4} | Mode: {(hasLogits ? "Logits (Applying Sigmoid)" : "Probabilities (Direct)")}");

        for (int i = 0; i < numAnchors; i++)
        {
            float cx, cy, w, h;

            if (!isTransposed)
            {
                cx = data[0 * numAnchors + i];
                cy = data[1 * numAnchors + i];
                w  = data[2 * numAnchors + i];
                h  = data[3 * numAnchors + i];
            }
            else
            {
                int baseIdx = i * numAttributes;
                cx = data[baseIdx + 0];
                cy = data[baseIdx + 1];
                w  = data[baseIdx + 2];
                h  = data[baseIdx + 3];
            }

            int maxClassIdx = -1;
            float maxRawScore = -1000f;

            for (int c = 0; c < numClasses; c++)
            {
                float rawScore = !isTransposed
                    ? data[(4 + c) * numAnchors + i]
                    : data[i * numAttributes + (4 + c)];

                if (rawScore > maxRawScore)
                {
                    maxRawScore = rawScore;
                    maxClassIdx = c;
                }
            }

            // Apply Sigmoid ONLY if model outputs raw logits (values outside [0, 1])
            float score = hasLogits
                ? (1.0f / (1.0f + Mathf.Exp(-maxRawScore)))
                : maxRawScore;

            if (score >= confidenceThreshold && maxClassIdx >= 0 && maxClassIdx < classNames.Length)
            {
                float normCx = cx / InputWidth;
                float normCy = cy / InputHeight;
                float normW  = w / InputWidth;
                float normH  = h / InputHeight;

                float normXMin = Mathf.Clamp01(normCx - (normW / 2f));
                float normYMin = Mathf.Clamp01(normCy - (normH / 2f));

                Rect bbox = new Rect(normXMin, normYMin, normW, normH);
                string label = classNames[maxClassIdx];

                detections.Add(new DetectedFurniture(label, score, bbox));
            }
        }

        Debug.Log($"[RenoVisionPipeline] Stage 8 - Detections After Confidence Threshold ({confidenceThreshold}): {detections.Count}");

        return detections;
    }

    private List<DetectedFurniture> ApplyNMS(List<DetectedFurniture> rawDetections, float threshold)
    {
        List<DetectedFurniture> result = new List<DetectedFurniture>();
        rawDetections.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));

        bool[] suppressed = new bool[rawDetections.Count];

        for (int i = 0; i < rawDetections.Count; i++)
        {
            if (suppressed[i]) continue;

            DetectedFurniture current = rawDetections[i];
            result.Add(current);

            for (int j = i + 1; j < rawDetections.Count; j++)
            {
                if (suppressed[j]) continue;

                float iou = CalculateIoU(current.BoundingBox, rawDetections[j].BoundingBox);
                if ((rawDetections[j].ClassName == current.ClassName && iou >= threshold) || iou >= 0.75f)
                {
                    suppressed[j] = true;
                }
            }
        }

        return result;
    }

    private float CalculateIoU(Rect a, Rect b)
    {
        float xMin = Mathf.Max(a.xMin, b.xMin);
        float yMin = Mathf.Max(a.yMin, b.yMin);
        float xMax = Mathf.Min(a.xMax, b.xMax);
        float yMax = Mathf.Min(a.yMax, b.yMax);

        float intersectionWidth = Mathf.Max(0f, xMax - xMin);
        float intersectionHeight = Mathf.Max(0f, yMax - yMin);
        float intersectionArea = intersectionWidth * intersectionHeight;

        float areaA = a.width * a.height;
        float areaB = b.width * b.height;
        float unionArea = areaA + areaB - intersectionArea;

        return unionArea > 0f ? (intersectionArea / unionArea) : 0f;
    }

    private void EstimateWorldPositions(List<DetectedFurniture> detections)
    {
        if (arRaycastManager == null)
            return;

        foreach (DetectedFurniture furniture in detections)
        {
            Vector2 normCenter = new Vector2(furniture.BoundingBox.x + furniture.BoundingBox.width / 2f, furniture.BoundingBox.y + furniture.BoundingBox.height / 2f);

            Vector2 screenPos = new Vector2(normCenter.x * Screen.width, (1.0f - normCenter.y) * Screen.height);

            if (arRaycastManager.Raycast(screenPos, raycastHits, TrackableType.PlaneWithinPolygon | TrackableType.FeaturePoint))
            {
                furniture.WorldPosition = raycastHits[0].pose.position;
                furniture.HasWorldPosition = true;
            }
        }
    }

    private void CleanupResources()
    {
        if (worker != null)
        {
            worker.Dispose();
            worker = null;
        }

        if (arCameraTexture != null)
        {
            Destroy(arCameraTexture);
            arCameraTexture = null;
        }

        if (preprocessRenderTexture != null)
        {
            preprocessRenderTexture.Release();
            Destroy(preprocessRenderTexture);
            preprocessRenderTexture = null;
        }

        if (preprocessTexture2D != null)
        {
            Destroy(preprocessTexture2D);
            preprocessTexture2D = null;
        }
    }
}
