using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceObject : MonoBehaviour
{
    public GameObject objectPrefab;

    // Reference to RoomMeasurement script
    public RoomMeasurement roomMeasurement;

    ARRaycastManager raycastManager;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        EnsureComponents();
    }

    void EnsureComponents()
    {
        if (raycastManager == null)
            raycastManager = GetComponent<ARRaycastManager>() ?? FindObjectOfType<ARRaycastManager>();

        if (roomMeasurement == null)
            roomMeasurement = GetComponent<RoomMeasurement>() ?? FindObjectOfType<RoomMeasurement>();
    }

    void Update()
    {
        EnsureComponents();

        // First complete at least one controlled surface scan before placing furniture.
        if (roomMeasurement != null && !roomMeasurement.CanPlaceFurniture)
            return;

        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        // Tray and control buttons must never also count as a placement tap.
        if (IsTouchOverInteractiveUI(touch))
            return;

        Camera cam = Camera.main ?? Camera.current;
        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(touch.position);
        RaycastHit hitObject;

        if (Physics.Raycast(ray, out hitObject) && hitObject.transform != null && hitObject.transform.CompareTag("Furniture"))
        {
            // User touched existing furniture
            return;
        }

        if (touch.phase != TouchPhase.Began)
            return;

        // A selected object is being edited; do not create a duplicate while the user is dragging or deselecting it.
        FurnitureInteraction interaction = FindObjectOfType<FurnitureInteraction>();
        if (interaction != null && interaction.selectedObject != null)
            return;

        // Ensure objectPrefab is assigned and is not an AR Default Plane prefab
        if (objectPrefab != null && objectPrefab.name.Contains("Default Plane"))
        {
            objectPrefab = null;
        }

        if (objectPrefab == null)
        {
            UIManager.Instance?.ShowToast("Choose Sofa or Wardrobe first.");
            return;
        }

        // Furniture belongs on AR planes, not unstable feature points.
        TrackableType trackableTypes = TrackableType.PlaneWithinPolygon;

        if (raycastManager != null && raycastManager.Raycast(touch.position, hits, trackableTypes))
        {
            Pose hitPose = hits[0].pose;

            GameObject obj = Instantiate(objectPrefab, hitPose.position, hitPose.rotation);
            obj.SetActive(true);
            MarkFurnitureHierarchy(obj.transform);

            // Imported models use different authoring scales (the Sofa is authored at 10x).
            // Normalize the visible footprint instead of relying on a particular prefab scale.
            ScaleToFurnitureFootprint(obj, 1.6f);

            EnsureCollider(obj);

            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = new Color(
                    Random.value,
                    Random.value,
                    Random.value
                );
            }

            // A furniture choice creates one object. The user must choose it again for another placement.
            objectPrefab = null;
            UIManager.Instance?.ShowToast("Furniture placed. Double-tap it to edit.");
        }
    }

    static void ScaleToFurnitureFootprint(GameObject obj, float targetHorizontalSizeMeters)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        float largestHorizontalDimension = Mathf.Max(bounds.size.x, bounds.size.z);
        if (largestHorizontalDimension <= 0.001f)
            return;

        float multiplier = targetHorizontalSizeMeters / largestHorizontalDimension;
        obj.transform.localScale *= multiplier;
    }

    static void MarkFurnitureHierarchy(Transform root)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            child.tag = "Furniture";
    }

    static void EnsureCollider(GameObject obj)
    {
        if (obj.GetComponentInChildren<Collider>(true) != null)
            return;

        BoxCollider collider = obj.AddComponent<BoxCollider>();
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        collider.center = obj.transform.InverseTransformPoint(bounds.center);
        collider.size = obj.transform.InverseTransformVector(bounds.size);
    }

    bool IsTouchOverInteractiveUI(Touch touch)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = touch.position;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == null) continue;

            // Only block placement if touch is directly on interactive UI components
            if (result.gameObject.GetComponentInParent<Button>() != null ||
                result.gameObject.GetComponentInParent<Slider>() != null ||
                result.gameObject.GetComponentInParent<Toggle>() != null ||
                result.gameObject.GetComponentInParent<TMP_Dropdown>() != null ||
                result.gameObject.GetComponentInParent<BottomNavBar>() != null)
            {
                return true;
            }
        }

        return false;
    }
}
