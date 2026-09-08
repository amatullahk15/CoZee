using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class RoomMeasurement : MonoBehaviour
{
    const float FloorNormalMinimum = 0.85f;
    const float WallNormalMaximum = 0.35f;
    const float MinimumSurfaceAreaSquareMeters = 0.12f;
    const float MinimumSurfaceDimensionMeters = 0.25f;
    const float MinimumScanSeconds = 0.5f;

    public TextMeshProUGUI distanceText;

    ARPlaneManager planeManager;
    ARRaycastManager raycastManager;
    Camera cachedCamera;
    readonly List<ScannedSurfaceRecord> scanHistory = new List<ScannedSurfaceRecord>();
    readonly List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();

    // A session is intentionally locked to one plane so a later scan cannot reuse a prior surface.
    ARPlane scannedPlane;
    float scanStartedAt;
    bool startButtonArmed;
    int wallCount;
    int floorCount;

    // Made public so PlaceObject can access it
    public int tapCount = 0;
    public bool IsScanning { get; private set; }
    public bool HasDetectedSurface => GetTargetedPlane() != null;
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
            UpdateScannedPlane();

        RefreshStatusText();
    }

    void EnsureComponents()
    {
        if (planeManager == null)
            planeManager = GetComponent<ARPlaneManager>() ?? FindObjectOfType<ARPlaneManager>();

        if (raycastManager == null)
            raycastManager = GetComponent<ARRaycastManager>() ?? FindObjectOfType<ARRaycastManager>();

        if (cachedCamera == null)
            cachedCamera = Camera.main ?? Camera.current;
    }

    public bool CanStartScan()
    {
        return !IsScanning && GetTargetedPlane() != null;
    }

    public void StartScan()
    {
        if (!CanStartScan())
            return;

        scannedPlane = ResolveCurrentPlane(GetTargetedPlane());
        if (scannedPlane == null)
            return;

        IsScanning = true;
        scanStartedAt = Time.time;
        RefreshStatusText();
    }

    public bool StopScan()
    {
        if (!IsScanning)
            return false;

        IsScanning = false;

        scannedPlane = ResolveCurrentPlane(scannedPlane);
        if (!IsUsablePlane(scannedPlane)
            || !IsLargeEnough(scannedPlane)
            || Time.time - scanStartedAt < MinimumScanSeconds)
        {
            scannedPlane = null;
            RefreshStatusText();
            return false;
        }

        // The tracked plane's final extent belongs only to this start/stop session.
        Vector2 size = scannedPlane.size;
        float primary = size.x;
        float secondary = size.y;
        float area = Mathf.Max(0f, size.x * size.y);

        bool isWall = IsWall(scannedPlane);
        string surfaceType = isWall ? "Wall" : "Floor";
        string label = isWall
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
        scannedPlane = null;

        RefreshStatusText();
        return true;
    }

    public string GetStatusText()
    {
        if (IsScanning)
        {
            if (scannedPlane == null)
                return "Tracking was lost. Aim the center marker at the same wall or floor.";

            string surface = IsWall(scannedPlane) ? "wall" : "floor";
            return IsLargeEnough(scannedPlane) && Time.time - scanStartedAt >= MinimumScanSeconds
                ? $"Scanning this {surface}. Tap Stop Scan to save its dimensions."
                : $"Scanning this {surface}. Keep the camera steady until the surface is fully tracked.";
        }

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
            string secondLabel = record.surfaceType == "Wall" ? "Height" : "Length";
            lines.Add($"{record.label}: Width {record.primaryDimensionMeters:F2}m | {secondLabel} {record.secondaryDimensionMeters:F2}m");
        }

        return string.Join("\n", lines);
    }

    public List<ScannedSurfaceRecord> CreateScanHistorySnapshot()
    {
        var snapshot = new List<ScannedSurfaceRecord>(scanHistory.Count);
        foreach (ScannedSurfaceRecord record in scanHistory)
        {
            snapshot.Add(new ScannedSurfaceRecord
            {
                label = record.label,
                surfaceType = record.surfaceType,
                primaryDimensionMeters = record.primaryDimensionMeters,
                secondaryDimensionMeters = record.secondaryDimensionMeters,
                areaSquareMeters = record.areaSquareMeters
            });
        }

        return snapshot;
    }

    public RoomScanData BuildRoomScanData(string roomId = null)
    {
        var data = new RoomScanData();
        data.roomId = string.IsNullOrEmpty(roomId) ? Guid.NewGuid().ToString() : roomId;
        data.surfaces = CreateScanHistorySnapshot();

        foreach (ScannedSurfaceRecord record in data.surfaces)
        {
            if (string.Equals(record.surfaceType, "Wall", StringComparison.OrdinalIgnoreCase))
            {
                data.wallCount++;
                data.totalWallAreaSquareMeters += record.areaSquareMeters;
            }
            else if (string.Equals(record.surfaceType, "Floor", StringComparison.OrdinalIgnoreCase))
            {
                data.floorCount++;
                data.totalFloorAreaSquareMeters += record.areaSquareMeters;
            }
        }

        data.summary = BuildRoomScanSummary(data);
        return data;
    }

    static string BuildRoomScanSummary(RoomScanData data)
    {
        if (data == null)
            return "No surfaces scanned";

        var parts = new List<string>(3);
        if (data.wallCount > 0)
            parts.Add($"{data.wallCount} wall{(data.wallCount == 1 ? "" : "s")}");
        if (data.floorCount > 0)
            parts.Add($"{data.floorCount} floor{(data.floorCount == 1 ? "" : "s")}");

        float totalArea = data.totalWallAreaSquareMeters + data.totalFloorAreaSquareMeters;
        if (totalArea > 0f)
            parts.Add($"{totalArea:F2} m2");

        return parts.Count > 0 ? string.Join(" • ", parts) : "No surfaces scanned";
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

            if (!IsUsablePlane(plane))
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

    ARPlane GetTargetedPlane()
    {
        if (raycastManager != null)
        {
            raycastHits.Clear();
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            if (raycastManager.Raycast(screenCenter, raycastHits, TrackableType.PlaneWithinPolygon))
            {
                foreach (ARRaycastHit hit in raycastHits)
                {
                    ARPlane plane = ResolveCurrentPlane(hit.trackable as ARPlane);
                    if (IsUsablePlane(plane))
                        return plane;
                }
            }
        }

        return GetBestVisiblePlane();
    }

    void UpdateScannedPlane()
    {
        // AR Foundation can subsume a small patch into its parent as tracking improves.
        scannedPlane = ResolveCurrentPlane(scannedPlane);
    }

    static ARPlane ResolveCurrentPlane(ARPlane plane)
    {
        while (plane != null && plane.subsumedBy != null)
            plane = plane.subsumedBy;
        return plane;
    }

    bool IsUsablePlane(ARPlane plane)
    {
        return plane != null
            && plane.trackingState == TrackingState.Tracking
            && (IsFloor(plane) || IsWall(plane));
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

    static bool IsFloor(ARPlane plane)
    {
        return plane != null
            && Mathf.Abs(Vector3.Dot(plane.transform.up.normalized, Vector3.up)) >= FloorNormalMinimum;
    }

    static bool IsWall(ARPlane plane)
    {
        return plane != null
            && Mathf.Abs(Vector3.Dot(plane.transform.up.normalized, Vector3.up)) <= WallNormalMaximum;
    }

    static bool IsLargeEnough(ARPlane plane)
    {
        if (plane == null)
            return false;

        Vector2 size = plane.size;
        return size.x >= MinimumSurfaceDimensionMeters
            && size.y >= MinimumSurfaceDimensionMeters
            && size.x * size.y >= MinimumSurfaceAreaSquareMeters;
    }

    void RefreshStatusText()
    {
        if (distanceText == null)
            return;

        distanceText.text = GetMeasurementsText();
    }
}
