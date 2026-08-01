using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BudgetSlider : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TextMeshProUGUI valueText;

    public float Value => slider != null ? slider.value : 0f;

    void Awake()
    {
        if (slider == null) slider = GetComponent<Slider>();
        
        if (slider != null)
        {
            slider.onValueChanged.AddListener(UpdateText);
            UpdateText(slider.value);
        }
    }

    public void Setup(Slider sliderComponent, TextMeshProUGUI textComponent, float min, float max, float value)
    {
        slider = sliderComponent;
        valueText = textComponent;
        
        if (slider != null)
        {
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            slider.onValueChanged.RemoveListener(UpdateText);
            slider.onValueChanged.AddListener(UpdateText);
            UpdateText(slider.value);
        }
    }

    void UpdateText(float value)
    {
        if (valueText != null)
        {
            valueText.text = $"${value:N0}";
        }
    }
}
