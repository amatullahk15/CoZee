using System.Collections;
using UnityEngine;
using TMPro;

public class ToastNotification : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] float displayDuration = 2f;

    Coroutine routine;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        SetVisible(false);
    }

    public void Show(string message)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(ShowRoutine(message));
    }

    IEnumerator ShowRoutine(string message)
    {
        if (messageText != null)
            messageText.text = message;

        SetVisible(true);
        yield return new WaitForSecondsRealtime(displayDuration);
        SetVisible(false);
        routine = null;
    }

    void SetVisible(bool visible)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = false;
    }
}
