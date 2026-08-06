using UnityEngine;

public class FurnitureSelector : MonoBehaviour
{
    public PlaceObject placeObject;

    public GameObject sofaPrefab;
    public GameObject wardrobePrefab;

    void Awake()
    {
        EnsurePrefabs();
    }

    void Start()
    {
        EnsurePrefabs();
    }

    public void EnsurePrefabs()
    {
        if (placeObject == null)
            placeObject = FindObjectOfType<PlaceObject>();

        if (sofaPrefab == null)
            sofaPrefab = Resources.Load<GameObject>("m_sofa") ?? Resources.Load<GameObject>("Sofa");

        if (wardrobePrefab == null)
            wardrobePrefab = Resources.Load<GameObject>("m_wardrobe") ?? Resources.Load<GameObject>("Wardrobe");

#if UNITY_EDITOR
        if (sofaPrefab == null)
            sofaPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/m_sofa.prefab");

        if (wardrobePrefab == null)
            wardrobePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/m_wardrobe.prefab");
#endif
    }

    public void SelectSofa()
    {
        EnsurePrefabs();
        if (placeObject != null && sofaPrefab != null)
        {
            placeObject.objectPrefab = sofaPrefab;
            Debug.Log("[FurnitureSelector] Selected Sofa: " + sofaPrefab.name);
        }
    }

    public void SelectWardrobe()
    {
        EnsurePrefabs();
        if (placeObject != null && wardrobePrefab != null)
        {
            placeObject.objectPrefab = wardrobePrefab;
            Debug.Log("[FurnitureSelector] Selected Wardrobe: " + wardrobePrefab.name);
        }
    }
}