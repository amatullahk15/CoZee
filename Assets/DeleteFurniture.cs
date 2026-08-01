using UnityEngine;

public class DeleteFurniture : MonoBehaviour
{
    public FurnitureInteraction furnitureInteraction;

    public void DeleteSelectedFurniture()
    {
        if (furnitureInteraction == null || furnitureInteraction.selectedObject == null)
            return;

        Destroy(furnitureInteraction.selectedObject);

        furnitureInteraction.selectedObject = null;

        if (furnitureInteraction.selectionRing != null)
            furnitureInteraction.selectionRing.SetActive(false);
    }
}