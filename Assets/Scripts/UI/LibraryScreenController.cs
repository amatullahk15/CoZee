using System.Collections.Generic;
using UnityEngine;

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
        Refresh();
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
}
