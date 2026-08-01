using System;
using System.Collections.Generic;
using UnityEngine;

public enum RoomDirection
{
    North,
    East,
    South,
    West
}

[Serializable]
public class VastuMessage
{
    public bool isUser;
    public string text;
}

public class VastuAssistantManager : MonoBehaviour
{
    public static VastuAssistantManager Instance { get; private set; }

    public event Action<VastuMessage> OnMessageAdded;

    readonly List<VastuMessage> messages = new List<VastuMessage>();

    [SerializeField] RoomDirection selectedDirection = RoomDirection.North;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IReadOnlyList<VastuMessage> GetMessages() => messages;
    public RoomDirection SelectedDirection => selectedDirection;

    public void EnsureWelcomeMessage()
    {
        if (messages.Count > 0)
            return;

        AddMessage(new VastuMessage
        {
            isUser = false,
            text = "Namaste! Select your room direction and ask about bed placement, kitchen, or living room layout."
        });
    }

    public void SetDirection(RoomDirection direction)
    {
        selectedDirection = direction;
    }

    public void SendUserMessage(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        AddMessage(new VastuMessage { isUser = true, text = text.Trim() });
        AddMessage(new VastuMessage
        {
            isUser = false,
            text = BuildAssistantReply(text.Trim(), selectedDirection)
        });
    }

    void AddMessage(VastuMessage message)
    {
        messages.Add(message);
        OnMessageAdded?.Invoke(message);
    }

    string BuildAssistantReply(string userText, RoomDirection direction)
    {
        string dir = direction.ToString();
        string lower = userText.ToLowerInvariant();

        if (lower.Contains("bed"))
            return $"For bedrooms facing {dir}, place the bed with the head toward the south or east wall when possible.";

        if (lower.Contains("kitchen"))
            return $"Kitchens work best in the southeast. Your room faces {dir}; keep the cooking zone in the southeast corner if layout allows.";

        if (lower.Contains("living") || lower.Contains("sofa"))
            return $"Living areas facing {dir} benefit from seating along the south or west walls, leaving the northeast open.";

        return $"Considering your room faces {dir}: keep the northeast clutter-free, main entrance well-lit, and heavy furniture in the southwest.";
    }
}
