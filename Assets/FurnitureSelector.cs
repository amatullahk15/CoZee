using UnityEngine;

public class FurnitureSelector : MonoBehaviour
{
    public PlaceObject placeObject;

    public GameObject sofaPrefab;
    public GameObject wardrobePrefab;

    public void SelectSofa()
    {
        placeObject.objectPrefab = sofaPrefab;
    }

    public void SelectWardrobe()
    {
        placeObject.objectPrefab = wardrobePrefab;
    }
}