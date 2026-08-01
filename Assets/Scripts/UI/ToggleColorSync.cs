using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToggleColorSync : MonoBehaviour
{
    [SerializeField] Toggle toggle;
    [SerializeField] TextMeshProUGUI labelText;
    
    Color activeBg = new Color(0.7f, 0.8f, 1f, 1f);
    Color activeText = Color.black;
    Color inactiveBg = new Color(0.20f, 0.25f, 0.33f, 0.96f);
    Color inactiveText = Color.white;

    void Awake()
    {
        if (toggle == null) toggle = GetComponent<Toggle>();
        if (labelText == null) labelText = GetComponentInChildren<TextMeshProUGUI>();

        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(OnToggle);
        }
    }

    public void Setup(Toggle t, TextMeshProUGUI text)
    {
        toggle = t;
        labelText = text;
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(OnToggle);
            toggle.onValueChanged.AddListener(OnToggle);
        }
    }

    void OnToggle(bool isOn)
    {
        if (toggle != null && toggle.targetGraphic != null)
        {
            toggle.targetGraphic.color = isOn ? activeBg : inactiveBg;
        }
        
        if (labelText != null)
        {
            labelText.color = isOn ? activeText : inactiveText;
        }
    }
}
