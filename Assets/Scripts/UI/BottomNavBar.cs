using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BottomNavBar : MonoBehaviour
{
    [System.Serializable]
    public class TabButton
    {
        public Button button;
        public Image highlight;
        public AppTab tab;
    }

    [SerializeField] TabButton[] tabs;
    [SerializeField] Color activeColor = new Color(0.23f, 0.51f, 0.96f, 1f); // #3B82F6 Royal Blue
    [SerializeField] Color inactiveColor = new Color(0.12f, 0.16f, 0.23f, 0.96f); // #1E293B Slate Dark
    [SerializeField] Color activeTextColor = Color.white;
    [SerializeField] Color inactiveTextColor = new Color(0.58f, 0.64f, 0.72f, 1f); // #94A3B8 Muted Slate

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

    void EnsureTabs()
    {
        if (tabs != null && tabs.Length > 0 && tabs[0] != null && tabs[0].button != null)
            return;

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

            if (index >= 0)
            {
                tabs[index] = new TabButton
                {
                    button = buttons[i],
                    highlight = buttons[i].GetComponent<Image>(),
                    tab = (AppTab)index
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

    void UpdateHighlights(AppTab activeTab)
    {
        if (tabs == null)
            return;

        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i] == null || tabs[i].button == null)
                continue;

            bool active = tabs[i].tab == activeTab;
            // if (tabs[i].highlight != null)
            //     tabs[i].highlight.color = active ? activeColor : inactiveColor;

            // var texts = tabs[i].button.GetComponentsInChildren<TextMeshProUGUI>(true);
            // foreach (var t in texts)
            // {
            //     t.color = active ? activeTextColor : inactiveTextColor;
            // }
        }
    }
}
