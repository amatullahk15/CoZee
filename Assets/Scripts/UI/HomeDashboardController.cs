using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HomeDashboardController : ScreenBase
{
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
        TMP_FontAsset font = TMP_Settings.defaultFontAsset ?? Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        TextMeshProUGUI[] tmps = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmp in tmps)
        {
            if (tmp != null)
            {
                if (tmp.font == null && font != null)
                    tmp.font = font;

                tmp.color = Color.white;
                tmp.overflowMode = TextOverflowModes.Overflow;
            }
        }
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
