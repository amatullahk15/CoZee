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

    protected override void OnShow()
    {
        EnsureViews();
        VastuAssistantManager.Instance?.EnsureWelcomeMessage();
    }

    void Awake()
    {
        EnsureViews();
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
        UIManager.Instance?.ShowLoading(true);
        // Simulate analysis
        Invoke(nameof(FinishAnalysis), 1.5f);
    }

    void FinishAnalysis()
    {
        UIManager.Instance?.ShowLoading(false);
        UIManager.Instance?.ShowToast("Vastu Analysis Updated!");
        VastuAssistantManager.Instance?.SendUserMessage("Re-analyzing layout based on current orientation...");
    }
}
