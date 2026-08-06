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

        // First complete room measurement if active
        if (roomMeasurement != null && roomMeasurement.tapCount < 3)
            return;

        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

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

        // Ensure objectPrefab is assigned and is not an AR Default Plane prefab
        if (objectPrefab != null && objectPrefab.name.Contains("Default Plane"))
        {
            objectPrefab = null;
        }

        if (objectPrefab == null)
        {
            var selector = FindObjectOfType<FurnitureSelector>();
            if (selector != null)
            {
                selector.EnsurePrefabs();
                if (selector.sofaPrefab != null) objectPrefab = selector.sofaPrefab;
                else if (selector.wardrobePrefab != null) objectPrefab = selector.wardrobePrefab;
            }
        }

        if (objectPrefab == null)
            return;

        TrackableType trackableTypes = TrackableType.PlaneWithinPolygon | TrackableType.PlaneEstimated | TrackableType.FeaturePoint;

        if (raycastManager != null && raycastManager.Raycast(touch.position, hits, trackableTypes))
        {
            Pose hitPose = hits[0].pose;

            GameObject obj = Instantiate(objectPrefab, hitPose.position, hitPose.rotation);
            obj.SetActive(true);
            obj.tag = "Furniture";

            if (obj.transform.localScale == Vector3.one)
            {
                obj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            }

            if (obj.GetComponent<Collider>() == null)
            {
                obj.AddComponent<BoxCollider>();
            }

            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = new Color(
                    Random.value,
                    Random.value,
                    Random.value
                );
            }
        }
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