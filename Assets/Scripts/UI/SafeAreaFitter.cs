using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    RectTransform panel;
    Rect lastSafeArea;
    Vector2Int lastScreenSize;

    void Awake()
    {
        panel = GetComponent<RectTransform>();
        Apply();
    }

    void Update()
    {
        if (Screen.safeArea != lastSafeArea
            || Screen.width != lastScreenSize.x
            || Screen.height != lastScreenSize.y)
        {
            Apply();
        }
    }

    void Apply()
    {
        Rect safe = Screen.safeArea;
        lastSafeArea = safe;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        Vector2 anchorMin = safe.position;
        Vector2 anchorMax = safe.position + safe.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;
    }
}
