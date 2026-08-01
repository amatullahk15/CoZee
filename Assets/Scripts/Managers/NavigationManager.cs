using System;
using UnityEngine;

public enum AppTab
{
    Home = 0,
    ScanAR = 1,
    DesignAI = 2,
    Vastu = 3,
    Library = 4
}

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager Instance { get; private set; }

    public event Action<AppTab> OnTabChanged;

    [SerializeField] ScreenBase[] tabScreens;
    [SerializeField] AppTab defaultTab = AppTab.Home;

    AppTab currentTab;
    bool initialized;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (this.tabScreens != null && this.tabScreens.Length > 0 && this.tabScreens[0] != null)
            {
                Instance.SetTabScreens(this.tabScreens);
            }
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureTabScreens();
    }

    public void SetTabScreens(ScreenBase[] screens)
    {
        if (screens != null && screens.Length > 0 && screens[0] != null)
        {
            tabScreens = screens;
            if (initialized)
            {
                SelectTab(currentTab, notify: true);
            }
        }
    }

    public void EnsureTabScreens()
    {
        if (tabScreens != null && tabScreens.Length >= 5 && tabScreens[0] != null)
            return;

        var home = FindObjectOfType<HomeDashboardController>(true);
        var scan = FindObjectOfType<ScanARScreenController>(true);
        var design = FindObjectOfType<DesignAIController>(true);
        var vastu = FindObjectOfType<VastuScreenController>(true);
        var library = FindObjectOfType<LibraryScreenController>(true);

        if (home != null && scan != null && design != null && vastu != null && library != null)
        {
            tabScreens = new ScreenBase[] { home, scan, design, vastu, library };
            return;
        }

        var found = GetComponentsInChildren<ScreenBase>(true);
        if (found != null && found.Length >= 5)
        {
            tabScreens = found;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        EnsureTabScreens();

        if (initialized)
            return;

        initialized = true;

        int saved = UserPreferences.LastTabIndex;
        AppTab startTab = Enum.IsDefined(typeof(AppTab), saved)
            ? (AppTab)saved
            : defaultTab;

        SelectTab(startTab, notify: false);
    }

    public void SelectTab(int tabIndex) => SelectTab((AppTab)tabIndex);

    public void SelectTab(AppTab tab, bool notify = true)
    {
        EnsureTabScreens();

        if (tabScreens == null || tabScreens.Length == 0)
        {
            Debug.LogError("NavigationManager: tabScreens is not assigned.");
            return;
        }

        currentTab = tab;
        UserPreferences.LastTabIndex = (int)tab;
        UserPreferences.Save();

        for (int i = 0; i < tabScreens.Length; i++)
        {
            if (tabScreens[i] == null)
                continue;

            if (i == (int)tab)
                tabScreens[i].Show();
            else
                tabScreens[i].Hide();
        }

        if (notify)
            OnTabChanged?.Invoke(tab);
    }

    public AppTab CurrentTab => currentTab;
}
