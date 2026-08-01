using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VastuChatView : MonoBehaviour
{
    [SerializeField] Transform contentRoot;
    [SerializeField] VastuChatBubble bubblePrefab;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] Button sendButton;
    [SerializeField] ScrollRect scrollRect;

    readonly List<VastuChatBubble> bubbles = new List<VastuChatBubble>();

    void Awake()
    {
        if (contentRoot == null)
        {
            var existing = transform.Find("ChatContent");
            contentRoot = existing != null ? existing : transform;
        }
    }

    void Start()
    {
        if (sendButton == null)
            sendButton = GetComponentInChildren<Button>(true);

        if (sendButton != null)
            sendButton.onClick.AddListener(Send);

        if (VastuAssistantManager.Instance != null)
        {
            VastuAssistantManager.Instance.EnsureWelcomeMessage();
            VastuAssistantManager.Instance.OnMessageAdded += AddBubble;
            ReplayExistingMessages();
        }
    }

    void OnDestroy()
    {
        if (VastuAssistantManager.Instance != null)
            VastuAssistantManager.Instance.OnMessageAdded -= AddBubble;
    }

    void ReplayExistingMessages()
    {
        foreach (VastuMessage message in VastuAssistantManager.Instance.GetMessages())
            AddBubble(message);
    }

    void Send()
    {
        if (inputField == null || VastuAssistantManager.Instance == null)
            return;

        VastuAssistantManager.Instance.SendUserMessage(inputField.text);
        inputField.text = string.Empty;
    }

    void AddBubble(VastuMessage message)
    {
        if (contentRoot == null)
            return;

        VastuChatBubble bubble = bubblePrefab != null
            ? Instantiate(bubblePrefab, contentRoot)
            : RuntimeUIFactory.CreateChatBubble(contentRoot);

        bubble.Bind(message);
        bubbles.Add(bubble);

        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }
}
