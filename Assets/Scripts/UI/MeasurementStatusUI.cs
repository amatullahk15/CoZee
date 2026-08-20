using UnityEngine;
using TMPro;

public class MeasurementStatusUI : MonoBehaviour
{
    [SerializeField] ARSessionBridge bridge;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] TextMeshProUGUI dimText;

    void Update()
    {
        if (bridge == null)
            bridge = FindObjectOfType<ARSessionBridge>();

        if (statusText != null && bridge != null)
            statusText.text = bridge.GetStatusText();

        if (dimText != null && bridge != null)
            dimText.text = bridge.GetDimensionsText();
    }
}
