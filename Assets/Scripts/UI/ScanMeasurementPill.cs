using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Shows only the newest completed surface briefly, leaving the complete record in Library.
public class ScanMeasurementPill : MonoBehaviour
{
    const float DisplaySeconds = 3f;
    const float FadeSeconds = 0.25f;

    static readonly Color Surface = new Color(0.04f, 0.11f, 0.13f, 0.88f);

    TextMeshProUGUI label;
    CanvasGroup canvasGroup;
    ARSessionBridge bridge;
    int observedSurfaceCount = -1;
    float visibleUntil;

    public static ScanMeasurementPill Create(Transform parent)
    {
        Transform existing = parent.Find("ScanMeasurementPill");
        if (existing != null)
        {
            ScanMeasurementPill pill = existing.GetComponent<ScanMeasurementPill>();
            pill.gameObject.SetActive(true);
            pill.ResetForScan();
            return pill;
        }

        GameObject panel = new GameObject("ScanMeasurementPill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup), typeof(ScanMeasurementPill));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.14f, 0.75f);
        rect.anchorMax = new Vector2(0.86f, 0.84f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        panel.GetComponent<Image>().color = Surface;
        ScanMeasurementPill created = panel.GetComponent<ScanMeasurementPill>();
        created.canvasGroup = panel.GetComponent<CanvasGroup>();
        created.canvasGroup.blocksRaycasts = false;
        created.canvasGroup.interactable = false;

        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panel.transform, false);
        created.label = textObject.GetComponent<TextMeshProUGUI>();
        created.label.text = string.Empty;
        TMP_FontAsset font = CoZeeTypography.FontFor("Measurement", 12f, FontStyles.Bold);
        if (font != null)
            created.label.font = font;
        created.label.fontSize = 13f;
        created.label.fontStyle = FontStyles.Bold;
        created.label.color = Color.white;
        created.label.alignment = TextAlignmentOptions.Center;
        created.label.enableWordWrapping = true;
        created.label.overflowMode = TextOverflowModes.Overflow;
        created.label.raycastTarget = false;

        RectTransform textRect = created.label.rectTransform;
        textRect.anchorMin = new Vector2(0.05f, 0.08f);
        textRect.anchorMax = new Vector2(0.95f, 0.92f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        created.ResetForScan();
        panel.transform.SetAsLastSibling();
        return created;
    }

    void Update()
    {
        if (bridge == null)
            bridge = FindObjectOfType<ARSessionBridge>();

        if (bridge != null)
        {
            var surfaces = bridge.CreateScanHistorySnapshot();
            if (observedSurfaceCount < 0)
            {
                observedSurfaceCount = surfaces.Count;
            }
            else if (surfaces.Count > observedSurfaceCount)
            {
                observedSurfaceCount = surfaces.Count;
                Show(surfaces[surfaces.Count - 1]);
            }
        }

        if (visibleUntil <= 0f)
            return;

        float remaining = visibleUntil - Time.unscaledTime;
        if (remaining <= 0f)
        {
            Hide();
            return;
        }

        canvasGroup.alpha = remaining < FadeSeconds ? remaining / FadeSeconds : 1f;
    }

    public void ResetForScan()
    {
        observedSurfaceCount = -1;
        Hide();
    }

    void Show(ScannedSurfaceRecord surface)
    {
        string secondName = surface.surfaceType == "Wall" ? "Height" : "Length";
        label.text = surface.label + " captured\nWidth " + surface.primaryDimensionMeters.ToString("F2")
            + " m  |  " + secondName + " " + surface.secondaryDimensionMeters.ToString("F2") + " m";
        visibleUntil = Time.unscaledTime + DisplaySeconds;
        canvasGroup.alpha = 1f;
    }

    void Hide()
    {
        visibleUntil = 0f;
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }
}
