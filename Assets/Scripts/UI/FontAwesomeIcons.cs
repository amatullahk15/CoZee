using TMPro;
using UnityEngine;

// Centralises the Font Awesome Free icon font so uGUI screens do not rely on unsupported emoji glyphs.
public static class FontAwesomeIcons
{
    const string ResourcePath = "Fonts/FontAwesome/fa-solid-900";
    static TMP_FontAsset fontAsset;

    public static TMP_FontAsset FontAsset
    {
        get
        {
            if (fontAsset != null)
                return fontAsset;

            Font sourceFont = Resources.Load<Font>(ResourcePath);
            if (sourceFont == null)
                return TMP_Settings.defaultFontAsset;

            fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
            fontAsset.name = "Font Awesome Free Solid Runtime";
            return fontAsset;
        }
    }

    public static string ForTab(AppTab tab)
    {
        switch (tab)
        {
            case AppTab.Home: return "\uf015";      // house
            case AppTab.ScanAR: return "\uf546";    // ruler-combined
            case AppTab.DesignAI: return "\uf0d0";  // magic
            case AppTab.Vastu: return "\uf14e";     // compass
            case AppTab.Library: return "\uf02d";   // book
            default: return string.Empty;
        }
    }

}
