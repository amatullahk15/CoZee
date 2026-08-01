using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FavoriteToggle : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] TextMeshProUGUI iconText;

    string itemId;
    bool isFavorite;

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(Toggle);
    }

    void OnEnable()
    {
        RefreshVisual();
    }

    public void Bind(string id, bool favorite)
    {
        itemId = id;
        isFavorite = favorite;
        RefreshVisual();
    }

    void Toggle()
    {
        if (LibraryDataManager.Instance == null || string.IsNullOrEmpty(itemId))
            return;

        LibraryDataManager.Instance.ToggleFavorite(itemId);
        isFavorite = !isFavorite;
        RefreshVisual();
    }

    void RefreshVisual()
    {
        if (iconText != null)
            iconText.text = isFavorite ? "★" : "☆";
    }
}
