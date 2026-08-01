using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OnboardingSlideView : MonoBehaviour
{
    [SerializeField] Image slideImage;
    [SerializeField] TextMeshProUGUI iconText;
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI bodyText;

    void Awake()
    {
        EnsureReferences();
    }

    void EnsureReferences()
    {
        if (titleText == null || bodyText == null || iconText == null)
        {
            TextMeshProUGUI[] tmps = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in tmps)
            {
                if (tmp.gameObject.name.Contains("Icon") && iconText == null)
                    iconText = tmp;
                else if (tmp.gameObject.name.Contains("Title") && titleText == null)
                    titleText = tmp;
                else if (tmp.gameObject.name.Contains("Body") && bodyText == null)
                    bodyText = tmp;
            }
        }
    }

    public void Bind(OnboardingSlideData data)
    {
        EnsureReferences();

        if (data == null)
            return;

        TMP_FontAsset font = TMP_Settings.defaultFontAsset ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

        if (titleText != null)
        {
            if (titleText.font == null && font != null) titleText.font = font;
            titleText.text = data.title;
            titleText.color = Color.white;
            titleText.fontSize = 32f;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.overflowMode = TextOverflowModes.Overflow;
            titleText.enabled = !string.IsNullOrWhiteSpace(data.title);
        }

        if (bodyText != null)
        {
            if (bodyText.font == null && font != null) bodyText.font = font;
            bodyText.text = data.body;
            bodyText.color = Color.white;
            bodyText.fontSize = 18f;
            bodyText.alignment = TextAlignmentOptions.Center;
            bodyText.overflowMode = TextOverflowModes.Overflow;
            bodyText.enabled = !string.IsNullOrWhiteSpace(data.body);
        }

        if (iconText != null)
        {
            if (iconText.font == null && font != null) iconText.font = font;
            iconText.color = Color.white;
            iconText.alignment = TextAlignmentOptions.Center;
            iconText.overflowMode = TextOverflowModes.Overflow;

            if (!string.IsNullOrEmpty(data.icon))
                iconText.text = data.icon;
        }

        if (slideImage != null)
        {
            slideImage.sprite = data.image;
            slideImage.enabled = data.image != null;
        }
    }
}
