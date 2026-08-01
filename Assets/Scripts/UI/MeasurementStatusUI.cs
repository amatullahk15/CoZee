using UnityEngine;
using TMPro;

public class MeasurementStatusUI : MonoBehaviour
{
    [SerializeField] ARSessionBridge bridge;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] TextMeshProUGUI dimText;

    RoomMeasurement roomMeasurement;

    void Update()
    {
        if (bridge == null)
            bridge = FindObjectOfType<ARSessionBridge>();

        if (roomMeasurement == null)
            roomMeasurement = FindObjectOfType<RoomMeasurement>();

        if (statusText != null && bridge != null)
            statusText.text = bridge.GetStatusText();

        if (dimText != null && roomMeasurement != null && roomMeasurement.distanceText != null)
        {
            dimText.text = roomMeasurement.distanceText.text;
        }
    }
}
