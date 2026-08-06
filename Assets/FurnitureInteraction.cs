using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        EnsureComponents();
    }

    void EnsureComponents()
    {
        if (raycastManager == null)
            raycastManager = GetComponent<ARRaycastManager>() ?? FindObjectOfType<ARRaycastManager>();

        EnsureSelectionRing();
    }

    public void EnsureSelectionRing()
    {
        if (selectionRing == null)
        {
            var ringGo = GameObject.Find("SelectionRing");
            if (ringGo == null)
            {
                ringGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ringGo.name = "SelectionRing";
                ringGo.transform.localScale = new Vector3(0.8f, 0.005f, 0.8f);

                ringGo.tag = "Untagged";
                var collider = ringGo.GetComponent<Collider>();
                if (collider != null) DestroyImmediate(collider);

                var renderer = ringGo.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = new Material(Shader.Find("Sprites/Default"));
                    renderer.material.color = new Color(0.2f, 0.6f, 1.0f, 0.5f);
                }
            }
            selectionRing = ringGo;
            selectionRing.tag = "Untagged";
        }

        if (selectionRing != null && selectedObject == null)
            selectionRing.SetActive(false);
    }

    public void SelectObject(GameObject obj)
    {
        EnsureSelectionRing();

        if (obj == null)
        {
            Deselect();
            return;
        }

        selectedObject = obj.transform.root != null ? obj.transform.root.gameObject : obj;

        if (selectionRing != null)
        {
            selectionRing.SetActive(true);
            UpdateSelectionRingPosition();
        }
    }

    public void Deselect()
    {
        selectedObject = null;
        if (selectionRing != null)
            selectionRing.SetActive(false);
    }

    void UpdateSelectionRingPosition()
    {
        if (selectedObject == null || selectionRing == null) return;

        Bounds bounds = GetFurnitureBounds(selectedObject);
        Vector3 ringPos = new Vector3(selectedObject.transform.position.x, bounds.min.y + 0.005f, selectedObject.transform.position.z);
        selectionRing.transform.position = ringPos;

        float ringRadius = Mathf.Max(bounds.size.x, bounds.size.z) * 1.2f;
        ringRadius = Mathf.Max(ringRadius, 0.4f);
        selectionRing.transform.localScale = new Vector3(ringRadius, 0.005f, ringRadius);
    }

    Bounds GetFurnitureBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return new Bounds(obj.transform.position, Vector3.one * 0.3f);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    void Update()
    {
        EnsureComponents();

        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        if (IsTouchOverInteractiveUI(touch))
            return;

        Camera cam = Camera.main ?? Camera.current;
        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(touch.position);
        RaycastHit hitObject;

        // 1. SELECT / DESELECT OBJECT ON TAP
        if (touch.phase == TouchPhase.Began)
        {
            if (Physics.Raycast(ray, out hitObject))
            {
                Transform hitTransform = hitObject.collider.transform;
                if (hitTransform.CompareTag("Furniture") || hitTransform.root.CompareTag("Furniture"))
                {
                    SelectObject(hitTransform.root.gameObject);
                }
                else
                {
                    Deselect();
                }
            }
            else
            {
                Deselect();
            }
        }

        // 2. DRAG / MOVE OBJECT ON FLOOR
        if (selectedObject != null && touch.phase == TouchPhase.Moved && Input.touchCount == 1)
        {
            TrackableType trackableTypes = TrackableType.PlaneWithinPolygon | TrackableType.PlaneEstimated | TrackableType.FeaturePoint;
            if (raycastManager != null && raycastManager.Raycast(touch.position, hits, trackableTypes))
            {
                Pose hitPose = hits[0].pose;
                selectedObject.transform.position = hitPose.position;
                UpdateSelectionRingPosition();
            }
        }

        // 3. PINCH TO SCALE OBJECT
        if (selectedObject != null && Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
            Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

            float prevMagnitude = (touch1PrevPos - touch2PrevPos).magnitude;
            float currentMagnitude = (touch1.position - touch2.position).magnitude;
            float difference = currentMagnitude - prevMagnitude;

            Vector3 currentScale = selectedObject.transform.localScale;
            float scaleChange = difference * scaleSpeed;
            float newScale = Mathf.Clamp(currentScale.x + scaleChange, minScale, maxScale);

            selectedObject.transform.localScale = new Vector3(newScale, newScale, newScale);
            UpdateSelectionRingPosition();
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