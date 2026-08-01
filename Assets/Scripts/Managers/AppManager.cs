using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-200)]
public class AppManager : MonoBehaviour
{
    public static AppManager Instance { get; private set; }

    [Header("Scene Names")]
    public string splashScene = "SplashScreen";
    public string onboardingScene = "Onboarding";
    public string permissionsScene = "Permissions";
    public string mainShellScene = "MainShell";

    bool started;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureRequiredManagers();
    }

    void Start()
    {
        if (started)
            return;

        started = true;
        EnsureRequiredManagers();

        if (SceneLoader.Instance == null)
        {
            Debug.LogWarning("AppManager: SceneLoader missing on Bootstrap object; falling back to direct scene loading.");
            SceneManager.LoadScene(splashScene);
            return;
        }

        SceneLoader.Instance.LoadScene(splashScene);
    }

    public void RouteAfterSplash()
    {
        if (SceneLoader.Instance == null)
        {
            string targetScene = !UserPreferences.IsOnboardingDone
                ? onboardingScene
                : !UserPreferences.IsPermissionsDone ? permissionsScene : mainShellScene;
            SceneManager.LoadScene(targetScene);
            return;
        }

        if (!UserPreferences.IsOnboardingDone)
        {
            SceneLoader.Instance.LoadScene(onboardingScene);
            return;
        }

        if (!UserPreferences.IsPermissionsDone)
        {
            SceneLoader.Instance.LoadScene(permissionsScene);
            return;
        }

        SceneLoader.Instance.LoadScene(mainShellScene);
    }

    public void CompleteOnboarding()
    {
        UserPreferences.IsOnboardingDone = true;
        UserPreferences.Save();

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(permissionsScene);
            return;
        }

        SceneManager.LoadScene(permissionsScene);
    }

    public void CompletePermissions()
    {
        UserPreferences.IsPermissionsDone = true;
        UserPreferences.Save();

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(mainShellScene);
            return;
        }

        SceneManager.LoadScene(mainShellScene);
    }

    static void EnsureRequiredManagers()
    {
        EnsureManager<SceneLoader>("SceneLoader");
        EnsureManager<PermissionManager>("PermissionManager");
        EnsureManager<LibraryDataManager>("LibraryDataManager");
        EnsureManager<DesignAIManager>("DesignAIManager");
        EnsureManager<VastuAssistantManager>("VastuAssistantManager");
        EnsureManager<AudioManager>("AudioManager");
    }

    static void EnsureManager<T>(string name) where T : Component
    {
        if (FindObjectOfType<T>() != null)
            return;

        GameObject go = new GameObject(name);
        go.AddComponent<T>();
    }
}
