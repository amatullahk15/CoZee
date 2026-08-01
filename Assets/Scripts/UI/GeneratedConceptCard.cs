using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GeneratedConceptCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI promptText;
    [SerializeField] Button saveButton;

    DesignConcept boundConcept;

    void Awake()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(Save);
    }

    public void Bind(DesignConcept concept)
    {
        boundConcept = concept;

        if (titleText != null)
            titleText.text = concept.style;

        if (promptText != null)
            promptText.text = concept.prompt;
    }

    void Save()
    {
        UIManager.Instance?.ShowToast("Design saved to Library");
    }
}
