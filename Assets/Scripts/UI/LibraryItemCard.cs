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
            deleteButton.onClick.AddListener(Delete);
    }

    public void Bind(LibraryItem item)
    {
        boundItem = item;

        if (titleText != null)
            titleText.text = item.title;

        if (categoryText != null)
            categoryText.text = item.category;

        favoriteToggle?.Bind(item.id, item.isFavorite);
    }

    void Open()
    {
        if (boundItem == null)
            return;

        UIManager.Instance?.ShowToast("Opening " + boundItem.title);

        if (boundItem.category == "rooms")
        {
            NavigationManager.Instance?.SelectTab(AppTab.ScanAR);
        }
        else
        {
            NavigationManager.Instance?.SelectTab(AppTab.DesignAI);
        }
    }

    void Delete()
    {
        if (boundItem == null || LibraryDataManager.Instance == null)
            return;

        LibraryDataManager.Instance.RemoveItem(boundItem.id);
    }
}
