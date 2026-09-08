using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class RuntimeUIFactory
{
    static readonly Color SurfaceDark = new Color(1f, 0.992f, 0.965f, 1f);
    static readonly Color SurfaceElevated = new Color(0.82f, 0.91f, 0.87f, 1f);
    static readonly Color PrimaryBlue = new Color(0.93f, 0.86f, 0.73f, 1f);
    static readonly Color DangerRed = new Color(0.94f, 0.27f, 0.27f, 1f); // #EF4444
    static readonly Color TextWhite = new Color(0.075f, 0.118f, 0.145f, 1f);
    static readonly Color TextMuted = new Color(0.30f, 0.38f, 0.40f, 1f);

    public static GeneratedConceptCard CreateConceptCard(Transform parent)
    {
        var go = CreatePanel(parent, "ConceptCard", SurfaceDark);
        var cardElem = go.AddComponent<LayoutElement>();
        cardElem.flexibleHeight = 1f;

        var csf = go.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var layout = go.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 16, 16);
        layout.spacing = 10f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;

        var title = CreateText(go.transform, "Title", "Modern Living Room Concept", 22, FontStyles.Bold, TextWhite);
        var titleElem = title.gameObject.AddComponent<LayoutElement>();
        titleElem.preferredHeight = 26f;

        var prompt = CreateText(go.transform, "Prompt", "Prompt description...", 16, FontStyles.Normal, TextMuted);
        var promptElem = prompt.gameObject.AddComponent<LayoutElement>();
        promptElem.flexibleHeight = 1f;

        var saveBtn = CreateButton(go.transform, "Save", "Save to Library", PrimaryBlue, TextWhite, 48f);
        var saveElem = saveBtn.GetComponent<LayoutElement>();
        if (saveElem == null) saveElem = saveBtn.AddComponent<LayoutElement>();
        saveElem.preferredHeight = 48f;

        var card = go.AddComponent<GeneratedConceptCard>();
        SetField(card, "titleText", title);
        SetField(card, "promptText", prompt);
        SetField(card, "saveButton", saveBtn.GetComponent<Button>());
        return card;
    }

    public static LibraryItemCard CreateLibraryCard(Transform parent)
    {
        var go = CreatePanel(parent, "LibraryCard", SurfaceDark);
        var cardElem = go.AddComponent<LayoutElement>();
        cardElem.preferredHeight = 184f;

        var title = CreateText(go.transform, "Title", "Saved Room Model", 21, FontStyles.Bold, TextWhite);
        Anchor(title.GetComponent<RectTransform>(), new Vector2(0.06f, 0.66f), new Vector2(0.94f, 0.91f));
        title.enableWordWrapping = true;
        title.overflowMode = TextOverflowModes.Ellipsis;

        var category = CreateText(go.transform, "Category", "2 walls | 1 floor | 18.40 m2", 14, FontStyles.Normal, TextMuted);
        Anchor(category.GetComponent<RectTransform>(), new Vector2(0.06f, 0.43f), new Vector2(0.94f, 0.62f));

        var openBtn = CreateButton(go.transform, "Open", "Open details", SurfaceElevated, TextWhite, 48f);
        Anchor(openBtn.GetComponent<RectTransform>(), new Vector2(0.06f, 0.10f), new Vector2(0.47f, 0.34f));
        var deleteBtn = CreateButton(go.transform, "Delete", "Delete", DangerRed, Color.white, 48f);
        Anchor(deleteBtn.GetComponent<RectTransform>(), new Vector2(0.53f, 0.10f), new Vector2(0.94f, 0.34f));

        var card = go.AddComponent<LibraryItemCard>();
        SetField(card, "titleText", title);
        SetField(card, "categoryText", category);
        SetField(card, "openButton", openBtn.GetComponent<Button>());
        SetField(card, "deleteButton", deleteBtn.GetComponent<Button>());
        return card;
    }

    static void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public static VastuChatBubble CreateChatBubble(Transform parent)
    {
        var go = CreatePanel(parent, "ChatBubble", SurfaceElevated);
        var bubbleElem = go.AddComponent<LayoutElement>();
        bubbleElem.preferredHeight = 80f;

        var layout = go.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 12, 12);
        layout.childControlWidth = true;
        layout.childControlHeight = true;

        var text = CreateText(go.transform, "Message", "", 18, FontStyles.Normal, TextWhite);

        var bubble = go.AddComponent<VastuChatBubble>();
        SetField(bubble, "messageText", text);
        SetField(bubble, "background", go.GetComponent<Image>());
        return bubble;
    }

    static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go;
    }

    static TMP_FontAsset cachedFont;

    static TMP_FontAsset GetDefaultFont()
    {
        if (cachedFont != null)
            return cachedFont;

        cachedFont = CoZeeTypography.UIFont;
        return cachedFont;
    }

    static TextMeshProUGUI CreateText(Transform parent, string name, string value, float size, FontStyles style, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        var font = GetDefaultFont();
        if (font != null)
            tmp.font = font;

        tmp.text = value;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.font = CoZeeTypography.FontFor(name, size, style);
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.enableWordWrapping = true;
        return tmp;
    }

    static GameObject CreateButton(Transform parent, string name, string label, Color bgColor, Color textColor, float height, float width = 0f)
    {
        var go = CreatePanel(parent, name, bgColor);
        go.AddComponent<Button>();

        var rect = go.GetComponent<RectTransform>();
        if (width > 0f) rect.sizeDelta = new Vector2(width, height);

        var layoutElem = go.AddComponent<LayoutElement>();
        layoutElem.preferredHeight = height;
        if (width > 0f) layoutElem.preferredWidth = width;

        var text = CreateText(go.transform, "Label", label, 18, FontStyles.Bold, textColor);
        text.alignment = TextAlignmentOptions.Center;
        Stretch(text.rectTransform);

        return go;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static void SetField(Object target, string fieldName, Object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        field?.SetValue(target, value);
    }
}
