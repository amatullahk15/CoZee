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
        ApplyMobileTextSafety();
        ArrangeSelectionCards();

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

    void ApplyMobileTextSafety()
    {
        foreach (TextMeshProUGUI text in GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.text != "Design AI")
                continue;

            text.fontSize = 30f;
            text.enableAutoSizing = true;
            text.fontSizeMin = 22f;
            text.fontSizeMax = 30f;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            break;
        }
    }

    void ArrangeSelectionCards()
    {
        if (roomSelector == null || styleSelector == null)
            return;

        Transform roomCard = roomSelector.transform.parent;
        Transform styleCard = styleSelector.transform;
        if (roomCard == null || roomCard.parent == null || roomCard.parent != styleCard.parent)
            return;

        if (roomCard.parent.name == "DesignSelectionRow")
        {
            ConfigureSelectionCard(roomCard);
            ConfigureSelectionCard(styleCard);
            styleSelector.EnableCompactSelector();
            return;
        }

        Transform form = roomCard.parent;
        Transform row = form.Find("DesignSelectionRow");
        if (row == null)
        {
            GameObject rowObject = new GameObject("DesignSelectionRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            rowObject.transform.SetParent(form, false);
            rowObject.transform.SetSiblingIndex(roomCard.GetSiblingIndex());
            HorizontalLayoutGroup layout = rowObject.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            rowObject.GetComponent<LayoutElement>().preferredHeight = 98f;
            row = rowObject.transform;
        }

        roomCard.SetParent(row, false);
        styleCard.SetParent(row, false);
        ConfigureSelectionCard(roomCard);
        ConfigureSelectionCard(styleCard);
        styleSelector.EnableCompactSelector();
    }

    static void ConfigureSelectionCard(Transform card)
    {
        LayoutElement element = card.GetComponent<LayoutElement>();
        if (element == null)
            element = card.gameObject.AddComponent<LayoutElement>();
        element.minWidth = 0f;
        element.preferredWidth = 0f;
        element.flexibleWidth = 1f;
        element.preferredHeight = 98f;

        Image image = card.GetComponent<Image>();
        if (image != null)
            image.color = new Color(1f, 0.992f, 0.965f, 1f);

        VerticalLayoutGroup layout = card.GetComponent<VerticalLayoutGroup>();
        if (layout != null)
        {
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 6f;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
        }
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
