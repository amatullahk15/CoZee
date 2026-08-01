using System.Collections.Generic;
using UnityEngine;
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
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        if (roomMeasurement == null || objectPrefab == null || raycastManager == null || Camera.main == null)
            return;

        // First complete room measurement
        if (roomMeasurement.tapCount < 3)
            return;

        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);
        if (UnityEngine.EventSystems.EventSystem.current
    .IsPointerOverGameObject(touch.fingerId))
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(touch.position);

        RaycastHit hitObject;

        if (Physics.Raycast(ray, out hitObject))
        {
            // User touched existing furniture
            return;
        }

        if (touch.phase != TouchPhase.Began)
            return;

        if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            Collider[] nearbyObjects = Physics.OverlapSphere(hitPose.position, 0.5f);

            int furnitureCount = 0;

            foreach (Collider collider in nearbyObjects)
            {
                if (collider.CompareTag("Furniture"))
                {
                    furnitureCount++;
                }
            }

            if (furnitureCount > 0)
            {
                return;
            }

            GameObject obj = Instantiate(objectPrefab, hitPose.position, hitPose.rotation);

            // Smaller cube size
            obj.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            // Random color
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
}