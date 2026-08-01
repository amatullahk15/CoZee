using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SplashScreenController : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] float displayDuration = 2f;
    [SerializeField] float fadeDuration = 0.5f;

    void Awake()
    {
        EnsureUi();
    }

    void Start()
    {
        EnsureUi();

        if (titleText != null)
        {
            titleText.text = "AR Interior Design";
            titleText.color = Color.white;
        }

        StartCoroutine(SplashRoutine());
    }

    void EnsureUi()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (titleText == null)
            titleText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (titleText != null)
        {
            if (titleText.font == null)
                titleText.font = TMP_Settings.defaultFontAsset ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

            titleText.text = "AR Interior Design";
            titleText.color = Color.white;
            titleText.fontSize = 44f;
            titleText.overflowMode = TextOverflowModes.Overflow;
            titleText.alignment = TextAlignmentOptions.Center;
        }

        if (canvasGroup == null && titleText == null)
        {
            var canvasGo = new GameObject("SplashCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var panelGo = new GameObject("SplashPanel", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(canvasGo.transform, false);
            var image = panelGo.GetComponent<Image>();
            image.color = new Color(0.07f, 0.09f, 0.13f, 1f);

            var rect = panelGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(panelGo.transform, false);
            titleText = titleGo.GetComponent<TextMeshProUGUI>();
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontSize = 36;
            titleText.color = Color.white;
            titleText.text = "AR Furniture";

            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    IEnumerator SplashRoutine()
    {
        yield return new WaitForSeconds(displayDuration);

        if (canvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1f - (elapsed / fadeDuration);
                yield return null;
            }
        }

        if (AppManager.Instance != null)
            AppManager.Instance.RouteAfterSplash();
        else if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene("MainShell");
        else
            SceneManager.LoadScene("MainShell");
    }
}
