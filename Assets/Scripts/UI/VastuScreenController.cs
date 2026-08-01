using UnityEngine;

public class VastuScreenController : ScreenBase
{
    [SerializeField] VastuChatView chatView;
    [SerializeField] RoomDirectionSelector directionSelector;

    protected override void OnShow()
    {
        EnsureViews();
        VastuAssistantManager.Instance?.EnsureWelcomeMessage();
    }

    void Awake()
    {
        EnsureViews();
    }

    void EnsureViews()
    {
        if (chatView == null)
            chatView = GetComponentInChildren<VastuChatView>(true);

        if (directionSelector == null)
            directionSelector = GetComponentInChildren<RoomDirectionSelector>(true);
    }
}
