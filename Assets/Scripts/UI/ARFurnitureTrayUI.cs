using UnityEngine;
using UnityEngine.UI;

public class ARFurnitureTrayUI : MonoBehaviour
{
    [SerializeField] Button sofaButton;
    [SerializeField] Button wardrobeButton;

    FurnitureSelector selector;
    bool wired;

    void Start()
    {
        TryWireButtons();
    }

    void Update()
    {
        if (selector == null)
            selector = FindObjectOfType<FurnitureSelector>();

        if (!wired)
            TryWireButtons();
    }

    void TryWireButtons()
    {
        if (sofaButton == null)
            sofaButton = FindButton("SofaBtn");
        if (wardrobeButton == null)
            wardrobeButton = FindButton("WardrobeBtn");
        if (sofaButton == null || wardrobeButton == null || wired)
            return;

        sofaButton.onClick.RemoveListener(SelectSofa);
        wardrobeButton.onClick.RemoveListener(SelectWardrobe);
        sofaButton.onClick.AddListener(SelectSofa);
        wardrobeButton.onClick.AddListener(SelectWardrobe);
        wired = true;
    }

    static Button FindButton(string objectName)
    {
        foreach (Button button in FindObjectsOfType<Button>(true))
        {
            if (button.gameObject.name == objectName)
                return button;
        }

        return null;
    }

    void SelectSofa()
    {
        if (selector == null)
            selector = FindObjectOfType<FurnitureSelector>();
        selector?.SelectSofa();
        UIManager.Instance?.ShowToast(selector != null ? "Sofa selected. Tap a detected surface to place it." : "Furniture controls are loading.");
    }

    void SelectWardrobe()
    {
        if (selector == null)
            selector = FindObjectOfType<FurnitureSelector>();
        selector?.SelectWardrobe();
        UIManager.Instance?.ShowToast(selector != null ? "Wardrobe selected. Tap a detected surface to place it." : "Furniture controls are loading.");
    }
}
