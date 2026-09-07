using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ARSessionBridge : MonoBehaviour
{
    RoomMeasurement roomMeasurement;

    public int TapCount => roomMeasurement != null ? roomMeasurement.tapCount : 0;
    public bool IsMeasurementComplete => roomMeasurement != null && roomMeasurement.HasCompletedScan;
    public bool IsScanning => roomMeasurement != null && roomMeasurement.IsScanning;
    public bool CanStartScan => roomMeasurement != null && roomMeasurement.CanStartScan();
    public bool CanPlaceFurniture => roomMeasurement != null && roomMeasurement.CanPlaceFurniture;

    void Start()
    {
        EnsureRenovisionComponents();
        EnsureMeasurement();
    }

    void Update()
    {
        EnsureMeasurement();
    }

    void EnsureMeasurement()
    {
        if (roomMeasurement == null)
            roomMeasurement = FindObjectOfType<RoomMeasurement>();
    }

    void EnsureRenovisionComponents()
    {
        var detector = FindObjectOfType<RenoVisionDetector>();
        if (detector == null)
        {
            var detectorObj = new GameObject("RenoVisionDetector");
            detectorObj.AddComponent<RenoVisionDetector>();
        }

        var labeler = FindObjectOfType<FurnitureLabeler>();
        if (labeler == null)
        {
            var labelerObj = new GameObject("FurnitureLabeler");
            labelerObj.AddComponent<FurnitureLabeler>();
        }
    }

    public string GetStatusText()
    {
        if (roomMeasurement == null)
            return "Starting AR session…";

        return roomMeasurement.GetStatusText();
    }

    public string GetDimensionsText()
    {
        if (roomMeasurement == null)
            return "Searching for surfaces...";

        return roomMeasurement.GetMeasurementsText();
    }

    public RoomScanData BuildRoomScanData()
    {
        if (roomMeasurement == null || !roomMeasurement.HasCompletedScan)
            return null;

        return roomMeasurement.BuildRoomScanData();
    }

    public List<ScannedSurfaceRecord> CreateScanHistorySnapshot()
    {
        if (roomMeasurement == null)
            return new List<ScannedSurfaceRecord>();

        return roomMeasurement.CreateScanHistorySnapshot();
    }

    public bool StartScan()
    {
        if (roomMeasurement == null)
            return false;

        roomMeasurement.StartScan();
        return roomMeasurement.IsScanning;
    }

    public bool StopScan()
    {
        if (roomMeasurement == null)
            return false;

        return roomMeasurement.StopScan();
    }

    public string GetPrimaryActionLabel()
    {
        if (roomMeasurement == null)
            return "Searching...";

        if (roomMeasurement.IsScanning)
            return "Stop Scan";

        if (roomMeasurement.HasCompletedScan && !roomMeasurement.CanStartScan())
            return "Save Room";

        return roomMeasurement.HasCompletedScan ? "Scan Next Surface" : "Start Scan";
    }
}
