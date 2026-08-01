using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecentRoomCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] Image thumbnail;
    [SerializeField] Button openButton;

    LibraryItem boundItem;

    void Awake()
    {
        if (openButton != null)
            openButton.onClick.AddListener(Open);
    }

    public void Bind(LibraryItem item)
    {
        boundItem = item;

        if (titleText != null)
            titleText.text = item != null ? item.title : "Untitled Room";

        if (thumbnail != null)
            thumbnail.enabled = false;
    }

    void Open()
    {
        NavigationManager.Instance?.SelectTab(AppTab.ScanAR);
        UIManager.Instance?.ShowToast(boundItem != null
            ? "Opening " + boundItem.title
            : "Opening Scan/AR");
    }
}
