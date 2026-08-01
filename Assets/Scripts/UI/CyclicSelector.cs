using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CyclicSelector : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] TextMeshProUGUI labelText;
    [SerializeField] string[] options;

    int currentIndex = 0;

    public string SelectedOption => (options != null && options.Length > 0) ? options[currentIndex] : "";

    void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (labelText == null) labelText = GetComponentInChildren<TextMeshProUGUI>();

        if (button != null)
            button.onClick.AddListener(CycleNext);

        UpdateLabel();
    }

    public void Setup(string[] newOptions, string defaultOption = null)
    {
        options = newOptions;
        currentIndex = 0;
        
        if (defaultOption != null && options != null)
        {
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] == defaultOption)
                {
                    currentIndex = i;
                    break;
                }
            }
        }
        
        UpdateLabel();
    }

    void CycleNext()
    {
        if (options == null || options.Length == 0) return;
        
        currentIndex = (currentIndex + 1) % options.Length;
        UpdateLabel();
    }

    void UpdateLabel()
    {
        if (labelText != null && options != null && options.Length > 0)
        {
            labelText.text = $"{options[currentIndex]}                                                  v";
        }
    }
}
