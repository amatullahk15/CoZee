using UnityEngine;

public class FurnitureRotation : MonoBehaviour
{
    public FurnitureInteraction furnitureInteraction;

    public bool rotateRight = false;
    public bool rotateLeft = false;

    void Update()
    {
        if (furnitureInteraction.selectedObject != null)
        {
            if (rotateRight)
            {
                furnitureInteraction.selectedObject.transform.Rotate(0, 100 * Time.deltaTime, 0);
            }

            if (rotateLeft)
            {
                furnitureInteraction.selectedObject.transform.Rotate(0, -100 * Time.deltaTime, 0);
            }
        }
    }

    public void StartRotateRight()
    {
        rotateRight = true;
    }

    public void StopRotateRight()
    {
        rotateRight = false;
    }

    public void StartRotateLeft()
    {
        rotateLeft = true;
    }

    public void StopRotateLeft()
    {
        rotateLeft = false;
    }
}