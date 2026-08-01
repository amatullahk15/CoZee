using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MeasureDistance : MonoBehaviour
{
    public TextMeshProUGUI distanceText;

    ARRaycastManager raycastManager;
    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    Vector3 firstPoint;
    bool hasFirstPoint = false;

    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();

        distanceText.text = "Tap 2 points";
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    Vector3 hitPos = hits[0].pose.position;

                    if (!hasFirstPoint)
                    {
                        firstPoint = hitPos;
                        hasFirstPoint = true;

                        distanceText.text = "First point selected";
                    }
                    else
                    {
                        float distance = Vector3.Distance(firstPoint, hitPos);

                        distanceText.text = "Distance: " + distance.ToString("F2") + " m";

                        hasFirstPoint = false;
                    }
                }
            }
        }
    }
}