using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomDirectionSelector : MonoBehaviour
{
    [SerializeField] Button northButton;
    [SerializeField] Button eastButton;
    [SerializeField] Button southButton;
    [SerializeField] Button westButton;
    [SerializeField] TextMeshProUGUI selectedLabel;

    void Start()
    {
        EnsureButtons();

        SetDirection(VastuAssistantManager.Instance != null
            ? VastuAssistantManager.Instance.SelectedDirection
            : RoomDirection.North);
    }

    void EnsureButtons()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            string name = btn.gameObject.name.ToLowerInvariant();
            btn.onClick.RemoveAllListeners();

            if (name.Contains("north"))
            {
                btn.onClick.AddListener(() => SetDirection(RoomDirection.North));
            }
            else if (name.Contains("east"))
            {
                btn.onClick.AddListener(() => SetDirection(RoomDirection.East));
            }
            else if (name.Contains("south"))
            {
                btn.onClick.AddListener(() => SetDirection(RoomDirection.South));
            }
            else if (name.Contains("west"))
            {
                btn.onClick.AddListener(() => SetDirection(RoomDirection.West));
            }
        }

        if (selectedLabel == null)
        {
            selectedLabel = GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    Button CreateDirectionButton(string name, string label, Vector2 anchoredPosition)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(transform, false);
        go.GetComponent<Image>().color = new Color(0.25f, 0.45f, 0.85f, 1f);

        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(120f, 56f);
        rect.anchoredPosition = anchoredPosition;

        var textGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(go.transform, false);
        var tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 22f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        Stretch(textGo.GetComponent<RectTransform>());

        return go.GetComponent<Button>();
    }

    void SetDirection(RoomDirection direction)
    {
        VastuAssistantManager.Instance?.SetDirection(direction);

        if (selectedLabel != null)
            selectedLabel.text = "Room faces: " + direction;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
