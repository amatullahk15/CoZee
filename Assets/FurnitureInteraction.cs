using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

public class FurnitureInteraction : MonoBehaviour
{
    public GameObject selectedObject;
    public GameObject selectionRing;

    public float scaleSpeed = 0.001f;
    public float minScale = 0.1f;
    public float maxScale = 0.5f;

    private ARRaycastManager raycastManager;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();

        if (selectionRing != null)
            selectionRing.SetActive(false);
    }

    void Update()
    {
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(touch.position);

        RaycastHit hitObject;

        // SELECT OBJECT
        if (touch.phase == TouchPhase.Began)
        {
            if (Physics.Raycast(ray, out hitObject))
            {
                if (hitObject.collider.CompareTag("Furniture"))
                {
                    selectedObject = hitObject.collider.gameObject;

                    if (selectionRing != null)
                    {
                        selectionRing.SetActive(true);
                        selectionRing.transform.position =
                            selectedObject.transform.position + new Vector3(0, -0.4f, 0);
                    }
                }
            }
            else
            {
                selectedObject = null;

                selectionRing.SetActive(false);
            }
        }

        // MOVE OBJECT
        if (selectedObject != null &&
            touch.phase == TouchPhase.Moved &&
            Input.touchCount == 1)
        {
            if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;

                selectedObject.transform.position = hitPose.position;

                if (selectionRing != null)
                    selectionRing.transform.position =
                        selectedObject.transform.position + new Vector3(0, -0.4f, 0);
            }
        }

        // SCALE OBJECT
        if (selectedObject != null && Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            Vector2 touch1PrevPos =
                touch1.position - touch1.deltaPosition;

            Vector2 touch2PrevPos =
                touch2.position - touch2.deltaPosition;

            float prevMagnitude =
                (touch1PrevPos - touch2PrevPos).magnitude;

            float currentMagnitude =
                (touch1.position - touch2.position).magnitude;

            float difference =
                currentMagnitude - prevMagnitude;

            Vector3 currentScale =
                selectedObject.transform.localScale;

            float scaleChange =
                difference * scaleSpeed;

            float newScale =
                Mathf.Clamp(
                    currentScale.x + scaleChange,
                    minScale,
                    maxScale
                );

            selectedObject.transform.localScale =
                new Vector3(newScale, newScale, newScale);
        }
    }
}