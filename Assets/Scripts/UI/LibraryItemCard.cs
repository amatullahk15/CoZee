using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LibraryItemCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI categoryText;
    [SerializeField] Button openButton;
    [SerializeField] Button deleteButton;
    [SerializeField] FavoriteToggle favoriteToggle;

    LibraryItem boundItem;

    void Awake()
    {
        if (openButton == null)
            openButton = GetComponentInChildren<Button>(true);

        if (deleteButton == null)
            deleteButton = GetComponentsInChildren<Button>(true).Length > 1 ? GetComponentsInChildren<Button>(true)[1] : null;

        if (openButton != null)
            openButton.onClick.AddListener(Open);

        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(RequestDelete);
            ConfigureDeleteButton();
        }
    }

    public void Bind(LibraryItem item, string displayTitle = null)
    {
        boundItem = item;

        if (titleText != null)
            titleText.text = string.IsNullOrWhiteSpace(displayTitle) ? item.title : displayTitle;

        if (categoryText != null)
        {
            if (item.roomScanData != null && !string.IsNullOrWhiteSpace(item.roomScanData.summary))
                categoryText.text = item.roomScanData.summary;
            else if (!string.IsNullOrWhiteSpace(item.detailsText))
                categoryText.text = item.detailsText;
            else
                categoryText.text = item.category;
        }

        favoriteToggle?.Bind(item.id, item.isFavorite);
    }

    void Open()
    {
        if (boundItem == null)
            return;

        UIManager.Instance?.ShowToast("Opening " + boundItem.title);

        if (boundItem.category == "rooms")
        {
            LibraryScreenController library = FindObjectOfType<LibraryScreenController>(true);
            if (library != null)
            {
                library.ShowRoomDetails(boundItem);
                return;
            }

            NavigationManager.Instance?.SelectTab(AppTab.ScanAR);
        }
        else
        {
            NavigationManager.Instance?.SelectTab(AppTab.DesignAI);
        }
    }

    void RequestDelete()
    {
        if (boundItem == null || LibraryDataManager.Instance == null)
            return;

        LibraryDeleteConfirmationDialog.Show(boundItem.title, Delete);
    }

    void Delete()
    {
        if (boundItem == null || LibraryDataManager.Instance == null)
            return;

        LibraryDataManager.Instance.RemoveItem(boundItem.id);
    }

    void ConfigureDeleteButton()
    {
        TextMeshProUGUI label = deleteButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
            label.text = "Delete";

        // The button already has a fixed width. Anchors keep the icon and label legible on narrow cards.
        HorizontalLayoutGroup layout = deleteButton.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
            layout.enabled = false;

        Transform existingIcon = deleteButton.transform.Find("DeleteIcon");
        TextMeshProUGUI icon = existingIcon != null
            ? existingIcon.GetComponent<TextMeshProUGUI>()
            : CreateDeleteIcon();

        if (icon == null)
            return;

        if (label != null)
        {
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0.37f, 0f);
            labelRect.anchorMax = new Vector2(0.88f, 1f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Overflow;
        }

        RectTransform iconRect = icon.rectTransform;
        iconRect.anchorMin = new Vector2(0.16f, 0f);
        iconRect.anchorMax = new Vector2(0.32f, 1f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        icon.font = FontAwesomeIcons.FontAsset;
        icon.text = "\uf1f8"; // trash
        icon.color = label != null ? label.color : Color.white;
        icon.fontSize = 15f;
        icon.alignment = TextAlignmentOptions.Center;
        icon.enableWordWrapping = false;
        icon.raycastTarget = false;
        icon.transform.SetAsFirstSibling();
    }

    TextMeshProUGUI CreateDeleteIcon()
    {
        GameObject iconObject = new GameObject("DeleteIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        iconObject.transform.SetParent(deleteButton.transform, false);

        return iconObject.GetComponent<TextMeshProUGUI>();
    }
}
