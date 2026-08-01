using UnityEngine;
using TMPro;

public class SavedRoomDetailView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI dateText;

    public void Bind(LibraryItem item)
    {
        if (item == null)
            return;

        if (titleText != null)
            titleText.text = item.title;

        if (dateText != null)
            dateText.text = item.createdAt;
    }
}
