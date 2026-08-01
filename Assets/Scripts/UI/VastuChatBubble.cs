using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VastuChatBubble : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] Image background;
    [SerializeField] Color userColor = new Color(0.2f, 0.5f, 0.9f, 0.9f);
    [SerializeField] Color assistantColor = new Color(0.25f, 0.25f, 0.28f, 0.95f);

    public void Bind(VastuMessage message)
    {
        if (messageText != null)
            messageText.text = message.text;

        if (background != null)
            background.color = message.isUser ? userColor : assistantColor;
    }
}
