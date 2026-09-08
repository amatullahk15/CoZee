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
            button.onClick.AddListener(OpenDropdown);

        EnsureDropdownIcon();
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

    void OpenDropdown()
    {
        OptionDropdownMenu.Toggle(button != null ? button.transform : transform, options, currentIndex, SelectOption);
    }

    void SelectOption(int index)
    {
        if (options == null || index < 0 || index >= options.Length)
            return;

        currentIndex = index;
        UpdateLabel();
    }

    void UpdateLabel()
    {
        if (labelText != null && options != null && options.Length > 0)
        {
            labelText.text = options[currentIndex];
            labelText.enableWordWrapping = false;
            labelText.overflowMode = TextOverflowModes.Ellipsis;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
            labelText.rectTransform.anchorMin = new Vector2(0.10f, 0f);
            labelText.rectTransform.anchorMax = new Vector2(0.76f, 1f);
            labelText.rectTransform.offsetMin = Vector2.zero;
            labelText.rectTransform.offsetMax = Vector2.zero;
        }
    }

    void EnsureDropdownIcon()
    {
        if (button == null || button.transform.Find("DropdownIcon") != null)
            return;

        GameObject iconObject = new GameObject("DropdownIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        iconObject.transform.SetParent(button.transform, false);
        TextMeshProUGUI icon = iconObject.GetComponent<TextMeshProUGUI>();
        icon.font = FontAwesomeIcons.FontAsset;
        icon.text = "\uf0d7";
        icon.fontSize = 12f;
        icon.color = new Color(0.075f, 0.118f, 0.145f, 1f);
        icon.alignment = TextAlignmentOptions.Center;
        icon.raycastTarget = false;
        icon.rectTransform.anchorMin = new Vector2(0.78f, 0f);
        icon.rectTransform.anchorMax = new Vector2(0.92f, 1f);
        icon.rectTransform.offsetMin = Vector2.zero;
        icon.rectTransform.offsetMax = Vector2.zero;
    }
}
