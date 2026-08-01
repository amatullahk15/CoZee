using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LibraryTabBar : MonoBehaviour
{
    [SerializeField] Button roomsTab;
    [SerializeField] Button designsTab;
    [SerializeField] Button objectsTab;
    [SerializeField] Button favoritesTab;

    public event System.Action<string> OnCategorySelected;

    void Start()
    {
        EnsureTabs();
        Select("all");
    }

    void EnsureTabs()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            string name = btn.gameObject.name.ToLowerInvariant();
            btn.onClick.RemoveAllListeners();

            if (name.Contains("all"))
            {
                btn.onClick.AddListener(() => Select("all"));
            }
            else if (name.Contains("room"))
            {
                btn.onClick.AddListener(() => Select("rooms"));
            }
            else if (name.Contains("concept") || name.Contains("design"))
            {
                btn.onClick.AddListener(() => Select("concepts"));
            }
            else if (name.Contains("favor"))
            {
                btn.onClick.AddListener(() => Select("favorites"));
            }
            else
            {
                btn.onClick.AddListener(() => Select("all"));
            }
        }
    }

    void Select(string category)
    {
        AudioManager.Instance?.PlayClick();
        OnCategorySelected?.Invoke(category);
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
