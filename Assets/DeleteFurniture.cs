using UnityEngine;

public class DeleteFurniture : MonoBehaviour
{
    public FurnitureInteraction furnitureInteraction;

    public void DeleteSelectedFurniture()
    {
        if (furnitureInteraction.selectedObject != null)
        {
            Destroy(furnitureInteraction.selectedObject);

            furnitureInteraction.selectedObject = null;

            furnitureInteraction.selectionRing.SetActive(false);
        }
    }
}