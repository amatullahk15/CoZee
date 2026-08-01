using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DesignAIController : ScreenBase
{
    [SerializeField] TMP_InputField promptInput;
    [SerializeField] StyleChipSelector styleSelector;
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
    }

    void Generate()
    {
        string prompt = promptInput != null ? promptInput.text : string.Empty;
        string style = styleSelector != null ? styleSelector.SelectedStyle : "Modern";

        if (string.IsNullOrWhiteSpace(prompt))
        {
            UIManager.Instance?.ShowToast("Enter a design prompt");
            return;
        }

        UIManager.Instance?.ShowLoading(true);
        DesignAIManager.Instance?.GenerateConcept(prompt, style, _ =>
        {
            UIManager.Instance?.ShowLoading(false);
            UIManager.Instance?.ShowToast("Concept generated");
        });
    }
}
