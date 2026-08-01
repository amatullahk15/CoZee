using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PermissionsScreenController : MonoBehaviour
{
    [SerializeField] PermissionRowUI cameraRow;
    [SerializeField] PermissionRowUI photosRow;
    [SerializeField] PermissionRowUI motionRow;
    [SerializeField] Button continueButton;

    [SerializeField] float cardHeight = 128f;
    [SerializeField] float cardSpacing = 24f;
    [SerializeField] float buttonHeight = 78f;
    [SerializeField] float panelPadding = 36f;

    RectTransform panelRect;

    void Awake()
    {
        panelRect = GetComponent<RectTransform>();

        if (cameraRow == null || photosRow == null || motionRow == null)
        {
            PermissionRowUI[] rows = GetComponentsInChildren<PermissionRowUI>(true);
            if (rows.Length > 0 && cameraRow == null) cameraRow = rows[0];
            if (rows.Length > 1 && photosRow == null) photosRow = rows[1];
            if (rows.Length > 2 && motionRow == null) motionRow = rows[2];
        }

        if (continueButton == null)
        {
            foreach (Button button in GetComponentsInChildren<Button>(true))
            {
                if (button.gameObject.name.Contains("Continue"))
                {
                    continueButton = button;
                    break;
                }
            }
        }
    }

    void Start()
    {
        cameraRow?.Setup(AppPermission.Camera, "Camera");
        photosRow?.Setup(AppPermission.Photos, "Photos");
        motionRow?.Setup(AppPermission.MotionTracking, "Motion Tracking");

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinue);

        ApplyLayout();
    }

    void ApplyLayout()
    {
        if (panelRect == null)
            return;

        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = new Vector2(panelPadding, panelPadding);
        panelRect.offsetMax = new Vector2(-panelPadding, -panelPadding);

        var verticalLayout = panelRect.GetComponent<VerticalLayoutGroup>();
        if (verticalLayout == null)
            verticalLayout = panelRect.gameObject.AddComponent<VerticalLayoutGroup>();

        verticalLayout.padding = new RectOffset(24, 24, 24, 24);
        verticalLayout.spacing = cardSpacing;
        verticalLayout.childAlignment = TextAnchor.UpperCenter;
        verticalLayout.childControlWidth = true;
        verticalLayout.childControlHeight = true;
        verticalLayout.childForceExpandWidth = true;
        verticalLayout.childForceExpandHeight = false;

        var contentFitter = panelRect.GetComponent<ContentSizeFitter>();
        if (contentFitter == null)
            contentFitter = panelRect.gameObject.AddComponent<ContentSizeFitter>();

        contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        ConfigureRow(cameraRow);
        ConfigureRow(photosRow);
        ConfigureRow(motionRow);

        if (continueButton != null)
        {
            var buttonRect = continueButton.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
                buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
                buttonRect.sizeDelta = new Vector2(300f, buttonHeight);
                buttonRect.anchoredPosition = Vector2.zero;
            }

            var buttonLayout = continueButton.GetComponent<LayoutElement>();
            if (buttonLayout == null)
                buttonLayout = continueButton.gameObject.AddComponent<LayoutElement>();

            buttonLayout.preferredHeight = buttonHeight;
            buttonLayout.preferredWidth = 320f;
            buttonLayout.flexibleWidth = 1f;
            buttonLayout.flexibleHeight = 0f;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
    }

    void ConfigureRow(PermissionRowUI row)
    {
        if (row == null)
            return;

        var rowRect = row.GetComponent<RectTransform>();
        if (rowRect == null)
            return;

        rowRect.anchorMin = new Vector2(0.5f, 0.5f);
        rowRect.anchorMax = new Vector2(0.5f, 0.5f);
        rowRect.sizeDelta = new Vector2(0f, cardHeight);
        rowRect.anchoredPosition = Vector2.zero;

        var layoutElement = row.GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = row.gameObject.AddComponent<LayoutElement>();

        layoutElement.preferredHeight = cardHeight;
        layoutElement.preferredWidth = 0f;
        layoutElement.flexibleWidth = 1f;
        layoutElement.flexibleHeight = 0f;

        var rowLayout = row.GetComponent<HorizontalLayoutGroup>();
        if (rowLayout == null)
            rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();

        rowLayout.padding = new RectOffset(24, 24, 0, 0);
        rowLayout.spacing = 24f;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = false;
        rowLayout.childControlHeight = false;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;

        var texts = row.GetComponentsInChildren<TextMeshProUGUI>(true);
        if (texts.Length > 0)
        {
            var titleText = texts[0];
            var titleElement = titleText.GetComponent<LayoutElement>();
            if (titleElement == null)
                titleElement = titleText.gameObject.AddComponent<LayoutElement>();

            titleElement.preferredWidth = 220f;
            titleElement.preferredHeight = 48f;
            titleElement.flexibleWidth = 0f;
            titleElement.flexibleHeight = 0f;
        }

        if (texts.Length > 1)
        {
            var statusText = texts[1];
            var statusElement = statusText.GetComponent<LayoutElement>();
            if (statusElement == null)
                statusElement = statusText.gameObject.AddComponent<LayoutElement>();

            statusElement.preferredWidth = 140f;
            statusElement.preferredHeight = 48f;
            statusElement.flexibleWidth = 0f;
            statusElement.flexibleHeight = 0f;
        }

        var button = row.GetComponentInChildren<Button>(true);
        if (button != null)
        {
            var buttonRect = button.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                buttonRect.sizeDelta = new Vector2(180f, 64f);
            }

            var buttonElement = button.GetComponent<LayoutElement>();
            if (buttonElement == null)
                buttonElement = button.gameObject.AddComponent<LayoutElement>();

            buttonElement.preferredWidth = 180f;
            buttonElement.preferredHeight = 64f;
            buttonElement.flexibleWidth = 0f;
            buttonElement.flexibleHeight = 0f;
        }
    }

    void OnContinue()
    {
        PermissionManager.Instance?.Request(AppPermission.Camera);
        PermissionManager.Instance?.Request(AppPermission.Photos);
        PermissionManager.Instance?.Request(AppPermission.MotionTracking);

        if (AppManager.Instance != null)
            AppManager.Instance.CompletePermissions();
        else if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene("MainShell");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainShell");
    }
}
