using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] ToastNotification toast;
    [SerializeField] LoadingSpinner loadingSpinner;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (this.toast != null)
                Instance.toast = this.toast;
            if (this.loadingSpinner != null)
                Instance.loadingSpinner = this.loadingSpinner;
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ShowToast(string message)
    {
        if (toast == null)
            toast = FindObjectOfType<ToastNotification>(true);

        if (toast != null)
            toast.Show(message);
        else
            Debug.Log("[Toast] " + message);
    }

    public void ShowLoading(bool visible)
    {
        if (loadingSpinner == null)
            loadingSpinner = FindObjectOfType<LoadingSpinner>(true);

        if (loadingSpinner != null)
            loadingSpinner.SetVisible(visible);
    }
}
