using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LibraryScreenController : ScreenBase
{
    [SerializeField] LibraryTabBar tabBar;
    [SerializeField] Transform listRoot;
    [SerializeField] LibraryItemCard cardPrefab;

    string currentCategory = "all";
    readonly List<LibraryItemCard> cards = new List<LibraryItemCard>();

    void Awake()
    {
        if (listRoot == null)
        {
            var existing = transform.Find("ListRoot");
            listRoot = existing != null ? existing : transform;
        }

        ConfigureLayout();
    }

    void OnEnable()
    {
        Refresh();
    }

    void Start()
    {
        if (tabBar == null)
            tabBar = GetComponentInChildren<LibraryTabBar>(true);

        if (tabBar != null)
            tabBar.OnCategorySelected += ShowCategory;

        if (LibraryDataManager.Instance != null)
            LibraryDataManager.Instance.OnLibraryChanged += Refresh;
    }

    void OnDestroy()
    {
        if (tabBar != null)
            tabBar.OnCategorySelected -= ShowCategory;

        if (LibraryDataManager.Instance != null)
            LibraryDataManager.Instance.OnLibraryChanged -= Refresh;
    }

    protected override void OnShow()
    {
        ConfigureLayout();
        Refresh();
    }

    void ConfigureLayout()
    {
        VerticalLayoutGroup layout = GetComponent<VerticalLayoutGroup>();
        if (layout != null)
        {
            // MainShell already reserves space for its bottom navigation.
            layout.padding = new RectOffset(24, 24, 28, 28);
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
        }

        Transform header = transform.Find("Header");
        if (header != null)
        {
            Image headerImage = header.GetComponent<Image>();
            if (headerImage != null)
                headerImage.color = new Color(0.055f, 0.36f, 0.34f, 1f);
            LayoutElement headerLayout = header.GetComponent<LayoutElement>();
            if (headerLayout != null)
            {
                headerLayout.minHeight = 82f;
                headerLayout.preferredHeight = 82f;
                headerLayout.flexibleHeight = 0f;
            }

            TextMeshProUGUI title = header.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (title != null)
            {
                title.text = "Library";
                title.color = new Color(0.98f, 0.96f, 0.91f, 1f);
                title.fontSize = 28f;
                title.fontStyle = FontStyles.Bold;
            }

            TextMeshProUGUI subtitle = header.Find("Sub")?.GetComponent<TextMeshProUGUI>();
            if (subtitle != null)
            {
                subtitle.text = "Your saved room scans and design concepts";
                subtitle.color = new Color(0.88f, 0.95f, 0.92f, 1f);
                subtitle.fontSize = 13f;
            }

            EnsureHeaderText(header);
        }

        ConfigureFilterBar();

        if (listRoot == null)
            return;

        ConfigureScrollableList();

        VerticalLayoutGroup listLayout = listRoot.GetComponent<VerticalLayoutGroup>();
        if (listLayout != null)
        {
            listLayout.padding = new RectOffset(0, 0, 0, 16);
            listLayout.spacing = 14f;
            listLayout.childControlWidth = true;
            listLayout.childControlHeight = true;
            listLayout.childForceExpandHeight = false;
        }

        LayoutElement listLayoutElement = listRoot.GetComponent<LayoutElement>();
        if (listLayoutElement == null)
            listLayoutElement = listRoot.gameObject.AddComponent<LayoutElement>();
        listLayoutElement.flexibleHeight = 0f;

        ContentSizeFitter contentFitter = listRoot.GetComponent<ContentSizeFitter>();
        if (contentFitter == null)
            contentFitter = listRoot.gameObject.AddComponent<ContentSizeFitter>();
        contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    void ConfigureScrollableList()
    {
        RectTransform listRect = listRoot as RectTransform;
        if (listRect == null)
            return;

        Transform existingViewport = transform.Find("LibraryScrollViewport");
        GameObject viewport = existingViewport != null ? existingViewport.gameObject : null;
        if (viewport == null)
        {
            viewport = new GameObject("LibraryScrollViewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D), typeof(ScrollRect), typeof(LayoutElement));
            viewport.transform.SetParent(listRoot.parent, false);
            viewport.transform.SetSiblingIndex(listRoot.GetSiblingIndex());
            listRoot.SetParent(viewport.transform, false);
        }

        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        Image viewportImage = viewport.GetComponent<Image>();
        viewportImage.color = new Color(1f, 0.992f, 0.965f, 1f);
        viewportImage.raycastTarget = true;

        LayoutElement viewportLayout = viewport.GetComponent<LayoutElement>();
        viewportLayout.flexibleHeight = 1f;
        viewportLayout.minHeight = 180f;

        listRect.anchorMin = new Vector2(0f, 1f);
        listRect.anchorMax = new Vector2(1f, 1f);
        listRect.pivot = new Vector2(0.5f, 1f);
        listRect.anchoredPosition = Vector2.zero;
        listRect.offsetMin = new Vector2(0f, listRect.offsetMin.y);
        listRect.offsetMax = new Vector2(0f, listRect.offsetMax.y);

        ScrollRect scrollRect = viewport.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = listRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;
    }

    void EnsureHeaderText(Transform header)
    {
        CreateHeaderText(header, "LibraryHeaderTitle", "Library", 25f, FontStyles.Bold, new Color(0.98f, 0.96f, 0.91f, 1f), new Vector2(20f, 25f), new Vector2(-20f, -8f));
        CreateHeaderText(header, "LibraryHeaderSubtitle", "Saved room scans and design concepts", 12f, FontStyles.Normal, new Color(0.88f, 0.95f, 0.92f, 1f), new Vector2(20f, 4f), new Vector2(-20f, -35f));
    }

    void CreateHeaderText(Transform parent, string name, string value, float size, FontStyles style, Color color, Vector2 offsetMin, Vector2 offsetMax)
    {
        Transform existing = parent.Find(name);
        TextMeshProUGUI text = existing != null ? existing.GetComponent<TextMeshProUGUI>() : null;
        if (text == null)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
            textObject.transform.SetParent(parent, false);
            textObject.GetComponent<LayoutElement>().ignoreLayout = true;
            text = textObject.GetComponent<TextMeshProUGUI>();
            RectTransform rect = text.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        text.transform.SetAsLastSibling();
        TMP_FontAsset font = CoZeeTypography.FontFor(name, size, style);
        if (font != null)
            text.font = font;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
    }

    void ConfigureFilterBar()
    {
        Transform filterBar = transform.Find("LibraryTabs");
        if (filterBar == null)
            return;

        HorizontalLayoutGroup layout = filterBar.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
        {
            layout.padding = new RectOffset(8, 8, 7, 7);
            layout.spacing = 5f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
        }

        Image filterImage = filterBar.GetComponent<Image>();
        if (filterImage != null)
            filterImage.color = new Color(1f, 0.992f, 0.965f, 1f);

        LayoutElement filterLayout = filterBar.GetComponent<LayoutElement>();
        if (filterLayout == null)
            filterLayout = filterBar.gameObject.AddComponent<LayoutElement>();
        filterLayout.minHeight = 52f;
        filterLayout.preferredHeight = 52f;
        filterLayout.flexibleHeight = 0f;

        string[] labels = { "All", "Saved Rooms", "AI Designs", "Favorites" };
        string[] icons = { "\uf00a", "\uf52b", "\uf0d0", "\uf005" };
        Button[] buttons = filterBar.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length && i < labels.Length; i++)
        {
            LayoutElement element = buttons[i].GetComponent<LayoutElement>();
            if (element == null)
                element = buttons[i].gameObject.AddComponent<LayoutElement>();
            element.minWidth = 0f;
            element.preferredWidth = 0f;
            element.flexibleWidth = 1f;

            TextMeshProUGUI label = buttons[i].GetComponentInChildren<TextMeshProUGUI>(true);
            Image buttonImage = buttons[i].GetComponent<Image>();
            if (label == null)
                continue;

            label.text = labels[i];
            label.fontSize = 9f;
            label.fontStyle = FontStyles.Bold;
            label.color = new Color(0.075f, 0.118f, 0.145f, 1f);
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.alignment = TextAlignmentOptions.MidlineLeft;

            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0.30f, 0f);
            labelRect.anchorMax = new Vector2(0.96f, 1f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            EnsureFilterIcon(buttons[i].transform, icons[i], label.color);
        }

        UpdateFilterSelection();
    }

    static void EnsureFilterIcon(Transform button, string glyph, Color color)
    {
        Transform existing = button.Find("FilterIcon");
        TextMeshProUGUI icon = existing != null ? existing.GetComponent<TextMeshProUGUI>() : null;
        if (icon == null)
        {
            GameObject iconObject = new GameObject("FilterIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            iconObject.transform.SetParent(button, false);
            icon = iconObject.GetComponent<TextMeshProUGUI>();
        }

        RectTransform iconRect = icon.rectTransform;
        iconRect.anchorMin = new Vector2(0.07f, 0f);
        iconRect.anchorMax = new Vector2(0.25f, 1f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        icon.font = FontAwesomeIcons.FontAsset;
        icon.text = glyph;
        icon.color = color;
        icon.fontSize = 12f;
        icon.alignment = TextAlignmentOptions.Center;
        icon.enableWordWrapping = false;
        icon.raycastTarget = false;
        icon.transform.SetAsFirstSibling();
    }

    void ShowCategory(string category)
    {
        currentCategory = category;
        UpdateFilterSelection();
        Refresh();
    }

    void UpdateFilterSelection()
    {
        Transform filterBar = transform.Find("LibraryTabs");
        if (filterBar == null)
            return;

        int activeIndex = currentCategory == "rooms" ? 1
            : currentCategory == "concepts" ? 2
            : currentCategory == "favorites" ? 3
            : 0;

        Button[] buttons = filterBar.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length && i < 4; i++)
        {
            Image buttonImage = buttons[i].GetComponent<Image>();
            if (buttonImage != null)
                buttonImage.color = i == activeIndex
                    ? new Color(0.93f, 0.86f, 0.73f, 1f)
                    : new Color(0.82f, 0.91f, 0.87f, 1f);
        }
    }

    void Refresh()
    {
        foreach (LibraryItemCard card in cards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        Transform emptyState = listRoot != null ? listRoot.Find("LibraryEmptyState") : null;
        if (emptyState != null)
            Destroy(emptyState.gameObject);

        cards.Clear();

        if (LibraryDataManager.Instance == null || listRoot == null)
            return;

        List<LibraryItem> items;
        if (currentCategory == "all" || string.IsNullOrEmpty(currentCategory))
            items = new List<LibraryItem>(LibraryDataManager.Instance.GetAll());
        else if (currentCategory == "favorites")
            items = LibraryDataManager.Instance.GetFavorites();
        else if (currentCategory == "concepts")
        {
            // Existing mock concepts and generated AI designs use different legacy keys.
            items = LibraryDataManager.Instance.GetByCategory("concepts");
            items.AddRange(LibraryDataManager.Instance.GetByCategory("designs"));
        }
        else
            items = LibraryDataManager.Instance.GetByCategory(currentCategory);

        if (items.Count == 0)
        {
            CreateEmptyState();
            return;
        }

        foreach (LibraryItem item in items)
        {
            LibraryItemCard card = cardPrefab != null
                ? Instantiate(cardPrefab, listRoot)
                : RuntimeUIFactory.CreateLibraryCard(listRoot);

            LayoutElement cardLayout = card.GetComponent<LayoutElement>();
            if (cardLayout == null)
                cardLayout = card.gameObject.AddComponent<LayoutElement>();
            cardLayout.minHeight = 184f;
            cardLayout.preferredHeight = 184f;
            cardLayout.flexibleHeight = 0f;

            Image cardImage = card.GetComponent<Image>();
            if (cardImage != null)
                cardImage.color = new Color(0.92f, 0.94f, 0.93f, 1f);

            card.Bind(item, GetDisplayTitle(item));
            cards.Add(card);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(listRoot as RectTransform);
        ScrollRect scrollRect = transform.Find("LibraryScrollViewport")?.GetComponent<ScrollRect>();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    void CreateEmptyState()
    {
        GameObject empty = new GameObject("LibraryEmptyState", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        empty.transform.SetParent(listRoot, false);
        empty.GetComponent<Image>().color = new Color(0.92f, 0.94f, 0.93f, 1f);
        LayoutElement layout = empty.GetComponent<LayoutElement>();
        layout.preferredHeight = 140f;
        layout.flexibleHeight = 0f;

        GameObject messageObject = new GameObject("Message", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        messageObject.transform.SetParent(empty.transform, false);
        TextMeshProUGUI message = messageObject.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset font = CoZeeTypography.FontFor("LibraryEmptyState", 16f, FontStyles.Normal);
        if (font != null)
            message.font = font;
        message.text = currentCategory == "favorites"
            ? "No favorites yet. Tap the star on a saved room or AI design to add it here."
            : currentCategory == "concepts"
                ? "No AI designs saved yet. Generate a design to add it here."
                : "No saved rooms yet. Complete a room scan to add one here.";
        message.fontSize = 16f;
        message.color = new Color(0.30f, 0.38f, 0.40f, 1f);
        message.alignment = TextAlignmentOptions.Center;
        message.enableWordWrapping = true;
        message.rectTransform.anchorMin = new Vector2(0.08f, 0.15f);
        message.rectTransform.anchorMax = new Vector2(0.92f, 0.85f);
        message.rectTransform.offsetMin = Vector2.zero;
        message.rectTransform.offsetMax = Vector2.zero;
    }

    string GetDisplayTitle(LibraryItem item)
    {
        if (item == null || item.category != "rooms" || LibraryDataManager.Instance == null)
            return item != null ? item.title : string.Empty;

        if (!string.IsNullOrWhiteSpace(item.title) && item.title.StartsWith("Room "))
            return item.title;

        List<LibraryItem> rooms = LibraryDataManager.Instance.GetByCategory("rooms");
        int roomIndex = rooms.FindIndex(room => room.id == item.id);
        return roomIndex >= 0 ? $"Room {roomIndex + 1}" : item.title;
    }

    public void ShowRoomDetails(LibraryItem item)
    {
        if (item == null)
            return;

        Transform existing = transform.Find("RoomDetailsModal");
        if (existing != null)
            Destroy(existing.gameObject);

        GameObject overlay = new GameObject("RoomDetailsModal", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        overlay.transform.SetParent(transform, false);
        overlay.GetComponent<LayoutElement>().ignoreLayout = true;
        Image overlayImage = overlay.GetComponent<Image>();
        overlayImage.color = new Color(0.03f, 0.06f, 0.08f, 0.72f);
        Stretch(overlay.GetComponent<RectTransform>());

        GameObject card = new GameObject("DetailsCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup));
        card.transform.SetParent(overlay.transform, false);
        card.GetComponent<Image>().color = new Color(0.98f, 0.985f, 0.975f, 1f);
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.06f, 0.16f);
        cardRect.anchorMax = new Vector2(0.94f, 0.84f);
        cardRect.offsetMin = Vector2.zero;
        cardRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup cardLayout = card.GetComponent<VerticalLayoutGroup>();
        cardLayout.padding = new RectOffset(22, 22, 24, 20);
        cardLayout.spacing = 10f;
        cardLayout.childAlignment = TextAnchor.UpperLeft;
        cardLayout.childControlWidth = true;
        cardLayout.childControlHeight = true;
        cardLayout.childForceExpandHeight = false;

        CreateDetailText(card.transform, "DetailsTitle", GetDisplayTitle(item), 23f, FontStyles.Bold, new Color(0.075f, 0.118f, 0.145f, 1f), 34f);
        string summary = item.roomScanData != null && !string.IsNullOrWhiteSpace(item.roomScanData.summary)
            ? item.roomScanData.summary
            : string.IsNullOrWhiteSpace(item.detailsText) ? "Saved room" : item.detailsText;
        CreateDetailText(card.transform, "DetailsSummary", summary, 14f, FontStyles.Normal, new Color(0.30f, 0.38f, 0.40f, 1f), 28f);

        if (item.roomScanData != null && item.roomScanData.surfaces != null && item.roomScanData.surfaces.Count > 0)
        {
            foreach (ScannedSurfaceRecord surface in item.roomScanData.surfaces)
            {
                CreateSurfaceDetailRow(card.transform, surface);
            }
        }
        else
        {
            CreateDetailText(card.transform, "SurfaceDetail", "No surface measurements were saved for this item.", 13f, FontStyles.Normal, new Color(0.30f, 0.38f, 0.40f, 1f), 28f);
        }

        GameObject close = CreateDetailButton(card.transform, "CloseButton", "Close");
        close.GetComponent<Button>().onClick.AddListener(() => Destroy(overlay));
        CoZeeVisualTheme.Apply(transform.root);
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static TextMeshProUGUI CreateDetailText(Transform parent, string name, string value, float size, FontStyles style, Color color, float height)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
        textObject.transform.SetParent(parent, false);
        LayoutElement element = textObject.GetComponent<LayoutElement>();
        element.preferredHeight = height;
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset font = CoZeeTypography.FontFor(name, size, style);
        if (font != null)
            text.font = font;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.enableWordWrapping = true;
        return text;
    }

    static GameObject CreateDetailButton(Transform parent, string name, string label)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);
        buttonObject.GetComponent<Image>().color = new Color(0.055f, 0.36f, 0.34f, 1f);
        buttonObject.GetComponent<LayoutElement>().preferredHeight = 46f;
        TextMeshProUGUI text = CreateDetailText(buttonObject.transform, "Label", label, 15f, FontStyles.Bold, Color.white, 0f);
        text.rectTransform.anchorMin = new Vector2(0.42f, 0f);
        text.rectTransform.anchorMax = new Vector2(0.78f, 1f);
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.enableWordWrapping = false;

        GameObject iconObject = new GameObject("CloseIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        iconObject.transform.SetParent(buttonObject.transform, false);
        TextMeshProUGUI icon = iconObject.GetComponent<TextMeshProUGUI>();
        icon.font = FontAwesomeIcons.FontAsset;
        icon.text = "\uf00d"; // xmark
        icon.color = Color.white;
        icon.fontSize = 14f;
        icon.alignment = TextAlignmentOptions.Center;
        icon.raycastTarget = false;
        icon.rectTransform.anchorMin = new Vector2(0.22f, 0f);
        icon.rectTransform.anchorMax = new Vector2(0.38f, 1f);
        icon.rectTransform.offsetMin = Vector2.zero;
        icon.rectTransform.offsetMax = Vector2.zero;
        icon.transform.SetAsFirstSibling();
        return buttonObject;
    }

    static void CreateSurfaceDetailRow(Transform parent, ScannedSurfaceRecord surface)
    {
        GameObject row = new GameObject("SurfaceDetail", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        row.transform.SetParent(parent, false);
        row.GetComponent<Image>().color = new Color(0.90f, 0.94f, 0.92f, 1f);
        row.GetComponent<LayoutElement>().preferredHeight = 62f;

        string secondLabel = surface.surfaceType == "Wall" ? "Height" : "Length";
        CreateSurfaceText(row.transform, "Label", $"{surface.label}  •  {surface.surfaceType}", 12f, FontStyles.Bold, new Vector2(0.06f, 0.54f), new Vector2(0.94f, 0.92f));
        CreateSurfaceText(row.transform, "Primary", $"Width  {surface.primaryDimensionMeters:F2} m", 12f, FontStyles.Normal, new Vector2(0.06f, 0.10f), new Vector2(0.48f, 0.50f));
        CreateSurfaceText(row.transform, "Secondary", $"{secondLabel}  {surface.secondaryDimensionMeters:F2} m", 12f, FontStyles.Normal, new Vector2(0.52f, 0.10f), new Vector2(0.94f, 0.50f));
    }

    static void CreateSurfaceText(Transform parent, string name, string value, float size, FontStyles style, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset font = CoZeeTypography.FontFor(name, size, style);
        if (font != null)
            text.font = font;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = new Color(0.075f, 0.118f, 0.145f, 1f);
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.rectTransform.anchorMin = anchorMin;
        text.rectTransform.anchorMax = anchorMax;
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
    }
}
