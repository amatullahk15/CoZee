using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] float fadeDuration = 0.35f;

    public bool IsLoading { get; private set; }
    public string ActiveSceneName => SceneManager.GetActiveScene().name;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName, Action onComplete = null)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        StartCoroutine(LoadSceneRoutine(sceneName, LoadSceneMode.Single, onComplete));
    }

    public void LoadSceneAdditive(string sceneName, Action onComplete = null)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        StartCoroutine(LoadSceneRoutine(sceneName, LoadSceneMode.Additive, onComplete));
    }

    public void UnloadScene(string sceneName, Action onComplete = null)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        StartCoroutine(UnloadSceneRoutine(sceneName, onComplete));
    }

    IEnumerator LoadSceneRoutine(string sceneName, LoadSceneMode mode, Action onComplete)
    {
        if (IsLoading)
            yield break;

        if (string.IsNullOrEmpty(sceneName))
        {
            IsLoading = false;
            yield break;
        }

        if (mode == LoadSceneMode.Single && IsSceneLoaded(sceneName))
        {
            onComplete?.Invoke();
            yield break;
        }

        if (mode == LoadSceneMode.Additive && IsSceneLoaded(sceneName))
        {
            onComplete?.Invoke();
            yield break;
        }

        IsLoading = true;

        if (mode == LoadSceneMode.Single && SceneTransition.Instance != null)
            yield return SceneTransition.Instance.FadeOut(fadeDuration);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, mode);
        if (op == null)
        {
            Debug.LogError("SceneLoader: failed to load scene '" + sceneName
                + "'. Add it to Build Settings.");
            IsLoading = false;
            onComplete?.Invoke();
            yield break;
        }

        while (!op.isDone)
            yield return null;

        if (mode == LoadSceneMode.Single && SceneTransition.Instance != null)
            yield return SceneTransition.Instance.FadeIn(fadeDuration);

        if (mode == LoadSceneMode.Single && !IsSceneLoaded(sceneName))
        {
            Debug.LogWarning("SceneLoader: scene '" + sceneName + "' did not become active after loading.");
        }

        IsLoading = false;
        onComplete?.Invoke();
    }

    IEnumerator UnloadSceneRoutine(string sceneName, Action onComplete)
    {
        if (!IsSceneLoaded(sceneName))
        {
            onComplete?.Invoke();
            yield break;
        }

        AsyncOperation op = SceneManager.UnloadSceneAsync(sceneName);
        while (op != null && !op.isDone)
            yield return null;

        onComplete?.Invoke();
    }

    public static bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == sceneName)
                return true;
        }

        return false;
    }
}
