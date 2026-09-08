using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Applies the Home dashboard's visual language to the existing tab content.
public static class CoZeeVisualTheme
{
    static readonly Color Ink = new Color(0.075f, 0.118f, 0.145f, 1f);
    static readonly Color MutedInk = new Color(0.30f, 0.38f, 0.40f, 1f);
    static readonly Color Teal = new Color(0.055f, 0.36f, 0.34f, 1f);
    static readonly Color Mint = new Color(0.82f, 0.91f, 0.87f, 1f);
    static readonly Color Cream = new Color(1f, 0.992f, 0.965f, 1f);
    static readonly Color Sand = new Color(0.93f, 0.86f, 0.73f, 1f);
    static readonly Color LightText = new Color(0.98f, 0.96f, 0.91f, 1f);
    static Sprite roundedSprite;
    static TMP_FontAsset fallbackFont;

    public static void Apply(Transform root)
    {
        if (root == null)
            return;

        ApplyTab(FindDeepChild(root, "DesignAITab"));
        ApplyTab(FindDeepChild(root, "VastuTab"));
        ApplyScanOverlay(FindDeepChild(root, "ScanARTab"));
    }

    static void ApplyTab(Transform tab)
    {
        if (tab == null)
            return;

        foreach (TextMeshProUGUI text in tab.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            TMP_FontAsset font = GetFont();
            if (font != null)
                text.font = font;
            text.color = IsOnTealSurface(text.transform) ? LightText : Ink;
            text.overflowMode = TextOverflowModes.Ellipsis;
        }

        foreach (Image image in tab.GetComponentsInChildren<Image>(true))
        {
            string name = image.gameObject.name;
            if (name.Contains("Header") || name.Contains("InfoCard"))
            {
                SetRounded(image, Teal);
            }
            else if (name.Contains("Card") || name.Contains("Panel") || name.Contains("Input") || name.Contains("Tabs"))
            {
                SetRounded(image, Cream);
            }
        }

        foreach (Button button in tab.GetComponentsInChildren<Button>(true))
            StyleButton(button);

        VerticalLayoutGroup[] layouts = tab.GetComponentsInChildren<VerticalLayoutGroup>(true);
        foreach (VerticalLayoutGroup layout in layouts)
            layout.spacing = Mathf.Max(layout.spacing, 14f);
    }

    static TMP_FontAsset GetFont()
    {
        if (fallbackFont != null)
            return fallbackFont;

        fallbackFont = TMP_Settings.defaultFontAsset ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        return fallbackFont;
    }

    static void ApplyScanOverlay(Transform tab)
    {
        if (tab == null)
            return;

        foreach (Image image in tab.GetComponentsInChildren<Image>(true))
        {
            string name = image.gameObject.name;
            if (name.Contains("TopBar") || name.Contains("Status") || name.Contains("Tray"))
                SetRounded(image, Teal);
        }

        foreach (Button button in tab.GetComponentsInChildren<Button>(true))
            StyleButton(button);
    }

    static void StyleButton(Button button)
    {
        if (button == null)
            return;

        Image image = button.GetComponent<Image>();
        if (image == null)
            return;

        string name = button.gameObject.name;
        bool primary = name.Contains("Generate") || name.Contains("Check") || name.Contains("Save") || name.Contains("Send") || name.Contains("Scan");
        SetRounded(image, primary ? Sand : Mint);
        button.transition = Selectable.Transition.ColorTint;

        TextMeshProUGUI[] labels = button.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI label in labels)
        {
            label.color = Ink;
            label.fontStyle = FontStyles.Bold;
        }
    }

    static bool IsOnTealSurface(Transform text)
    {
        for (Transform parent = text.parent; parent != null; parent = parent.parent)
        {
            string name = parent.name;
            if (name.Contains("Header") || name.Contains("InfoCard") || name.Contains("TopBar"))
                return true;
        }

        return false;
    }

    static void SetRounded(Image image, Color color)
    {
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = color;
    }

    static Sprite GetRoundedSprite()
    {
        if (roundedSprite != null)
            return roundedSprite;

        const int size = 64;
        const float radius = 14f;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(Mathf.Abs(x - center.x) - (size * 0.5f - radius), 0f);
                float dy = Mathf.Max(Mathf.Abs(y - center.y) - (size * 0.5f - radius), 0f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, dx * dx + dy * dy <= radius * radius ? 1f : 0f));
            }
        }

        texture.Apply(false, true);
        roundedSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        return roundedSprite;
    }

    static Transform FindDeepChild(Transform root, string name)
    {
        if (root == null)
            return null;
        if (root.name == name)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform match = FindDeepChild(root.GetChild(i), name);
            if (match != null)
                return match;
        }

        return null;
    }
}
