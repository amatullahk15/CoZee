using TMPro;
using UnityEngine;

// Provides a single Google Font source for CoZee's runtime uGUI and TextMeshPro text.
public static class CoZeeTypography
{
    const string BodyResourcePath = "Fonts/CoZee/Inter";
    const string ButtonResourcePath = "Fonts/CoZee/DMSans";
    const string HeadingResourcePath = "Fonts/CoZee/Poppins-SemiBold";
    static TMP_FontAsset bodyFont;
    static TMP_FontAsset buttonFont;
    static TMP_FontAsset headingFont;

    public static TMP_FontAsset UIFont
    {
        get { return BodyFont; }
    }

    public static TMP_FontAsset BodyFont
    {
        get
        {
            if (bodyFont != null)
                return bodyFont;

            Font sourceFont = Resources.Load<Font>(BodyResourcePath);
            if (sourceFont == null)
                return TMP_Settings.defaultFontAsset;

            bodyFont = TMP_FontAsset.CreateFontAsset(sourceFont);
            bodyFont.name = "CoZee Inter Runtime";
            return bodyFont;
        }
    }

    public static TMP_FontAsset ButtonFont
    {
        get
        {
            if (buttonFont != null)
                return buttonFont;

            Font sourceFont = Resources.Load<Font>(ButtonResourcePath);
            if (sourceFont == null)
                return BodyFont;

            buttonFont = TMP_FontAsset.CreateFontAsset(sourceFont);
            buttonFont.name = "CoZee DM Sans Runtime";
            return buttonFont;
        }
    }

    public static TMP_FontAsset HeadingFont
    {
        get
        {
            if (headingFont != null)
                return headingFont;

            Font sourceFont = Resources.Load<Font>(HeadingResourcePath);
            if (sourceFont == null)
                return BodyFont;

            headingFont = TMP_FontAsset.CreateFontAsset(sourceFont);
            headingFont.name = "CoZee Poppins SemiBold Runtime";
            return headingFont;
        }
    }

    public static TMP_FontAsset FontFor(string elementName, float size, FontStyles style)
    {
        bool isButton = elementName == "Label"
            || (!string.IsNullOrEmpty(elementName)
                && (elementName.Contains("Button") || elementName.Contains("Btn")));
        if (isButton)
            return ButtonFont;

        bool isHeading = size >= 20f
            || (!string.IsNullOrEmpty(elementName)
                && (elementName.Contains("Title") || elementName.Contains("Heading") || elementName.Contains("Hero")));
        return isHeading ? HeadingFont : BodyFont;
    }
}
