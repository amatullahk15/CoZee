using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [SerializeField] Image fadeImage;
    [SerializeField] float defaultDuration = 0.35f;
    [SerializeField] int sortOrder = 9999;

    Canvas overlayCanvas;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureOverlayCanvas();

        if (fadeImage != null)
            SetAlpha(0f);
    }

    void EnsureOverlayCanvas()
    {
        if (fadeImage == null)
            return;

        overlayCanvas = fadeImage.GetComponentInParent<Canvas>();
        if (overlayCanvas != null)
            return;

        var canvasGo = new GameObject("TransitionCanvas");
        canvasGo.transform.SetParent(transform, false);

        overlayCanvas = canvasGo.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = sortOrder;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        var rect = fadeImage.rectTransform;
        rect.SetParent(canvasGo.transform, false);
        Stretch(rect);
    }

    public Coroutine FadeOut(float duration = -1f)
    {
        return StartCoroutine(FadeRoutine(1f, duration < 0f ? defaultDuration : duration));
    }

    public Coroutine FadeIn(float duration = -1f)
    {
        return StartCoroutine(FadeRoutine(0f, duration < 0f ? defaultDuration : duration));
    }

    IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        if (fadeImage == null)
            yield break;

        float start = fadeImage.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = duration <= 0f ? 1f : elapsed / duration;
            SetAlpha(Mathf.Lerp(start, targetAlpha, t));
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    void SetAlpha(float alpha)
    {
        if (fadeImage == null)
            return;

        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;

        bool blockInput = alpha > 0f;
        fadeImage.raycastTarget = blockInput;

        if (overlayCanvas != null)
        {
            GraphicRaycaster raycaster = overlayCanvas.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
                raycaster.enabled = blockInput;
        }
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
