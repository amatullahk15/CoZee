using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class RoomMeasurement : MonoBehaviour
{
    public TextMeshProUGUI distanceText;

    private ARRaycastManager raycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private Vector3 point1;
    private Vector3 point2;
    private Vector3 point3;

    // Made public so PlaceObject can access it
    public int tapCount = 0;

    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();

        if (distanceText != null)
            distanceText.text = "Tap first corner";
    }

    void Update()
    {
        // Stop measurement after 3 taps
        if (tapCount >= 3)
            return;

        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase != TouchPhase.Began)
            return;

        if (raycastManager != null && raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
        {
            Vector3 hitPos = hits[0].pose.position;

            tapCount++;

            if (tapCount == 1)
            {
                point1 = hitPos;

                if (distanceText != null)
                    distanceText.text = "Tap second corner";
            }
            else if (tapCount == 2)
            {
                point2 = hitPos;

                float width = Vector3.Distance(point1, point2);

                if (distanceText != null)
                    distanceText.text =
                        "Width: " + width.ToString("F2") + " m\nTap third corner";
            }
            else if (tapCount == 3)
            {
                point3 = hitPos;

                float width = Vector3.Distance(point1, point2);

                float length = Vector3.Distance(point2, point3);

                if (distanceText != null)
                    distanceText.text =
                        "Room Size:\n" +
                        "Width: " + width.ToString("F2") + " m\n" +
                        "Length: " + length.ToString("F2") + " m\n\n" +
                        "Now tap to place furniture";
            }
        }
    }
}