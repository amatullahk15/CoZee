using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HomeDashboardController : ScreenBase
{
    static readonly Color Ink = new Color(0.075f, 0.118f, 0.145f, 1f);
    static readonly Color MutedInk = new Color(0.30f, 0.38f, 0.40f, 1f);
    static readonly Color CanvasCream = new Color(0.965f, 0.949f, 0.910f, 1f);
    static readonly Color CardCream = new Color(1f, 0.992f, 0.965f, 1f);
    static readonly Color Teal = new Color(0.055f, 0.36f, 0.34f, 1f);
    static readonly Color TealSoft = new Color(0.82f, 0.91f, 0.87f, 1f);
    static readonly Color Sand = new Color(0.93f, 0.86f, 0.73f, 1f);
    static readonly Color HeadingLight = new Color(0.98f, 0.96f, 0.91f, 1f);
    static readonly Color HeadingAccent = new Color(0.74f, 0.90f, 0.84f, 1f);
    static Sprite roundedPanelSprite;

    [SerializeField] Transform recentListRoot;
    [SerializeField] RecentRoomCard recentCardPrefab;
    [SerializeField] QuickActionButton scanAction;
    [SerializeField] QuickActionButton designAction;
    [SerializeField] QuickActionButton vastuAction;
    [SerializeField] QuickActionButton savedAction;

    readonly List<RecentRoomCard> cards = new List<RecentRoomCard>();

    protected override void OnShow()
    {
        EnsureQuickActions();
        EnsureTextColors();
        WireScanButtons();
        RefreshRecent();
    }

    void Awake()
    {
        EnsureQuickActions();
        EnsureTextColors();
        WireScanButtons();
    }

    void EnsureTextColors()
    {
        TextMeshProUGUI[] tmps = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmp in tmps)
        {
            if (tmp != null)
            {
                TMP_FontAsset font = CoZeeTypography.FontFor(tmp.gameObject.name, tmp.fontSize, tmp.fontStyle);
                if (font != null)
                    tmp.font = font;

                tmp.color = IsLightText(tmp) ? Color.white : Ink;
                tmp.enableWordWrapping = true;
                tmp.overflowMode = TextOverflowModes.Ellipsis;
            }
        }

        ApplyHomeVisualDesign();
    }

    void ApplyHomeVisualDesign()
    {
        Image background = FindDeepChild(transform.root, "CanvasBackground")?.GetComponent<Image>();
        if (background != null)
            background.color = CanvasCream;

        SetRoundedImageColor("HeroBannerCard", Teal);
        SetRoundedImageColor("ScanPillBtn", Sand);
        SetRoundedImageColor("ScanAction", CardCream);
        SetRoundedImageColor("DesignAction", CardCream);
        SetRoundedImageColor("VastuAction", CardCream);
        SetRoundedImageColor("SavedAction", CardCream);
        SetRoundedImageColor("FeaturedCard", CardCream);
        SetRoundedImageColor("VastuWisdomCard", Teal);

        ApplyHeroLayout();

        VerticalLayoutGroup contentLayout = FindDeepChild(transform, "Content")?.GetComponent<VerticalLayoutGroup>();
        if (contentLayout != null)
        {
            contentLayout.padding = new RectOffset(28, 28, 32, 132);
            contentLayout.spacing = 20;
        }

        Transform avatar = FindDeepChild(transform, "Avatar");
        if (avatar != null)
            avatar.gameObject.SetActive(false);

        Transform bell = FindDeepChild(transform, "BellBtn");
        if (bell != null)
            bell.gameObject.SetActive(false);

        Transform welcomeTag = FindDeepChild(transform, "WelcomeTag");
        if (welcomeTag != null)
        {
            welcomeTag.gameObject.SetActive(true);
            TextMeshProUGUI welcomeText = welcomeTag.GetComponent<TextMeshProUGUI>();
            if (welcomeText != null)
            {
                welcomeText.text = "Welcome back";
                welcomeText.color = HeadingLight;
                welcomeText.fontSize = 24;
                welcomeText.fontStyle = FontStyles.Bold;
            }
        }

        Transform greeting = FindDeepChild(transform, "GreetingText");
        if (greeting != null)
            greeting.gameObject.SetActive(false);

        Transform userStack = FindDeepChild(transform, "UserStack");
        if (userStack != null)
            userStack.gameObject.SetActive(false);

        EnsureHeaderTitle();
        SetTextStyle("HeroTitle", Color.white, 29, FontStyles.Bold);
        SetTextStyle("HeroSub", new Color(0.88f, 0.95f, 0.92f, 1f), 14, FontStyles.Normal);
        SetTextStyle("SectionHeader", HeadingLight, 23, FontStyles.Bold);
        SetTextStyle("RecentTitle", HeadingLight, 23, FontStyles.Bold);
        SetTextStyle("ViewAllBtn", HeadingAccent, 14, FontStyles.Bold);
        SetTextStyle("FeatTitle", Ink, 20, FontStyles.Bold);
        SetTextStyle("FeatDesc", MutedInk, 13, FontStyles.Normal);
        SetTextStyle("VastuTag", new Color(0.88f, 0.95f, 0.92f, 1f), 11, FontStyles.Bold);
        SetTextStyle("VastuBody", Color.white, 15, FontStyles.Normal);
        SetTextStyle("VastuLink", Sand, 14, FontStyles.Bold);

        foreach (QuickActionButton action in GetComponentsInChildren<QuickActionButton>(true))
        {
            TextMeshProUGUI[] labels = action.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI label in labels)
            {
                label.color = label.gameObject.name == "SubLabel" ? MutedInk : Ink;
                label.overflowMode = TextOverflowModes.Ellipsis;
            }

            Image badge = FindDeepChild(action.transform, "IconBadge")?.GetComponent<Image>();
            if (badge != null)
            {
                ApplyRoundedCorners(badge);
                badge.color = TealSoft;
            }

            CreateQuickActionIcon(action.transform, action == scanAction ? 0 : action == designAction ? 1 : action == vastuAction ? 2 : 3);
        }
    }

    void EnsureHeaderTitle()
    {
        Transform topBar = FindDeepChild(transform, "TopBar");
        if (topBar == null)
            return;

        Transform existing = topBar.Find("HomeWelcomeTitle");
        TextMeshProUGUI title = existing != null ? existing.GetComponent<TextMeshProUGUI>() : null;
        if (title == null)
        {
            GameObject titleObject = new GameObject("HomeWelcomeTitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
            titleObject.transform.SetParent(topBar, false);
            title = titleObject.GetComponent<TextMeshProUGUI>();

            LayoutElement layoutElement = titleObject.GetComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            RectTransform rect = title.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(2f, 0f);
            rect.offsetMax = new Vector2(-2f, 0f);
        }

        title.transform.SetAsLastSibling();
        title.font = TMP_Settings.defaultFontAsset;
        title.text = "Welcome back";
        title.color = Color.white;
        title.fontSize = 24;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.MidlineLeft;
        title.enableWordWrapping = false;
        title.overflowMode = TextOverflowModes.Overflow;
        title.raycastTarget = false;
    }

    void ApplyHeroLayout()
    {
        Transform hero = FindDeepChild(transform, "HeroBannerCard");
        if (hero == null)
            return;

        // Preserve the existing hero-card size and use its space as a deliberate vertical composition.
        HorizontalLayoutGroup horizontalLayout = hero.GetComponent<HorizontalLayoutGroup>();
        if (horizontalLayout != null)
            horizontalLayout.enabled = false;

        Transform textColumn = hero.Find("HeroTextCol");
        if (textColumn != null)
        {
            RectTransform textRect = textColumn.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.08f, 0.48f);
            textRect.anchorMax = new Vector2(0.92f, 0.88f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI title = FindDeepChild(textColumn, "HeroTitle")?.GetComponent<TextMeshProUGUI>();
            if (title != null)
            {
                title.text = "Start a new scan";
                title.fontSize = 25;
                title.enableWordWrapping = false;
                title.overflowMode = TextOverflowModes.Overflow;
            }

            TextMeshProUGUI subtitle = FindDeepChild(textColumn, "HeroSub")?.GetComponent<TextMeshProUGUI>();
            if (subtitle != null)
            {
                subtitle.text = "Measure your room in AR with confidence.";
                subtitle.fontSize = 13;
                subtitle.overflowMode = TextOverflowModes.Ellipsis;
            }
        }

        Transform scanButton = hero.Find("ScanPillBtn");
        if (scanButton == null)
            return;

        RectTransform buttonRect = scanButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.08f, 0.12f);
        buttonRect.anchorMax = new Vector2(0.92f, 0.37f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;

        LayoutElement buttonLayout = scanButton.GetComponent<LayoutElement>();
        if (buttonLayout != null)
        {
            buttonLayout.preferredWidth = -1f;
            buttonLayout.preferredHeight = -1f;
        }

        TextMeshProUGUI buttonLabel = scanButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (buttonLabel != null)
        {
            buttonLabel.text = "Start Scan";
            buttonLabel.fontSize = 16;
            buttonLabel.fontStyle = FontStyles.Bold;
            buttonLabel.enableWordWrapping = false;
            buttonLabel.overflowMode = TextOverflowModes.Overflow;
        }
    }

    bool IsLightText(TextMeshProUGUI text)
    {
        string name = text.gameObject.name;
        return text.GetComponentInParent<Transform>() != null
            && (name == "HeroTitle" || name == "HeroSub" || name == "VastuBody");
    }

    void SetRoundedImageColor(string objectName, Color color)
    {
        Image image = FindDeepChild(transform, objectName)?.GetComponent<Image>();
        if (image == null)
            return;

        ApplyRoundedCorners(image);
        image.color = color;
    }

    static void ApplyRoundedCorners(Image image)
    {
        image.sprite = GetRoundedPanelSprite();
        image.type = Image.Type.Sliced;
    }

    static Sprite GetRoundedPanelSprite()
    {
        if (roundedPanelSprite != null)
            return roundedPanelSprite;

        const int textureSize = 64;
        const float cornerRadius = 14f;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
        {
            name = "RuntimeRoundedPanel"
        };

        Vector2 center = new Vector2((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float dx = Mathf.Max(Mathf.Abs(x - center.x) - (textureSize * 0.5f - cornerRadius), 0f);
                float dy = Mathf.Max(Mathf.Abs(y - center.y) - (textureSize * 0.5f - cornerRadius), 0f);
                float alpha = dx * dx + dy * dy <= cornerRadius * cornerRadius ? 1f : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply(false, true);
        roundedPanelSprite = Sprite.Create(texture, new Rect(0f, 0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(cornerRadius, cornerRadius, cornerRadius, cornerRadius));
        return roundedPanelSprite;
    }

    void CreateQuickActionIcon(Transform action, int iconType)
    {
        Transform badge = FindDeepChild(action, "IconBadge");
        if (badge == null || badge.Find("QuickActionIcon") != null)
            return;

        TextMeshProUGUI placeholder = badge.GetComponentInChildren<TextMeshProUGUI>(true);
        if (placeholder != null)
            placeholder.text = string.Empty;

        GameObject icon = new GameObject("QuickActionIcon", typeof(RectTransform));
        icon.transform.SetParent(badge, false);
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(10f, 6f);
        iconRect.offsetMax = new Vector2(-10f, -6f);

        switch (iconType)
        {
            case 0: // AR scan frame
                AddIconMark(icon.transform, new Vector2(-9f, 7f), new Vector2(9f, 3f));
                AddIconMark(icon.transform, new Vector2(9f, 7f), new Vector2(9f, 3f));
                AddIconMark(icon.transform, new Vector2(-9f, -7f), new Vector2(9f, 3f));
                AddIconMark(icon.transform, new Vector2(9f, -7f), new Vector2(9f, 3f));
                break;
            case 1: // Design sparkle
                AddIconMark(icon.transform, Vector2.zero, new Vector2(13f, 13f), 45f);
                AddIconMark(icon.transform, new Vector2(10f, 9f), new Vector2(5f, 5f));
                break;
            case 2: // Vastu compass
                AddIconMark(icon.transform, Vector2.zero, new Vector2(14f, 14f), 45f);
                AddIconMark(icon.transform, Vector2.zero, new Vector2(22f, 3f));
                AddIconMark(icon.transform, Vector2.zero, new Vector2(3f, 22f));
                break;
            default: // Library books
                AddIconMark(icon.transform, new Vector2(-7f, 0f), new Vector2(4f, 19f));
                AddIconMark(icon.transform, Vector2.zero, new Vector2(4f, 19f));
                AddIconMark(icon.transform, new Vector2(7f, 0f), new Vector2(4f, 19f));
                break;
        }
    }

    static void AddIconMark(Transform parent, Vector2 position, Vector2 size, float rotation = 0f)
    {
        GameObject mark = new GameObject("Mark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        mark.transform.SetParent(parent, false);
        RectTransform rect = mark.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
        Image image = mark.GetComponent<Image>();
        image.color = Teal;
        image.raycastTarget = false;
    }

    void SetTextStyle(string objectName, Color color, float size, FontStyles style)
    {
        TextMeshProUGUI text = FindDeepChild(transform, objectName)?.GetComponent<TextMeshProUGUI>();
        if (text == null)
            return;

        text.color = color;
        text.fontSize = size;
        text.fontStyle = style;
    }

    static Transform FindDeepChild(Transform root, string objectName)
    {
        if (root == null)
            return null;

        if (root.name == objectName)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform result = FindDeepChild(root.GetChild(i), objectName);
            if (result != null)
                return result;
        }

        return null;
    }

    void EnsureQuickActions()
    {
        QuickActionButton[] actions = GetComponentsInChildren<QuickActionButton>(true);
        if (actions.Length > 0 && scanAction == null) scanAction = actions[0];
        if (actions.Length > 1 && designAction == null) designAction = actions[1];
        if (actions.Length > 2 && vastuAction == null) vastuAction = actions[2];
        if (actions.Length > 3 && savedAction == null) savedAction = actions[3];

        scanAction?.SetLabel("Scan Room");
        designAction?.SetLabel("AI Design");
        vastuAction?.SetLabel("Vastu Check");
        savedAction?.SetLabel("Saved Rooms");
    }

    void WireScanButtons()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            if (btn == null) continue;
            string n = btn.gameObject.name.ToLowerInvariant();
            if (n.Contains("scanpill") || n.Contains("herobanner") || n.Contains("startscan") || n.Contains("scanaction"))
            {
                btn.onClick.RemoveListener(OnScanClicked);
                btn.onClick.AddListener(OnScanClicked);
            }
            else if (n.Contains("viewall"))
            {
                btn.onClick.RemoveListener(OnViewAllClicked);
                btn.onClick.AddListener(OnViewAllClicked);
            }
        }
    }

    void OnScanClicked()
    {
        AudioManager.Instance?.PlayClick();
        NavigationManager.Instance?.SelectTab(AppTab.ScanAR);
    }

    void OnViewAllClicked()
    {
        AudioManager.Instance?.PlayClick();
        NavigationManager.Instance?.SelectTab(AppTab.Library);
    }

    void RefreshRecent()
    {
        foreach (RecentRoomCard card in cards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        cards.Clear();

        if (LibraryDataManager.Instance == null || recentListRoot == null)
            return;

        if (recentCardPrefab == null)
            return;

        List<LibraryItem> rooms = LibraryDataManager.Instance.GetByCategory("rooms");
        int count = Mathf.Min(rooms.Count, 5);

        for (int i = 0; i < count; i++)
        {
            RecentRoomCard card = Instantiate(recentCardPrefab, recentListRoot);
            card.Bind(rooms[i]);
            cards.Add(card);
        }
    }
}
