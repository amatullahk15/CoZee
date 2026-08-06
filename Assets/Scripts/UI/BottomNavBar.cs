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

    // Distinct theme colors for each of the 5 navigation tabs
    static readonly Color ColorHome = new Color(0.086f, 0.420f, 0.408f, 1f);       // #166B68 Teal
    static readonly Color ColorHomeActive = new Color(0.110f, 0.522f, 0.506f, 1f);

    static readonly Color ColorScan = new Color(0.141f, 0.341f, 0.773f, 1f);       // #2457C5 Blue
    static readonly Color ColorScanActive = new Color(0.188f, 0.427f, 0.941f, 1f);

    static readonly Color ColorDesign = new Color(0.773f, 0.231f, 0.231f, 1f);     // #C53B3B Red
    static readonly Color ColorDesignActive = new Color(0.878f, 0.275f, 0.275f, 1f);

    static readonly Color ColorVastu = new Color(0.710f, 0.294f, 0.796f, 1f);      // #B54BCB Purple
    static readonly Color ColorVastuActive = new Color(0.784f, 0.353f, 0.878f, 1f);

    static readonly Color ColorLibrary = new Color(0.235f, 0.478f, 0.176f, 1f);    // #3C7A2D Green
    static readonly Color ColorLibraryActive = new Color(0.282f, 0.580f, 0.212f, 1f);

    void Start()
    {
        EnsureTabs();

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
        Color[] defaultNormals = new Color[] { ColorHome, ColorScan, ColorDesign, ColorVastu, ColorLibrary };
        Color[] defaultActives = new Color[] { ColorHomeActive, ColorScanActive, ColorDesignActive, ColorVastuActive, ColorLibraryActive };

        if (tabs != null && tabs.Length == 5 && tabs[0] != null && tabs[0].button != null)
        {
            for (int k = 0; k < tabs.Length; k++)
            {
                if (tabs[k] != null)
                {
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
                var indGo = btn.transform.Find("ActiveIndicator");
                Image indImg = indGo != null ? indGo.GetComponent<Image>() : null;

                var iconGo = btn.transform.Find("Icon") ?? btn.transform.Find("PillIndicator/Icon");
                TextMeshProUGUI iconTmp = iconGo != null ? iconGo.GetComponent<TextMeshProUGUI>() : null;

                var labelGo = btn.transform.Find("Label");
                TextMeshProUGUI labelTmp = labelGo != null ? labelGo.GetComponent<TextMeshProUGUI>() : null;

                tabs[index] = new TabButton
                {
                    button = btn,
                    buttonImage = btn.GetComponent<Image>(),
                    activeIndicator = indImg,
                    iconText = iconTmp,
                    labelText = labelTmp,
                    tab = (AppTab)index,
                    normalColor = defaultNormals[index],
                    activeColor = defaultActives[index]
                };
            }
        }
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

        Color[] defaultNormals = new Color[] { ColorHome, ColorScan, ColorDesign, ColorVastu, ColorLibrary };
        Color[] defaultActives = new Color[] { ColorHomeActive, ColorScanActive, ColorDesignActive, ColorVastuActive, ColorLibraryActive };

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
                indicator.color = Color.white;
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
                icon.color = Color.white;
                icon.fontStyle = active ? FontStyles.Bold : FontStyles.Normal;
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
                label.color = active ? Color.white : new Color(1f, 1f, 1f, 0.90f);
                label.fontStyle = active ? FontStyles.Bold : FontStyles.Normal;
            }
        }
    }
}


