using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuickActionButton : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] TextMeshProUGUI labelText;
    [SerializeField] AppTab targetTab = AppTab.ScanAR;

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    public void SetLabel(string label)
    {
        if (labelText != null)
            labelText.text = label;
    }

    void OnClick()
    {
        AudioManager.Instance?.PlayClick();
        NavigationManager.Instance?.SelectTab(targetTab);
    }
}
