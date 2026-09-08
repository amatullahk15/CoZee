using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LibraryScreenController : ScreenBase
{
    [SerializeField] LibraryTabBar tabBar;
    [SerializeField] Transform listRoot;
    [SerializeField] LibraryItemCard cardPrefab;

    string currentCategory = "rooms";
    readonly List<LibraryItemCard> cards = new List<LibraryItemCard>();

    void Awake()
    {
        if (listRoot == null)
        {
            var existing = transform.Find("ListRoot");
            listRoot = existing != null ? existing : transform;

        ConfigureLayout();
        }
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
            layout.padding = new RectOffset(24, 24, 28, 116);
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
                headerLayout.preferredHeight = 92f;

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

        LayoutElement listLayoutElement = listRoot.GetComponent<LayoutElement>();
        if (listLayoutElement == null)
            listLayoutElement = listRoot.gameObject.AddComponent<LayoutElement>();
        listLayoutElement.flexibleHeight = 1f;

        VerticalLayoutGroup listLayout = listRoot.GetComponent<VerticalLayoutGroup>();
        if (listLayout != null)
        {
            listLayout.spacing = 14f;
            listLayout.childControlWidth = true;
            listLayout.childControlHeight = true;
            listLayout.childForceExpandHeight = false;
        }
    }

    void EnsureHeaderText(Transform header)
    {
        CreateHeaderText(header, "LibraryHeaderTitle", "Library", 25f, FontStyles.Bold, new Color(0.98f, 0.96f, 0.91f, 1f), new Vector2(20f, 30f), new Vector2(-20f, -8f));
        CreateHeaderText(header, "LibraryHeaderSubtitle", "Saved room scans and design concepts", 12f, FontStyles.Normal, new Color(0.88f, 0.95f, 0.92f, 1f), new Vector2(20f, 4f), new Vector2(-20f, -42f));
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
        TMP_FontAsset font = TMP_Settings.defaultFontAsset ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
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
            layout.padding = new RectOffset(8, 8, 10, 10);
            layout.spacing = 6f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
        }

        Image filterImage = filterBar.GetComponent<Image>();
        if (filterImage != null)
            filterImage.color = new Color(1f, 0.992f, 0.965f, 1f);

        string[] labels = { "All", "Saved Rooms", "AI Designs", "Favorites" };
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
            if (buttonImage != null)
                buttonImage.color = i == 1 ? new Color(0.93f, 0.86f, 0.73f, 1f) : new Color(0.82f, 0.91f, 0.87f, 1f);
            if (label == null)
                continue;

            label.text = labels[i];
            label.fontSize = 11f;
            label.fontStyle = FontStyles.Bold;
            label.color = new Color(0.075f, 0.118f, 0.145f, 1f);
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.alignment = TextAlignmentOptions.Center;
        }
    }

    void ShowCategory(string category)
    {
        currentCategory = category;
        Refresh();
    }

    void Refresh()
    {
        foreach (LibraryItemCard card in cards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        cards.Clear();

        if (LibraryDataManager.Instance == null || listRoot == null)
            return;

        List<LibraryItem> items;
        if (currentCategory == "all" || string.IsNullOrEmpty(currentCategory))
            items = new List<LibraryItem>(LibraryDataManager.Instance.GetAll());
        else if (currentCategory == "favorites")
            items = LibraryDataManager.Instance.GetFavorites();
        else
            items = LibraryDataManager.Instance.GetByCategory(currentCategory);

        if (items.Count == 0)
            items = new List<LibraryItem>(LibraryDataManager.Instance.GetAll());

        foreach (LibraryItem item in items)
        {
            LibraryItemCard card = cardPrefab != null
                ? Instantiate(cardPrefab, listRoot)
                : RuntimeUIFactory.CreateLibraryCard(listRoot);

            card.Bind(item);
            cards.Add(card);
        }

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

        CreateDetailText(card.transform, "DetailsTitle", item.title, 23f, FontStyles.Bold, new Color(0.075f, 0.118f, 0.145f, 1f), 34f);
        string summary = item.roomScanData != null && !string.IsNullOrWhiteSpace(item.roomScanData.summary)
            ? item.roomScanData.summary
            : string.IsNullOrWhiteSpace(item.detailsText) ? "Saved room" : item.detailsText;
        CreateDetailText(card.transform, "DetailsSummary", summary, 14f, FontStyles.Normal, new Color(0.30f, 0.38f, 0.40f, 1f), 28f);

        if (item.roomScanData != null && item.roomScanData.surfaces != null && item.roomScanData.surfaces.Count > 0)
        {
            foreach (ScannedSurfaceRecord surface in item.roomScanData.surfaces)
            {
                string secondLabel = surface.surfaceType == "Wall" ? "Height" : "Length";
                string line = $"{surface.label} ({surface.surfaceType})  |  Width {surface.primaryDimensionMeters:F2}m  |  {secondLabel} {surface.secondaryDimensionMeters:F2}m";
                CreateDetailText(card.transform, "SurfaceDetail", line, 13f, FontStyles.Normal, new Color(0.075f, 0.118f, 0.145f, 1f), 28f);
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
        TMP_FontAsset font = TMP_Settings.defaultFontAsset ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
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
        buttonObject.GetComponent<LayoutElement>().preferredHeight = 46f;
        TextMeshProUGUI text = CreateDetailText(buttonObject.transform, "Label", label, 15f, FontStyles.Bold, new Color(0.075f, 0.118f, 0.145f, 1f), 0f);
        Stretch(text.rectTransform);
        text.alignment = TextAlignmentOptions.Center;
        return buttonObject;
    }
}
