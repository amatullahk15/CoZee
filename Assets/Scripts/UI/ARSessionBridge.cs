using UnityEngine;
using TMPro;

public class ARSessionBridge : MonoBehaviour
{
    RoomMeasurement roomMeasurement;

    public int TapCount => roomMeasurement != null ? roomMeasurement.tapCount : 0;
    public bool IsMeasurementComplete => TapCount >= 3;

    void Update()
    {
        if (roomMeasurement == null)
            roomMeasurement = FindObjectOfType<RoomMeasurement>();
    }

    public string GetStatusText()
    {
        if (roomMeasurement == null)
            return "Starting AR session…";

        switch (roomMeasurement.tapCount)
        {
            case 0: return "Tap first corner";
            case 1: return "Tap second corner";
            case 2: return "Tap third corner";
            default: return "Room measured — tap to place furniture";
        }
    }
}
