using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// DetectionOverlay draws bounding boxes, class names, and confidence scores on UI Canvas.
/// Example output: Sofa 96%, Wardrobe 91%, Door 94%.
/// Compatible with Unity 2022.3 LTS.
/// </summary>
public class DetectionOverlay : MonoBehaviour
{
    [Header("Component References")]
    [Tooltip("Reference to RenovisionManager in scene")]
    [SerializeField] private RenovisionManager renovisionManager;

    [Tooltip("UI Canvas overlay root transform containing bounding box elements")]
    [SerializeField] private RectTransform overlayCanvasRect;

    [Header("Styling")]
    [Tooltip("Font size for bounding box text label")]
    [SerializeField] private float labelFontSize = 14f;

    // Cache of UI box pool
    private List<GameObject> boxPool = new List<GameObject>();
    private Color strokeColor = new Color(0.1f, 0.95f, 0.4f, 1.0f); // Bright green accent
    private Color fillColor = new Color(0.15f, 0.85f, 0.45f, 0.25f); // Translucent green fill

    private void Start()
    {
        InitializeUI();
    }

    private void OnDestroy()
    {
        if (renovisionManager != null)
        {
            renovisionManager.OnObjectsDetected -= RenderBoundingBoxes;
        }
    }

    private void InitializeUI()
    {
        if (renovisionManager == null)
            renovisionManager = FindObjectOfType<RenovisionManager>();

        if (overlayCanvasRect == null)
        {
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                overlayCanvasRect = parentCanvas.GetComponent<RectTransform>();
            }
            else
            {
                // Auto-create Screen Space Overlay Canvas if missing from scene hierarchy
                GameObject canvasObj = new GameObject("RenovisionOverlayCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Canvas canvas = canvasObj.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 99;

                CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);

                overlayCanvasRect = canvasObj.GetComponent<RectTransform>();
            }
        }

        if (renovisionManager != null)
        {
            renovisionManager.OnObjectsDetected += RenderBoundingBoxes;
        }
    }

    /// <summary>
    /// Renders bounding box RectTransforms and text labels for each detected object.
    /// Maps normalized image space bounding boxes [0..1] to Unity Canvas screen coordinates.
    /// </summary>
    private void RenderBoundingBoxes(List<DetectedIndoorObject> detectedObjects)
    {
        HideAllBoxes();

        int count = detectedObjects != null ? detectedObjects.Count : 0;
        Debug.Log($"[RenoVisionPipeline] Stage 10 - DetectionOverlay Received Detections: {count}");

        if (detectedObjects == null || detectedObjects.Count == 0 || overlayCanvasRect == null)
            return;

        Vector2 canvasSize = overlayCanvasRect.rect.size;
        if (canvasSize.x <= 0f || canvasSize.y <= 0f)
        {
            canvasSize = new Vector2(Screen.width, Screen.height);
        }

        for (int i = 0; i < detectedObjects.Count; i++)
        {
            DetectedIndoorObject obj = detectedObjects[i];
            GameObject boxObj = GetOrCreateBoxUI(i);
            boxObj.SetActive(true);

            RectTransform boxRectTransform = boxObj.GetComponent<RectTransform>();

            float x = obj.BoundingBox.x * canvasSize.x;
            // Invert Y for Unity Canvas (0,0 bottom-left vs 0,0 top-left in image space)
            float y = (1.0f - (obj.BoundingBox.y + obj.BoundingBox.height)) * canvasSize.y;
            float w = obj.BoundingBox.width * canvasSize.x;
            float h = obj.BoundingBox.height * canvasSize.y;

            boxRectTransform.anchoredPosition = new Vector2(x, y);
            boxRectTransform.sizeDelta = new Vector2(Mathf.Max(20f, w), Mathf.Max(20f, h));

            TextMeshProUGUI labelText = boxObj.GetComponentInChildren<TextMeshProUGUI>();
            if (labelText != null)
            {
                labelText.text = obj.DisplayLabel; // e.g. "Sofa 96%", "Wardrobe 91%", "Door 94%"
            }
        }
    }

    private GameObject GetOrCreateBoxUI(int index)
    {
        if (index < boxPool.Count)
            return boxPool[index];

        GameObject boxObj = new GameObject($"DetectionBox_{index}", typeof(RectTransform), typeof(Image), typeof(Outline));
        boxObj.transform.SetParent(overlayCanvasRect, false);

        RectTransform rt = boxObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot = Vector2.zero;

        Image img = boxObj.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.01f);

        Outline outline = boxObj.GetComponent<Outline>();
        outline.effectColor = strokeColor;
        outline.effectDistance = new Vector2(5f, -5f);

        // Label Badge Container
        GameObject badgeObj = new GameObject("LabelBadge", typeof(RectTransform), typeof(Image));
        badgeObj.transform.SetParent(boxObj.transform, false);

        RectTransform badgeRt = badgeObj.GetComponent<RectTransform>();
        badgeRt.anchorMin = new Vector2(0, 1);
        badgeRt.anchorMax = new Vector2(0, 1);
        badgeRt.pivot = new Vector2(0, 0);
        badgeRt.anchoredPosition = Vector2.zero;
        badgeRt.sizeDelta = new Vector2(130f, 26f);

        Image badgeImg = badgeObj.GetComponent<Image>();
        badgeImg.color = new Color(0.08f, 0.12f, 0.2f, 0.9f);

        // Label Text
        GameObject labelObj = new GameObject("LabelText", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObj.transform.SetParent(badgeObj.transform, false);

        RectTransform labelRt = labelObj.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.pivot = new Vector2(0.5f, 0.5f);
        labelRt.anchoredPosition = Vector2.zero;
        labelRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI txt = labelObj.GetComponent<TextMeshProUGUI>();
        txt.fontSize = labelFontSize;
        txt.color = strokeColor;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;

        boxPool.Add(boxObj);
        return boxObj;
    }

    private void HideAllBoxes()
    {
        foreach (GameObject box in boxPool)
        {
            if (box != null)
                box.SetActive(false);
        }
    }
}
