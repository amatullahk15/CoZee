using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BottomNavBar : MonoBehaviour
{
    [System.Serializable]
    public class TabButton
    {
        public Button button;
        public Image buttonImage;
        public Image activeIndicator;
        public TextMeshProUGUI iconText;
        public TextMeshProUGUI labelText;
        public AppTab tab;
        public Color normalColor;
        public Color activeColor;
    }

    [SerializeField] TabButton[] tabs;

    static readonly Color NavSurface = new Color(0.985f, 0.973f, 0.945f, 1f);
    static readonly Color NavIdle = new Color(0.42f, 0.49f, 0.48f, 1f);
    static readonly Color NavActive = new Color(0.055f, 0.36f, 0.34f, 1f);
    static readonly Color NavActiveSurface = new Color(0.82f, 0.91f, 0.87f, 1f);

    void Start()
    {
        EnsureTabs();
        ApplyNavigationPresentation();

        if (tabs == null || tabs.Length == 0)
        {
            Debug.LogWarning("BottomNavBar: no tab buttons assigned.");
            return;
        }

        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i] == null || tabs[i].button == null)
                continue;

            AppTab tab = tabs[i].tab;
            tabs[i].button.onClick.AddListener(() => OnTabClicked(tab));
        }

        if (NavigationManager.Instance != null)
        {
            NavigationManager.Instance.OnTabChanged += UpdateHighlights;
            UpdateHighlights(NavigationManager.Instance.CurrentTab);
        }
        else
        {
            UpdateHighlights(AppTab.Home);
        }
    }

    public void EnsureTabs()
    {
        Color[] defaultNormals = new Color[] { NavSurface, NavSurface, NavSurface, NavSurface, NavSurface };
        Color[] defaultActives = new Color[] { NavActiveSurface, NavActiveSurface, NavActiveSurface, NavActiveSurface, NavActiveSurface };

        if (tabs != null && tabs.Length == 5 && tabs[0] != null && tabs[0].button != null)
        {
            for (int k = 0; k < tabs.Length; k++)
            {
                if (tabs[k] != null)
                {
                    PopulateTabReferences(tabs[k]);
                    int idx = (int)tabs[k].tab;
                    if (idx >= 0 && idx < 5)
                    {
                        if (tabs[k].normalColor.a < 0.05f) tabs[k].normalColor = defaultNormals[idx];
                        if (tabs[k].activeColor.a < 0.05f) tabs[k].activeColor = defaultActives[idx];
                    }
                }
            }
            return;
        }

        tabs = new TabButton[5];
        Button[] buttons = GetComponentsInChildren<Button>(true);

        for (int i = 0; i < buttons.Length; i++)
        {
            string name = buttons[i].name.ToLowerInvariant();
            int index = -1;

            if (name.Contains("home")) index = (int)AppTab.Home;
            else if (name.Contains("scan")) index = (int)AppTab.ScanAR;
            else if (name.Contains("design")) index = (int)AppTab.DesignAI;
            else if (name.Contains("vastu")) index = (int)AppTab.Vastu;
            else if (name.Contains("library")) index = (int)AppTab.Library;

            if (index >= 0 && index < 5)
            {
                var btn = buttons[i];
                tabs[index] = new TabButton
                {
                    button = btn,
                    tab = (AppTab)index,
                    normalColor = defaultNormals[index],
                    activeColor = defaultActives[index]
                };
                PopulateTabReferences(tabs[index]);
            }
        }
    }

    void PopulateTabReferences(TabButton tab)
    {
        if (tab == null || tab.button == null)
            return;

        if (tab.buttonImage == null)
            tab.buttonImage = tab.button.GetComponent<Image>();

        if (tab.activeIndicator == null)
            tab.activeIndicator = tab.button.transform.Find("ActiveIndicator")?.GetComponent<Image>();

        if (tab.iconText == null)
        {
            Transform icon = tab.button.transform.Find("Icon") ?? tab.button.transform.Find("PillIndicator/Icon");
            tab.iconText = icon != null ? icon.GetComponent<TextMeshProUGUI>() : null;
        }

        if (tab.labelText == null)
            tab.labelText = tab.button.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();

        LayoutElement layout = tab.button.GetComponent<LayoutElement>();
        if (layout == null)
            layout = tab.button.gameObject.AddComponent<LayoutElement>();
        layout.minWidth = 0f;
        layout.preferredWidth = 0f;
        layout.flexibleWidth = 1f;
    }

    void OnDestroy()
    {
        if (NavigationManager.Instance != null)
            NavigationManager.Instance.OnTabChanged -= UpdateHighlights;
    }

    void OnTabClicked(AppTab tab)
    {
        AudioManager.Instance?.PlayClick();
        NavigationManager.Instance?.SelectTab(tab);
    }

    public void UpdateHighlights(AppTab activeTab)
    {
        if (tabs == null || tabs.Length == 0)
            EnsureTabs();

        if (tabs == null)
            return;

        Color[] defaultNormals = new Color[] { NavSurface, NavSurface, NavSurface, NavSurface, NavSurface };
        Color[] defaultActives = new Color[] { NavActiveSurface, NavActiveSurface, NavActiveSurface, NavActiveSurface, NavActiveSurface };

        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i] == null || tabs[i].button == null)
                continue;

            int tabIdx = (int)tabs[i].tab;
            if (tabIdx < 0 || tabIdx >= 5) tabIdx = i;

            bool active = tabs[i].tab == activeTab;

            // Ensure Unity Button transition doesn't tint or darken the image
            tabs[i].button.transition = Selectable.Transition.None;

            // Fallback to vibrant default color if normalColor is zero/unassigned
            Color normColor = tabs[i].normalColor.a > 0.05f ? tabs[i].normalColor : defaultNormals[tabIdx];
            Color actColor = tabs[i].activeColor.a > 0.05f ? tabs[i].activeColor : defaultActives[tabIdx];

            // Apply unique vibrant color to each button background
            var btnImg = tabs[i].buttonImage != null ? tabs[i].buttonImage : tabs[i].button.GetComponent<Image>();
            if (btnImg != null)
            {
                btnImg.enabled = true;
                btnImg.color = active ? actColor : normColor;
            }

            // Update active top indicator bar
            var indicator = tabs[i].activeIndicator;
            if (indicator == null && tabs[i].button != null)
            {
                var indTransform = tabs[i].button.transform.Find("ActiveIndicator");
                if (indTransform != null) indicator = indTransform.GetComponent<Image>();
            }
            if (indicator != null)
            {
                indicator.gameObject.SetActive(active);
                indicator.color = NavActive;
            }

            // Update Icon
            var icon = tabs[i].iconText;
            if (icon == null && tabs[i].button != null)
            {
                var iconTransform = tabs[i].button.transform.Find("Icon") ?? tabs[i].button.transform.Find("PillIndicator/Icon");
                if (iconTransform != null) icon = iconTransform.GetComponent<TextMeshProUGUI>();
            }
            if (icon != null)
            {
                icon.color = active ? NavActive : NavIdle;
                icon.fontStyle = active ? FontStyles.Bold : FontStyles.Normal;
                SetVectorIconColor(icon.transform, active ? NavActive : NavIdle);
            }

            // Update Label
            var label = tabs[i].labelText;
            if (label == null && tabs[i].button != null)
            {
                var labelTransform = tabs[i].button.transform.Find("Label");
                if (labelTransform != null) label = labelTransform.GetComponent<TextMeshProUGUI>();
            }
            if (label != null)
            {
                label.color = active ? NavActive : NavIdle;
                label.fontStyle = active ? FontStyles.Bold : FontStyles.Normal;
            }
        }
    }

    void ApplyNavigationPresentation()
    {
        Image navBackground = GetComponent<Image>();
        if (navBackground != null)
            navBackground.color = NavSurface;

        string[] labels = { "Home", "Scan", "Design", "Vastu", "Library" };
        for (int i = 0; i < tabs.Length; i++)
        {
            TabButton tab = tabs[i];
            if (tab == null || tab.button == null)
                continue;

            if (tab.iconText != null)
            {
                tab.iconText.text = string.Empty;
                CreateVectorIcon(tab.iconText.transform, tab.tab);
            }

            if (tab.labelText != null)
            {
                tab.labelText.text = labels[i];
                tab.labelText.fontSize = 11;
                tab.labelText.enableWordWrapping = false;
            }
            else
            {
                tab.labelText = CreateLabel(tab.button.transform, labels[i]);
            }
        }
    }

    TextMeshProUGUI CreateLabel(Transform parent, string label)
    {
        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(parent, false);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0.42f);
        rect.offsetMin = new Vector2(4f, 1f);
        rect.offsetMax = new Vector2(-4f, 0f);

        TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.text = label;
        text.fontSize = 11;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        return text;
    }

    void CreateVectorIcon(Transform parent, AppTab tab)
    {
        if (parent.Find("VectorIcon") != null)
            return;

        GameObject icon = new GameObject("VectorIcon", typeof(RectTransform));
        icon.transform.SetParent(parent, false);
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(8f, 5f);
        iconRect.offsetMax = new Vector2(-8f, -5f);

        switch (tab)
        {
            case AppTab.Home:
                AddMark(icon.transform, new Vector2(0f, -3f), new Vector2(14f, 11f));
                AddMark(icon.transform, new Vector2(-5f, 7f), new Vector2(14f, 2.5f), 42f);
                AddMark(icon.transform, new Vector2(5f, 7f), new Vector2(14f, 2.5f), -42f);
                break;
            case AppTab.ScanAR:
                AddMark(icon.transform, new Vector2(-7f, 6f), new Vector2(7f, 2.5f));
                AddMark(icon.transform, new Vector2(-7f, -6f), new Vector2(7f, 2.5f));
                AddMark(icon.transform, new Vector2(7f, 6f), new Vector2(7f, 2.5f));
                AddMark(icon.transform, new Vector2(7f, -6f), new Vector2(7f, 2.5f));
                break;
            case AppTab.DesignAI:
                AddMark(icon.transform, Vector2.zero, new Vector2(11f, 11f), 45f);
                AddMark(icon.transform, new Vector2(8f, 8f), new Vector2(4f, 4f));
                break;
            case AppTab.Vastu:
                AddMark(icon.transform, Vector2.zero, new Vector2(12f, 12f), 45f);
                AddMark(icon.transform, Vector2.zero, new Vector2(18f, 2.5f));
                AddMark(icon.transform, Vector2.zero, new Vector2(2.5f, 18f));
                break;
            case AppTab.Library:
                AddMark(icon.transform, new Vector2(-6f, 0f), new Vector2(3f, 16f));
                AddMark(icon.transform, Vector2.zero, new Vector2(3f, 16f));
                AddMark(icon.transform, new Vector2(6f, 0f), new Vector2(3f, 16f));
                break;
        }
    }

    void AddMark(Transform parent, Vector2 position, Vector2 size, float rotation = 0f)
    {
        GameObject mark = new GameObject("Mark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        mark.transform.SetParent(parent, false);
        RectTransform rect = mark.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
        mark.GetComponent<Image>().color = NavIdle;
        mark.GetComponent<Image>().raycastTarget = false;
    }

    void SetVectorIconColor(Transform parent, Color color)
    {
        Transform icon = parent.Find("VectorIcon");
        if (icon == null)
            return;

        foreach (Image mark in icon.GetComponentsInChildren<Image>(true))
            mark.color = color;
    }
}


