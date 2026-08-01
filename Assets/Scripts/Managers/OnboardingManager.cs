using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class OnboardingSlideData
{
    public string title;
    [TextArea] public string body;
    public Sprite image;
    public string icon;
}

public class OnboardingManager : MonoBehaviour
{
    public static OnboardingManager Instance { get; private set; }

    public event Action<int> OnSlideChanged;
    public event Action OnCompleted;

    [SerializeField] OnboardingSlideData[] slides;

    int currentIndex;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (slides == null || slides.Length == 0)
        {
            slides = new[]
            {
                new OnboardingSlideData
                {
                    title = "3D Room Measurement",
                    body = "Measure floor areas, walls, and captured room dimensions in real-time with AR.",
                    icon = "📐"
                },
                new OnboardingSlideData
                {
                    title = "AR Furniture Placement",
                    body = "Preview 3D sofas, wardrobes, and decor items in high fidelity inside your home.",
                    icon = "🛋️"
                },
                new OnboardingSlideData
                {
                    title = "AI & Vastu Assistant",
                    body = "Generate instant room design themes and consult smart Vastu directional layout guidance.",
                    icon = "✨"
                }
            };
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        WireButtons();
        GoToSlide(0);
    }

    public int SlideCount => slides != null ? slides.Length : 0;
    public int CurrentIndex => currentIndex;

    public OnboardingSlideData GetSlide(int index)
    {
        if (slides == null || index < 0 || index >= slides.Length)
            return null;

        return slides[index];
    }

    public void GoToSlide(int index)
    {
        if (slides == null || slides.Length == 0)
            return;

        currentIndex = Mathf.Clamp(index, 0, slides.Length - 1);
        BindCurrentSlide();
        OnSlideChanged?.Invoke(currentIndex);
    }

    public void Next()
    {
        if (slides == null || slides.Length == 0)
            return;

        if (currentIndex >= slides.Length - 1)
        {
            Complete();
            return;
        }

        GoToSlide(currentIndex + 1);
    }

    public void Skip()
    {
        Complete();
    }

    void Complete()
    {
        OnCompleted?.Invoke();

        if (AppManager.Instance != null)
        {
            AppManager.Instance.CompleteOnboarding();
            return;
        }

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene("Permissions");
            return;
        }

        SceneManager.LoadScene("Permissions");
    }

    void WireButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);
        foreach (Button button in buttons)
        {
            if (button == null)
                continue;

            if (button.name.Contains("Next"))
            {
                button.interactable = true;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Next);
            }
            else if (button.name.Contains("Skip"))
            {
                button.interactable = true;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Skip);
            }
        }
    }

    void BindCurrentSlide()
    {
        OnboardingSlideView slideView = FindObjectOfType<OnboardingSlideView>(true);
        if (slideView != null)
            slideView.Bind(GetSlide(currentIndex));
    }
}
