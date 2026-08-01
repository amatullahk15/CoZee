using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DesignAIController : ScreenBase
{
    [SerializeField] CyclicSelector roomSelector;
    [SerializeField] BudgetSlider budgetSlider;
    [SerializeField] StyleChipSelector styleSelector;
    [SerializeField] TMP_InputField promptInput;
    [SerializeField] Button generateButton;
    [SerializeField] ConceptGalleryView gallery;

    protected override void OnShow()
    {
        EnsureControls();

        if (promptInput != null && string.IsNullOrEmpty(promptInput.text))
            promptInput.text = "Cozy living room with natural light";
    }

    void Start()
    {
        EnsureControls();

        if (generateButton != null)
            generateButton.onClick.AddListener(Generate);
    }

    void Awake()
    {
        EnsureControls();
    }

    void EnsureControls()
    {
        if (promptInput == null)
            promptInput = GetComponentInChildren<TMP_InputField>(true);

        if (generateButton == null)
        {
            foreach (Button button in GetComponentsInChildren<Button>(true))
            {
                if (button.gameObject.name.Contains("Generate"))
                {
                    generateButton = button;
                    break;
                }
            }
        }

        if (gallery == null)
            gallery = GetComponentInChildren<ConceptGalleryView>(true);
            
        if (roomSelector == null) roomSelector = GetComponentInChildren<CyclicSelector>(true);
        if (budgetSlider == null) budgetSlider = GetComponentInChildren<BudgetSlider>(true);
        if (styleSelector == null) styleSelector = GetComponentInChildren<StyleChipSelector>(true);
    }

    void Generate()
    {
        string prompt = promptInput != null ? promptInput.text : string.Empty;
        string style = styleSelector != null ? styleSelector.SelectedStyle : "Modern";
        string room = roomSelector != null ? roomSelector.SelectedOption : "Room";
        float budget = budgetSlider != null ? budgetSlider.Value : 25000f;

        if (string.IsNullOrWhiteSpace(prompt))
        {
            UIManager.Instance?.ShowToast("Enter a design prompt");
            return;
        }

        string finalPrompt = $"[{room} - ${budget:N0}] {prompt}";

        UIManager.Instance?.ShowLoading(true);
        DesignAIManager.Instance?.GenerateConcept(finalPrompt, style, _ =>
        {
            UIManager.Instance?.ShowLoading(false);
            UIManager.Instance?.ShowToast("Concept generated");
        });
    }
}
