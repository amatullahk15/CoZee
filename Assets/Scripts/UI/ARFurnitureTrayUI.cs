using UnityEngine;
using UnityEngine.UI;

public class ARFurnitureTrayUI : MonoBehaviour
{
    [SerializeField] Button sofaButton;
    [SerializeField] Button wardrobeButton;

    FurnitureSelector selector;

    void Start()
    {
        if (sofaButton != null)
            sofaButton.onClick.AddListener(SelectSofa);

        if (wardrobeButton != null)
            wardrobeButton.onClick.AddListener(SelectWardrobe);
    }

    void Update()
    {
        if (selector == null)
            selector = FindObjectOfType<FurnitureSelector>();
    }

    void SelectSofa()
    {
        selector?.SelectSofa();
        UIManager.Instance?.ShowToast("Sofa selected");
    }

    void SelectWardrobe()
    {
        selector?.SelectWardrobe();
        UIManager.Instance?.ShowToast("Wardrobe selected");
    }
}
