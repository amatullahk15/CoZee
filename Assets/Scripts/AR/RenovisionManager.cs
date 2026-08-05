using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Sentis;

/// <summary>
/// RenovisionManager runs YOLOv8 indoor object detection using Unity Sentis 2.1.3 API.
/// Compatible with Unity 2022.3 LTS and AR Foundation 5.2.
/// Reads background CPU frames asynchronously without interrupting AR camera rendering.
/// </summary>
public class RenovisionManager : MonoBehaviour
{
    [Header("Model & Labels")]
    [Tooltip("Assign renovision.onnx ModelAsset from Assets/Models")]
    [SerializeField] private ModelAsset modelAsset;

    [Tooltip("Assign renovision.names TextAsset from Assets/Models")]
    [SerializeField] private TextAsset labelsAsset;

    [Header("AR Camera Reference")]
    [Tooltip("Reference to ARCameraManager (auto-acquired if unassigned)")]
    [SerializeField] private ARCameraManager arCameraManager;

    [Header("Detection Thresholds")]
    [Tooltip("Minimum confidence score threshold for object detection (default 0.45)")]
    [SerializeField] private float confidenceThreshold = 0.45f;

    [Tooltip("Non-Maximum Suppression (NMS) Intersection-over-Union (IoU) overlap threshold")]
    [SerializeField] private float iouThreshold = 0.45f;

    [Tooltip("Inference interval in seconds (e.g. 0.75s)")]
    [SerializeField] private float inferenceIntervalSeconds = 0.75f;

    [Tooltip("Sentis execution engine backend type")]
    [SerializeField] private BackendType backendType = BackendType.GPUCompute;

    // Standard YOLOv8 input resolution (640x640 RGB)
    private const int InputWidth = 640;
    private const int InputHeight = 640;
    private const int InputChannels = 3;

    // Custom class label list loaded from renovision.names
    private string[] classLabels = new string[]
    {
        "bed", "sofa", "chair", "table", "lamp", "tv",
        "laptop", "wardrobe", "window", "door", "potted plant", "photo frame"
    };

    // Sentis Model & Worker objects
    private Model runtimeModel;
    private Worker worker;

    // Texture buffer & state variables
    private Texture2D arCameraTexture;
    private RenderTexture preprocessRenderTexture;
    private Texture2D preprocessTexture2D;
    private bool isInferencing = false;
    private float lastInferenceTime = 0f;
    private List<DetectedIndoorObject> latestDetections = new List<DetectedIndoorObject>();

    // Events & Properties
    public event Action<List<DetectedIndoorObject>> OnObjectsDetected;
    public List<DetectedIndoorObject> LatestDetections => latestDetections;
    public bool IsReady => worker != null;

    private void Start()
    {
        Debug.Log("=== RenovisionManager Started ===");
        InitializeCameraReferences();
        LoadLabels();
        InitializeSentisModel();
    }

    private void OnDestroy()
    {
        CleanupResources();
    }

    private void Update()
    {
        CaptureAndProcessFrame();
    }

    private void InitializeCameraReferences()
    {
        if (arCameraManager == null)
            arCameraManager = FindObjectOfType<ARCameraManager>();

        if (arCameraManager == null)
        {
            Debug.LogWarning("[RenoVisionPipeline] Stage 3 ARCameraManager Reference: FAILED (ARCameraManager not found in scene).");
        }
    }

    private void LoadLabels()
    {
        if (labelsAsset == null)
        {
            labelsAsset = Resources.Load<TextAsset>("renovision");
            if (labelsAsset == null)
                labelsAsset = Resources.Load<TextAsset>("renovision.txt");
        }

#if UNITY_EDITOR
        if (labelsAsset == null)
        {
            labelsAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Models/renovision.names");
        }
#endif

        if (labelsAsset != null && !string.IsNullOrEmpty(labelsAsset.text))
        {
            string[] lines = labelsAsset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
            {
                classLabels = lines;
                Debug.Log($"[RenovisionManager] Loaded {classLabels.Length} custom labels: {string.Join(", ", classLabels)}");
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
            Debug.LogError("[RenoVisionPipeline] Stage 1 - ONNX Model Asset: FAILED (ModelAsset 'renovision.onnx' is null / unassigned). Object detection inactive.");
            return;
        }

        Debug.Log($"[RenoVisionPipeline] Stage 1 - ONNX Model Asset: SUCCESS (Loaded asset '{modelAsset.name}')");

        try
        {
            runtimeModel = ModelLoader.Load(modelAsset);

            try
            {
                worker = new Worker(runtimeModel, backendType);
                Debug.Log("Sentis worker initialized successfully.");
                Debug.Log($"[RenoVisionPipeline] Stage 2 - Sentis Worker: SUCCESS (Backend: {backendType})");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RenoVisionPipeline] Stage 2 - Sentis Worker GPU Creation Failed ({ex.Message}). Falling back to CPU backend.");
                backendType = BackendType.CPU;
                worker = new Worker(runtimeModel, backendType);
                Debug.Log($"[RenoVisionPipeline] Stage 2 - Sentis Worker: SUCCESS (Fallback Backend: {backendType})");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RenoVisionPipeline] Stage 1 & 2 FAILED: Error loading ONNX model in Sentis: {ex.Message}");
        }
    }

    /// <summary>
    /// Frame Acquisition: Reads background CPU image stream via ARCameraManager at native resolution,
    /// then resizes to YOLO input resolution (640x640) via Graphics.Blit without native height upscaling errors.
    /// </summary>
    private void CaptureAndProcessFrame()
    {
        if (worker == null || isInferencing)
            return;

        if (Time.time - lastInferenceTime < inferenceIntervalSeconds)
            return;

        if (arCameraManager == null)
        {
            Debug.LogWarning("[RenoVisionPipeline] Stage 3 - TryAcquireLatestCpuImage: FAILED (ARCameraManager reference is null)");
            return;
        }

        if (!arCameraManager.enabled)
        {
            Debug.LogWarning("[RenoVisionPipeline] Stage 3 - TryAcquireLatestCpuImage: FAILED (ARCameraManager component is disabled)");
            return;
        }

        if (!arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage cpuImage))
        {
            Debug.LogWarning("[RenoVisionPipeline] Stage 3 - TryAcquireLatestCpuImage(): FAILED (Returned false; waiting for CPU camera frame)");
            return;
        }

        Debug.Log($"[RenoVisionPipeline] Stage 3 - TryAcquireLatestCpuImage(): SUCCESS");
        Debug.Log($"[RenoVisionPipeline] Stage 4 - Camera Resolution: Native Frame {cpuImage.width}x{cpuImage.height} -> Resized Input {InputWidth}x{InputHeight}");

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
            {
                Debug.LogWarning("[RenoVisionPipeline] Stage 4 - Frame Conversion: FAILED (GetConvertedDataSize returned 0)");
                return;
            }

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
                Debug.LogWarning($"[RenoVisionPipeline] Stage 4 - Frame Conversion Exception: {ex.Message}");
            }
            finally
            {
                rawData.Dispose();
            }
        }
    }

    /// <summary>
    /// Async coroutine executing Sentis pre-processing, inference, YOLOv8 output decoding, NMS, and tensor cleanup.
    /// </summary>
    private IEnumerator RunInferenceCoroutine(Texture2D cameraTexture)
    {
        isInferencing = true;

        Color[] pixels = cameraTexture.GetPixels();
        Tensor<float> inputTensor = CreateInputTensor(pixels);
        Debug.Log($"[RenoVisionPipeline] Stage 5 - Input Tensor Shape: {inputTensor.shape}");

        yield return null; // Yield to keep main UI thread smooth

        try
        {
            worker.Schedule(inputTensor);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RenoVisionPipeline] Stage 5 - Inference Worker Schedule Exception: {ex.Message}");
            inputTensor.Dispose();
            isInferencing = false;
            yield break;
        }

        yield return null;

        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;

        if (outputTensor != null)
        {
            Debug.Log($"[RenoVisionPipeline] Stage 6 - Output Tensor Shape: {outputTensor.shape}");
            outputTensor.CompleteAllPendingOperations();
            List<DetectedIndoorObject> rawDetections = DecodeYOLOv8Output(outputTensor);
            List<DetectedIndoorObject> filteredDetections = ApplyNMS(rawDetections, iouThreshold);

            latestDetections = filteredDetections;
            Debug.Log($"[RenoVisionPipeline] Stage 9 - Detections After NMS (threshold {iouThreshold}): {filteredDetections.Count}");

            Debug.Log($"[RenoVisionPipeline] Stage 10 - Passing {latestDetections.Count} detections to DetectionOverlay listeners.");
            OnObjectsDetected?.Invoke(latestDetections);
        }
        else
        {
            Debug.LogError("[RenoVisionPipeline] Stage 6 - PeekOutput(): FAILED (Returned NULL output tensor)");
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
    /// Fast YOLOv8 output decoding matching standard Ultralytics YOLOv8 ONNX export.
    /// Automatically detects whether model outputs raw logits or probabilities,
    /// logs minimum and maximum class score values, and filters by confidenceThreshold (0.45).
    /// </summary>
    private List<DetectedIndoorObject> DecodeYOLOv8Output(Tensor<float> outputTensor)
    {
        List<DetectedIndoorObject> detections = new List<DetectedIndoorObject>();
        TensorShape shape = outputTensor.shape;

        float[] data = outputTensor.DownloadToArray();

        int numClasses = classLabels.Length; // 12 custom classes
        int numAttributes = 4 + numClasses; // 16 total attributes per candidate

        bool isTransposed = false;
        int numAnchors = 8400;

        if (shape.rank == 3)
        {
            if (shape[1] == numAttributes)
            {
                numAnchors = shape[2];
                isTransposed = false; // Shape [1, 16, 8400]
            }
            else if (shape[2] == numAttributes)
            {
                numAnchors = shape[1];
                isTransposed = true; // Shape [1, 8400, 16]
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

            if (score >= confidenceThreshold && maxClassIdx >= 0 && maxClassIdx < classLabels.Length)
            {
                float normCx = cx / InputWidth;
                float normCy = cy / InputHeight;
                float normW  = w / InputWidth;
                float normH  = h / InputHeight;

                float normXMin = Mathf.Clamp01(normCx - (normW / 2f));
                float normYMin = Mathf.Clamp01(normCy - (normH / 2f));

                Rect bbox = new Rect(normXMin, normYMin, normW, normH);
                string label = classLabels[maxClassIdx];

                detections.Add(new DetectedIndoorObject(label, score, bbox));
            }
        }

        Debug.Log($"[RenoVisionPipeline] Stage 8 - Detections After Confidence Threshold ({confidenceThreshold}): {detections.Count}");

        return detections;
    }

    private List<DetectedIndoorObject> ApplyNMS(List<DetectedIndoorObject> rawDetections, float threshold)
    {
        List<DetectedIndoorObject> result = new List<DetectedIndoorObject>();
        rawDetections.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));

        bool[] suppressed = new bool[rawDetections.Count];

        for (int i = 0; i < rawDetections.Count; i++)
        {
            if (suppressed[i]) continue;

            DetectedIndoorObject current = rawDetections[i];
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
