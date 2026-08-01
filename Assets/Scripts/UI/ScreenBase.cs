using UnityEngine;

public class ScreenBase : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] bool disableGameObjectOnHide = true;

    public bool IsVisible { get; private set; }

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void Show()
    {
        IsVisible = true;

        if (disableGameObjectOnHide)
            gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        OnShow();
    }

    public virtual void Hide()
    {
        IsVisible = false;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (disableGameObjectOnHide)
            gameObject.SetActive(false);

        OnHide();
    }

    protected virtual void OnShow() { }
    protected virtual void OnHide() { }
}
