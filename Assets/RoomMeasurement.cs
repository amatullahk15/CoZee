using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[Serializable]
public class ScannedSurfaceRecord
{
    public string label;
    public string surfaceType;
    public float primaryDimensionMeters;
    public float secondaryDimensionMeters;
    public float areaSquareMeters;
}

public class RoomMeasurement : MonoBehaviour
{
    public TextMeshProUGUI distanceText;

    ARPlaneManager planeManager;
    Camera cachedCamera;
    readonly List<ScannedSurfaceRecord> scanHistory = new List<ScannedSurfaceRecord>();

    ARPlane bestPlaneDuringScan;
    float bestPlaneScore;
    bool startButtonArmed;
    int wallCount;
    int floorCount;

    // Made public so PlaceObject can access it
    public int tapCount = 0;
    public bool IsScanning { get; private set; }
    public bool HasDetectedSurface => GetBestVisiblePlane() != null;
    public bool HasCompletedScan => scanHistory.Count > 0;
    public bool CanPlaceFurniture => HasCompletedScan && !IsScanning;
    public IReadOnlyList<ScannedSurfaceRecord> ScanHistory => scanHistory;

    void Start()
    {
        EnsureComponents();
        RefreshStatusText();
    }

    void Update()
    {
        EnsureComponents();

        if (!startButtonArmed && HasDetectedSurface)
            startButtonArmed = true;

        if (IsScanning)
            UpdateBestPlaneCandidate();

        RefreshStatusText();
    }

    void EnsureComponents()
    {
        if (planeManager == null)
            planeManager = GetComponent<ARPlaneManager>() ?? FindObjectOfType<ARPlaneManager>();

        if (cachedCamera == null)
            cachedCamera = Camera.main ?? Camera.current;
    }

    public bool CanStartScan()
    {
        return !IsScanning && HasDetectedSurface;
    }

    public void StartScan()
    {
        if (!CanStartScan())
            return;

        IsScanning = true;
        bestPlaneDuringScan = null;
        bestPlaneScore = 0f;
        UpdateBestPlaneCandidate();
        RefreshStatusText();
    }

    public bool StopScan()
    {
        if (!IsScanning)
            return false;

        IsScanning = false;

        if (bestPlaneDuringScan == null)
        {
            RefreshStatusText();
            return false;
        }

        Vector2 size = bestPlaneDuringScan.size;
        float primary = Mathf.Max(size.x, size.y);
        float secondary = Mathf.Min(size.x, size.y);
        float area = Mathf.Max(0f, size.x * size.y);

        bool isVertical = IsVertical(bestPlaneDuringScan.alignment);
        string surfaceType = isVertical ? "Wall" : "Floor";
        string label = isVertical
            ? $"Wall {++wallCount}"
            : $"Floor {++floorCount}";

        scanHistory.Add(new ScannedSurfaceRecord
        {
            label = label,
            surfaceType = surfaceType,
            primaryDimensionMeters = primary,
            secondaryDimensionMeters = secondary,
            areaSquareMeters = area
        });

        // Preserve the previous gate expected by placement scripts.
        tapCount = 3;

        RefreshStatusText();
        return true;
    }

    public string GetStatusText()
    {
        if (IsScanning)
            return "Scanning current surface... tap Stop Scan when the wall or floor is fully mapped.";

        if (!startButtonArmed)
            return "Move the camera until a wall or floor is detected.";

        if (!HasCompletedScan)
            return "Surface detected. Tap Start Scan to capture wall 1 or the floor.";

        return "Scan another wall/floor or place furniture on the measured area.";
    }

    public string GetMeasurementsText()
    {
        if (scanHistory.Count == 0)
            return IsScanning
                ? "Capturing dimensions..."
                : "No completed scans yet.";

        var lines = new List<string>(scanHistory.Count);
        foreach (ScannedSurfaceRecord record in scanHistory)
        {
            string dimensionNames = record.surfaceType == "Wall"
                ? "width x height"
                : "width x length";

            lines.Add(
                $"{record.label}: {record.primaryDimensionMeters:F2}m x {record.secondaryDimensionMeters:F2}m ({dimensionNames})");
        }

        return string.Join("\n", lines);
    }

    ARPlane GetBestVisiblePlane()
    {
        if (planeManager == null)
            return null;

        ARPlane bestPlane = null;
        float bestScore = 0f;

        foreach (ARPlane plane in planeManager.trackables)
        {
            if (plane == null || plane.trackingState != TrackingState.Tracking)
                continue;

            if (!IsSupportedAlignment(plane.alignment))
                continue;

            float score = ScorePlane(plane);
            if (score > bestScore)
            {
                bestScore = score;
                bestPlane = plane;
            }
        }

        return bestPlane;
    }

    void UpdateBestPlaneCandidate()
    {
        ARPlane visiblePlane = GetBestVisiblePlane();
        if (visiblePlane == null)
            return;

        float score = ScorePlane(visiblePlane);
        if (bestPlaneDuringScan == null || score >= bestPlaneScore)
        {
            bestPlaneDuringScan = visiblePlane;
            bestPlaneScore = score;
        }
    }

    float ScorePlane(ARPlane plane)
    {
        if (plane == null)
            return 0f;

        if (cachedCamera == null)
            cachedCamera = Camera.main ?? Camera.current;

        float area = Mathf.Max(0.01f, plane.size.x * plane.size.y);
        if (cachedCamera == null)
            return area;

        Vector3 worldCenter = plane.transform.TransformPoint(plane.center);
        Vector3 toPlane = worldCenter - cachedCamera.transform.position;
        float distance = Mathf.Max(0.25f, toPlane.magnitude);

        Vector3 planeNormal = plane.transform.up.normalized;
        float facing = Mathf.Abs(Vector3.Dot(cachedCamera.transform.forward.normalized, -planeNormal));

        return area * Mathf.Lerp(0.35f, 1.25f, facing) / distance;
    }

    bool IsSupportedAlignment(PlaneAlignment alignment)
    {
        return IsVertical(alignment)
            || alignment == PlaneAlignment.HorizontalDown
            || alignment == PlaneAlignment.HorizontalUp;
    }

    bool IsVertical(PlaneAlignment alignment)
    {
        return alignment == PlaneAlignment.Vertical;
    }

    void RefreshStatusText()
    {
        if (distanceText == null)
            return;

        distanceText.text = GetMeasurementsText();
    }
}
