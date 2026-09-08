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
        TryDeleteSelectedFurniture();
    }

    public bool TryDeleteSelectedFurniture()
    {
        EnsureInteraction();
        if (furnitureInteraction == null || furnitureInteraction.selectedObject == null)
            return false;

        GameObject selected = furnitureInteraction.selectedObject;
        furnitureInteraction.Deselect();
        Destroy(selected);
        return true;
    }

    void EnsureInteraction()
    {
        if (furnitureInteraction == null)
            furnitureInteraction = FindObjectOfType<FurnitureInteraction>();
    }
}
