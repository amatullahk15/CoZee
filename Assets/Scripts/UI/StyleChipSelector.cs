using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StyleChipSelector : MonoBehaviour
{
    [SerializeField] Toggle[] styleToggles;
    [SerializeField] string[] styleNames;

    static readonly Color Mint = new Color(0.82f, 0.91f, 0.87f, 1f);
    static readonly Color Sand = new Color(0.93f, 0.86f, 0.73f, 1f);
    static readonly Color Ink = new Color(0.075f, 0.118f, 0.145f, 1f);
    TextMeshProUGUI compactLabel;
    Button compactButton;

    void Awake()
    {
        WireToggles();
        RefreshPresentation();
    }

    void OnEnable()
    {
        RefreshPresentation();
    }

    void OnDestroy()
    {
        if (styleToggles == null)
            return;

        foreach (Toggle toggle in styleToggles)
            if (toggle != null)
                toggle.onValueChanged.RemoveListener(OnStyleChanged);
    }

    void WireToggles()
    {
        if (styleToggles == null)
            return;

        foreach (Toggle toggle in styleToggles)
        {
            if (toggle == null)
                continue;
            toggle.onValueChanged.RemoveListener(OnStyleChanged);
            toggle.onValueChanged.AddListener(OnStyleChanged);
        }
    }

    void OnStyleChanged(bool _)
    {
        RefreshPresentation();
    }

    public void EnableCompactSelector()
    {
        if (styleToggles == null)
            return;

        foreach (Toggle toggle in styleToggles)
        {
            if (toggle != null)
            {
                Transform row = toggle.transform.parent;
                if (row != null)
                    row.gameObject.SetActive(false);
            }
        }

        Transform existing = transform.Find("CompactStyleSelector");
        Button button = existing != null ? existing.GetComponent<Button>() : null;
        if (button == null)
        {
            GameObject selector = new GameObject("CompactStyleSelector", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            selector.transform.SetParent(transform, false);
            selector.GetComponent<Image>().color = Mint;
            selector.GetComponent<LayoutElement>().preferredHeight = 48f;
            button = selector.GetComponent<Button>();
            compactButton = button;
            button.onClick.AddListener(OpenDropdown);

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(selector.transform, false);
            compactLabel = labelObject.GetComponent<TMPro.TextMeshProUGUI>();
            compactLabel.font = CoZeeTypography.FontFor("Style", 13f, FontStyles.Bold);
            compactLabel.fontSize = 13f;
            compactLabel.fontStyle = FontStyles.Bold;
            compactLabel.color = Ink;
            compactLabel.alignment = TextAlignmentOptions.MidlineLeft;
            compactLabel.enableWordWrapping = false;
            compactLabel.overflowMode = TextOverflowModes.Ellipsis;
            compactLabel.raycastTarget = false;
            RectTransform labelRect = compactLabel.rectTransform;
            labelRect.anchorMin = new Vector2(0.10f, 0f);
            labelRect.anchorMax = new Vector2(0.76f, 1f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            GameObject iconObject = new GameObject("DropdownIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            iconObject.transform.SetParent(selector.transform, false);
            TextMeshProUGUI icon = iconObject.GetComponent<TextMeshProUGUI>();
            icon.font = FontAwesomeIcons.FontAsset;
            icon.text = "\uf0d7";
            icon.fontSize = 12f;
            icon.color = Ink;
            icon.alignment = TextAlignmentOptions.Center;
            icon.raycastTarget = false;
            icon.rectTransform.anchorMin = new Vector2(0.78f, 0f);
            icon.rectTransform.anchorMax = new Vector2(0.92f, 1f);
            icon.rectTransform.offsetMin = Vector2.zero;
            icon.rectTransform.offsetMax = Vector2.zero;
        }
        else if (compactLabel == null)
        {
            compactLabel = button.GetComponentInChildren<TextMeshProUGUI>(true);
            compactButton = button;
        }

        RefreshCompactLabel();
    }

    void OpenDropdown()
    {
        if (styleToggles == null || styleToggles.Length == 0)
            return;

        OptionDropdownMenu.Toggle(compactButton != null ? compactButton.transform : transform, styleNames, GetSelectedIndex(), SelectOption);
    }

    int GetSelectedIndex()
    {
        int selectedIndex = 0;
        for (int i = 0; i < styleToggles.Length; i++)
            if (styleToggles[i] != null && styleToggles[i].isOn)
                selectedIndex = i;

        return selectedIndex;
    }

    void SelectOption(int index)
    {
        if (styleToggles == null || index < 0 || index >= styleToggles.Length || styleToggles[index] == null)
            return;

        // The original style toggles are not guaranteed to share a ToggleGroup.
        // Set the complete state explicitly so only the chosen style can be active.
        for (int i = 0; i < styleToggles.Length; i++)
        {
            if (styleToggles[i] != null)
                styleToggles[i].SetIsOnWithoutNotify(i == index);
        }

        RefreshPresentation();
        RefreshCompactLabel();
    }

    void RefreshPresentation()
    {
        if (styleToggles == null)
            return;

        foreach (Toggle toggle in styleToggles)
        {
            if (toggle == null)
                continue;

            Image background = toggle.GetComponent<Image>();
            if (background != null)
                background.color = toggle.isOn ? Sand : Mint;

            foreach (var label in toggle.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
            {
                label.color = Ink;
                label.fontStyle = TMPro.FontStyles.Bold;
                label.enableWordWrapping = false;
                label.overflowMode = TMPro.TextOverflowModes.Ellipsis;
            }
        }

        RefreshCompactLabel();
    }

    void RefreshCompactLabel()
    {
        if (compactLabel != null)
            compactLabel.text = SelectedStyle;
    }

    public string SelectedStyle
    {
        get
        {
            if (styleToggles == null || styleNames == null)
                return "Modern";

            for (int i = 0; i < styleToggles.Length && i < styleNames.Length; i++)
            {
                if (styleToggles[i] != null && styleToggles[i].isOn)
                    return styleNames[i];
            }

            return styleNames.Length > 0 ? styleNames[0] : "Modern";
        }
    }
}
