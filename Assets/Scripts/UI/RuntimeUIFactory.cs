using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class RuntimeUIFactory
{
    static readonly Color SurfaceDark = new Color(0.12f, 0.16f, 0.23f, 0.96f); // #1E293B
    static readonly Color SurfaceElevated = new Color(0.20f, 0.25f, 0.33f, 0.96f); // #334155
    static readonly Color PrimaryBlue = new Color(0.23f, 0.51f, 0.96f, 1f); // #3B82F6
    static readonly Color DangerRed = new Color(0.94f, 0.27f, 0.27f, 1f); // #EF4444
    static readonly Color TextWhite = new Color(0.97f, 0.98f, 0.99f, 1f);
    static readonly Color TextMuted = new Color(0.58f, 0.64f, 0.72f, 1f);

    public static GeneratedConceptCard CreateConceptCard(Transform parent)
    {
        var go = CreatePanel(parent, "ConceptCard", SurfaceDark);
        var cardElem = go.AddComponent<LayoutElement>();
        cardElem.preferredHeight = 160f;

        var layout = go.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 16, 16);
        layout.spacing = 10f;
        layout.childControlWidth = true;
        layout.childControlHeight = false;

        var title = CreateText(go.transform, "Title", "Modern Living Room Concept", 22, FontStyles.Bold, TextWhite);
        var prompt = CreateText(go.transform, "Prompt", "Prompt description...", 16, FontStyles.Normal, TextMuted);

        var saveBtn = CreateButton(go.transform, "Save", "💾 Save to Library", PrimaryBlue, TextWhite, 48f);

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
        cardElem.preferredHeight = 110f;

        var layout = go.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 16, 16);
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        // Info Stack
        var infoCol = CreatePanel(go.transform, "InfoCol", Color.clear);
        var infoElem = infoCol.AddComponent<LayoutElement>();
        infoElem.preferredWidth = 400f;
        infoElem.flexibleWidth = 1f;

        var colLayout = infoCol.AddComponent<VerticalLayoutGroup>();
        colLayout.spacing = 4f;

        var title = CreateText(infoCol.transform, "Title", "Saved Room Model", 22, FontStyles.Bold, TextWhite);
        var category = CreateText(infoCol.transform, "Category", "Category: AR Scans", 16, FontStyles.Normal, TextMuted);

        var openBtn = CreateButton(go.transform, "Open", "Open", SurfaceElevated, TextWhite, 48f, 110f);
        var deleteBtn = CreateButton(go.transform, "Delete", "Delete", DangerRed, TextWhite, 48f, 110f);

        var card = go.AddComponent<LibraryItemCard>();
        SetField(card, "titleText", title);
        SetField(card, "categoryText", category);
        SetField(card, "openButton", openBtn.GetComponent<Button>());
        SetField(card, "deleteButton", deleteBtn.GetComponent<Button>());
        return card;
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

        cachedFont = TMP_Settings.defaultFontAsset;
        if (cachedFont != null)
            return cachedFont;

        cachedFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
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
