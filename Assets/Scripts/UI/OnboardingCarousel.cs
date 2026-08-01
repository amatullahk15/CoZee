using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OnboardingCarousel : MonoBehaviour
{
    [SerializeField] OnboardingManager onboardingManager;
    [SerializeField] OnboardingSlideView slideView;
    [SerializeField] Button nextButton;
    [SerializeField] Button skipButton;
    [SerializeField] TextMeshProUGUI nextButtonLabel;
    [SerializeField] Transform dotsContainer;
    [SerializeField] GameObject dotPrefab;

    Image[] dots;

    void Awake()
    {
        if (onboardingManager == null)
            onboardingManager = FindObjectOfType<OnboardingManager>();

        if (slideView == null)
            slideView = GetComponentInChildren<OnboardingSlideView>(true);

        if (nextButton == null || skipButton == null)
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (Button button in buttons)
            {
                if (button.gameObject.name.Contains("Next") && nextButton == null)
                    nextButton = button;
                if (button.gameObject.name.Contains("Skip") && skipButton == null)
                    skipButton = button;
            }
        }

        if (nextButtonLabel == null && nextButton != null)
            nextButtonLabel = nextButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {
        if (onboardingManager != null)
        {
            onboardingManager.OnSlideChanged += Refresh;
            BuildDots(onboardingManager.SlideCount);
            Refresh(onboardingManager.CurrentIndex);
        }

        if (nextButton != null)
        {
            nextButton.interactable = true;
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => onboardingManager?.Next());
        }

        if (skipButton != null)
        {
            skipButton.interactable = true;
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(() => onboardingManager?.Skip());
        }
    }

    void OnDestroy()
    {
        if (onboardingManager != null)
            onboardingManager.OnSlideChanged -= Refresh;
    }

    void BuildDots(int count)
    {
        if (dotsContainer == null || dotPrefab == null || count <= 0)
            return;

        dots = new Image[count];
        for (int i = 0; i < count; i++)
        {
            GameObject dot = Instantiate(dotPrefab, dotsContainer, false);
            dot.SetActive(true);
            dot.transform.localScale = Vector3.one;

            var image = dot.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(1f, 1f, 1f, 0.35f);
                image.raycastTarget = false;
            }

            dots[i] = image;
        }
    }

    void Refresh(int index)
    {
        if (onboardingManager != null && slideView != null)
            slideView.Bind(onboardingManager.GetSlide(index));

        if (dots != null)
        {
            for (int i = 0; i < dots.Length; i++)
            {
                if (dots[i] != null)
                    dots[i].color = i == index ? Color.white : new Color(1f, 1f, 1f, 0.35f);
            }
        }

        if (nextButtonLabel != null && onboardingManager != null)
        {
            bool last = index >= onboardingManager.SlideCount - 1;
            nextButtonLabel.text = last ? "Get Started" : "Next";
        }
    }
}
