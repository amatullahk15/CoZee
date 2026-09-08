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
            StyleSlider();
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

    void StyleSlider()
    {
        Image fill = slider.fillRect != null ? slider.fillRect.GetComponent<Image>() : null;
        Image handle = slider.handleRect != null ? slider.handleRect.GetComponent<Image>() : null;
        Image background = slider.targetGraphic as Image;

        if (background != null)
            background.color = new Color(0.31f, 0.38f, 0.40f, 1f);
        if (fill != null)
            fill.color = new Color(0.055f, 0.36f, 0.34f, 1f);
        if (handle != null)
            handle.color = new Color(0.93f, 0.86f, 0.73f, 1f);
    }
}
