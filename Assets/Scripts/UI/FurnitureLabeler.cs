using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Visualization Script: Renders vibrant green bounding boxes and class labels (e.g. sofa 96%, chair 92%)
/// on a Screen Space UI Canvas in real-time.
/// </summary>
public class FurnitureLabeler : MonoBehaviour
{
    [Header("Component References")]
    [Tooltip("Reference to RenoVisionDetector script in scene")]
    [SerializeField] private RenoVisionDetector detector;

    [Tooltip("UI Canvas overlay RectTransform container")]
    [SerializeField] private RectTransform overlayCanvasRect;

    [Header("Visual Customization")]
    [Tooltip("Green color for bounding box border stroke")]
    [SerializeField] private Color strokeColor = new Color(0.1f, 0.95f, 0.4f, 1.0f); // Bright vibrant green

    [Tooltip("Fill color for bounding box interior")]
    [SerializeField] private Color fillColor = new Color(0.15f, 0.85f, 0.45f, 0.25f); // Translucent green fill

    [Tooltip("Font size for class label text")]
    [SerializeField] private float fontSize = 15f;

    // Cache of pooled UI box GameObjects
    private List<GameObject> boxPool = new List<GameObject>();

    private void Start()
    {
        InitializeUI();
    }

    private void OnDestroy()
    {
        if (detector != null)
        {
            detector.OnFurnitureDetected -= HandleFurnitureDetected;
        }
    }

    private void InitializeUI()
    {
        if (detector == null)
            detector = FindObjectOfType<RenoVisionDetector>();

        if (overlayCanvasRect == null)
        {
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                overlayCanvasRect = parentCanvas.GetComponent<RectTransform>();
            }
            else
            {
                // Auto-create Screen Space Overlay Canvas if missing
                GameObject canvasObj = new GameObject("FurnitureOverlayCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Canvas canvas = canvasObj.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 99;

                CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);

                overlayCanvasRect = canvasObj.GetComponent<RectTransform>();
            }
        }

        if (detector != null)
        {
            detector.OnFurnitureDetected += HandleFurnitureDetected;
        }
    }

    private void HandleFurnitureDetected(List<DetectedFurniture> detectedList)
    {
        HideAllBoxes();

        int count = detectedList != null ? detectedList.Count : 0;
        Debug.Log($"[RenoVisionPipeline] Stage 10 - FurnitureLabeler Received Detections: {count}");

        if (detectedList == null || detectedList.Count == 0 || overlayCanvasRect == null)
            return;

        Vector2 canvasSize = overlayCanvasRect.rect.size;
        if (canvasSize.x <= 0f || canvasSize.y <= 0f)
        {
            canvasSize = new Vector2(Screen.width, Screen.height);
        }

        for (int i = 0; i < detectedList.Count; i++)
        {
            DetectedFurniture furniture = detectedList[i];
            GameObject boxObj = GetOrCreateBoxUI(i);
            boxObj.SetActive(true);

            RectTransform boxRt = boxObj.GetComponent<RectTransform>();

            float x = furniture.BoundingBox.x * canvasSize.x;
            // Invert Y for Unity Canvas (0,0 bottom-left vs 0,0 top-left in image space)
            float y = (1.0f - (furniture.BoundingBox.y + furniture.BoundingBox.height)) * canvasSize.y;
            float w = furniture.BoundingBox.width * canvasSize.x;
            float h = furniture.BoundingBox.height * canvasSize.y;

            boxRt.anchoredPosition = new Vector2(x, y);
            boxRt.sizeDelta = new Vector2(Mathf.Max(20f, w), Mathf.Max(20f, h));

            TextMeshProUGUI labelText = boxObj.GetComponentInChildren<TextMeshProUGUI>();
            if (labelText != null)
            {
                labelText.text = furniture.DisplayLabel; // e.g. "sofa 96%", "chair 92%"
            }
        }
    }

    private GameObject GetOrCreateBoxUI(int index)
    {
        if (index < boxPool.Count)
            return boxPool[index];

        // Green bounding box container
        GameObject boxObj = new GameObject($"FurnitureBox_{index}", typeof(RectTransform), typeof(Image), typeof(Outline));
        boxObj.transform.SetParent(overlayCanvasRect, false);

        RectTransform rt = boxObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot = Vector2.zero;

        Image img = boxObj.GetComponent<Image>();
        img.color = fillColor;

        Outline outline = boxObj.GetComponent<Outline>();
        outline.effectColor = strokeColor;
        outline.effectDistance = new Vector2(3f, -3f);

        // Dark badge label background
        GameObject badgeObj = new GameObject("LabelBadge", typeof(RectTransform), typeof(Image));
        badgeObj.transform.SetParent(boxObj.transform, false);

        RectTransform badgeRt = badgeObj.GetComponent<RectTransform>();
        badgeRt.anchorMin = new Vector2(0, 1);
        badgeRt.anchorMax = new Vector2(0, 1);
        badgeRt.pivot = new Vector2(0, 0);
        badgeRt.anchoredPosition = Vector2.zero;
        badgeRt.sizeDelta = new Vector2(140f, 28f);

        Image badgeImg = badgeObj.GetComponent<Image>();
        badgeImg.color = new Color(0.08f, 0.12f, 0.2f, 0.9f);

        // Label text child
        GameObject labelObj = new GameObject("LabelText", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObj.transform.SetParent(badgeObj.transform, false);

        RectTransform labelRt = labelObj.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.pivot = new Vector2(0.5f, 0.5f);
        labelRt.anchoredPosition = Vector2.zero;
        labelRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI txt = labelObj.GetComponent<TextMeshProUGUI>();
        txt.fontSize = fontSize;
        txt.color = strokeColor; // Green text
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
