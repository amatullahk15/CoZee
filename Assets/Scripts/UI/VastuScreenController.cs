using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VastuScreenController : ScreenBase
{
    [SerializeField] VastuChatView chatView;
    [SerializeField] RoomDirectionSelector directionSelector;
    [SerializeField] Button compassToggleBtn;
    [SerializeField] Button checkLayoutBtn;

    bool compassOn = true;
    VastuDashboardView dashboard;

    protected override void OnShow()
    {
        EnsureViews();
        VastuAssistantManager.Instance?.EnsureWelcomeMessage();
        EnsureDashboard();
        dashboard?.RefreshRooms();
    }

    void Awake()
    {
        EnsureViews();
        EnsureDashboard();
    }

    void Start()
    {
        if (compassToggleBtn != null)
            compassToggleBtn.onClick.AddListener(ToggleCompass);
        
        if (checkLayoutBtn != null)
            checkLayoutBtn.onClick.AddListener(CheckLayout);
    }

    void EnsureViews()
    {
        if (chatView == null)
            chatView = GetComponentInChildren<VastuChatView>(true);

        if (directionSelector == null)
            directionSelector = GetComponentInChildren<RoomDirectionSelector>(true);
    }

    void EnsureDashboard()
    {
        if (dashboard == null)
            dashboard = GetComponentInChildren<VastuDashboardView>(true);

        if (dashboard == null)
        {
            // Preserve the scene's legacy Vastu controls, but do not let them overlap the unified dashboard.
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(false);

            GameObject root = new GameObject("VastuDashboard", typeof(RectTransform));
            root.transform.SetParent(transform, false);
            dashboard = root.AddComponent<VastuDashboardView>();
        }

        dashboard.Build();
    }

    void ToggleCompass()
    {
        compassOn = !compassOn;
        var text = compassToggleBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = compassOn ? "ON" : "OFF";
            text.color = compassOn ? Color.white : new Color(1f, 1f, 1f, 0.5f);
        }
        UIManager.Instance?.ShowToast(compassOn ? "Compass View Enabled" : "Compass View Disabled");
    }

    void CheckLayout()
    {
        // Delegate to the AI scan via VastuApiClient instead of the old stub simulation.
        if (VastuApiClient.Instance == null)
        {
            UIManager.Instance?.ShowToast("VastuApiClient not found in scene.");
            return;
        }

        if (VastuApiClient.Instance.IsBusy)
        {
            UIManager.Instance?.ShowToast("Scan already in progress…");
            return;
        }

        // Default to livingroom; VastuDashboardView.OnScanWithAIClicked infers a better
        // room type from the selected library item — keep this as the quick-check fallback.
        VastuApiClient.Instance.AnalyzeCurrentRoom("livingroom");
    }
}
