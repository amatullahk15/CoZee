using UnityEngine;

public class DeleteFurniture : MonoBehaviour
{
    public FurnitureInteraction furnitureInteraction;

    void Awake()
    {
        EnsureInteraction();
    }

    public void DeleteSelectedFurniture()
    {
        EnsureInteraction();
        if (furnitureInteraction == null || furnitureInteraction.selectedObject == null)
            return;

        Destroy(furnitureInteraction.selectedObject);

        furnitureInteraction.selectedObject = null;

        if (furnitureInteraction.selectionRing != null)
            furnitureInteraction.selectionRing.SetActive(false);
    }

    void EnsureInteraction()
    {
        if (furnitureInteraction == null)
            furnitureInteraction = FindObjectOfType<FurnitureInteraction>();
    }
}
