using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A focused confirmation dialog that protects Library records from accidental deletion.
public class LibraryDeleteConfirmationDialog : MonoBehaviour
{
    static readonly Color Ink = new Color(0.075f, 0.118f, 0.145f, 1f);
    static readonly Color CardCream = new Color(1f, 0.992f, 0.965f, 1f);
    static readonly Color TealSoft = new Color(0.82f, 0.91f, 0.87f, 1f);
    static readonly Color DangerRed = new Color(0.94f, 0.27f, 0.27f, 1f);

    static LibraryDeleteConfirmationDialog current;

    Action onConfirm;

    public static void Show(string itemTitle, Action confirmed)
    {
        if (current != null)
            Destroy(current.gameObject);

        Canvas canvas = FindShellCanvas();
        if (canvas == null)
        {
            Debug.LogWarning("Library delete confirmation could not find the main UI canvas.");
            return;
        }

        GameObject overlay = new GameObject("LibraryDeleteConfirmation", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LibraryDeleteConfirmationDialog));
        overlay.transform.SetParent(canvas.transform, false);
        overlay.transform.SetAsLastSibling();

        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlay.GetComponent<Image>().color = new Color(0.02f, 0.04f, 0.06f, 0.70f);

        current = overlay.GetComponent<LibraryDeleteConfirmationDialog>();
        current.onConfirm = confirmed;
        current.Build(itemTitle);
    }

    void Build(string itemTitle)
    {
        GameObject dialog = CreatePanel(transform, "Dialog", CardCream);
        RectTransform dialogRect = dialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.08f, 0.36f);
        dialogRect.anchorMax = new Vector2(0.92f, 0.64f);
        dialogRect.offsetMin = Vector2.zero;
        dialogRect.offsetMax = Vector2.zero;

        AddText(dialog.transform, "Title", "Delete saved item?", 24f, FontStyles.Bold, Ink,
            new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.90f), TextAlignmentOptions.MidlineLeft);

        string description = "This will permanently remove " + itemTitle + ".";
        AddText(dialog.transform, "Message", description, 15f, FontStyles.Normal, new Color(Ink.r, Ink.g, Ink.b, 0.78f),
            new Vector2(0.08f, 0.40f), new Vector2(0.92f, 0.65f), TextAlignmentOptions.TopLeft);

        Button cancel = AddButton(dialog.transform, "Cancel", "Cancel", TealSoft, Ink,
            new Vector2(0.08f, 0.11f), new Vector2(0.47f, 0.31f));
        cancel.onClick.AddListener(Close);

        Button delete = AddButton(dialog.transform, "ConfirmDelete", "Delete", DangerRed, Color.white,
            new Vector2(0.53f, 0.11f), new Vector2(0.92f, 0.31f));
        delete.onClick.AddListener(Confirm);
    }

    void Confirm()
    {
        Action confirmed = onConfirm;
        Close();
        confirmed?.Invoke();
    }

    void Close()
    {
        if (current == this)
            current = null;

        Destroy(gameObject);
    }

    static Canvas FindShellCanvas()
    {
        GameObject background = GameObject.Find("CanvasBackground");
        if (background != null)
        {
            Canvas shellCanvas = background.GetComponentInParent<Canvas>();
            if (shellCanvas != null)
                return shellCanvas;
        }

        return FindObjectOfType<Canvas>(true);
    }

    static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    static TextMeshProUGUI AddText(Transform parent, string name, string value, float size, FontStyles style, Color color,
        Vector2 anchorMin, Vector2 anchorMax, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = CoZeeTypography.FontFor(name, size, style);
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        return text;
    }

    static Button AddButton(Transform parent, string name, string label, Color background, Color textColor,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject buttonObject = CreatePanel(parent, name, background);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Button button = buttonObject.AddComponent<Button>();
        AddText(buttonObject.transform, "Label", label, 16f, FontStyles.Bold, textColor,
            Vector2.zero, Vector2.one, TextAlignmentOptions.Center);
        return button;
    }
}
