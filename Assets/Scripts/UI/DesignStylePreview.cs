using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Uses the locally packaged preview sheet, avoiding a network dependency in the mobile app.
public static class DesignStylePreview
{
    static readonly Dictionary<string, int> StyleIndices = new Dictionary<string, int>
    {
        { "Modern", 0 },
        { "Boho", 1 },
        { "Scandinavian", 2 },
        { "Minimal", 3 },
        { "Japandi", 4 },
        { "Industrial", 0 }
    };

    static readonly Dictionary<int, Sprite> Sprites = new Dictionary<int, Sprite>();
    static Texture2D previewSheet;

    public static void Apply(Image image, string style)
    {
        if (image == null)
            return;

        if (previewSheet == null)
            previewSheet = Resources.Load<Texture2D>("DesignStylePreviews");

        if (previewSheet == null)
        {
            image.color = new Color(0.82f, 0.91f, 0.87f, 1f);
            return;
        }

        int index = StyleIndices.TryGetValue(style ?? string.Empty, out int mappedIndex) ? mappedIndex : 0;
        if (!Sprites.TryGetValue(index, out Sprite sprite))
        {
            float panelWidth = previewSheet.width / 5f;
            sprite = Sprite.Create(
                previewSheet,
                new Rect(panelWidth * index, 0f, panelWidth, previewSheet.height),
                new Vector2(0.5f, 0.5f),
                100f);
            Sprites[index] = sprite;
        }

        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        image.color = Color.white;
    }
}
