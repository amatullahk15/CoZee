using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PermissionRowUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] Button requestButton;

    AppPermission permission;

    void Awake()
    {
        if (requestButton != null)
            requestButton.onClick.AddListener(Request);
    }

    public void Setup(AppPermission permission, string title)
    {
        this.permission = permission;

        if (titleText != null)
            titleText.text = title;

        Refresh();
    }

    void OnEnable()
    {
        if (PermissionManager.Instance != null)
            PermissionManager.Instance.OnPermissionUpdated += OnPermissionUpdated;

        Refresh();
    }

    void OnDisable()
    {
        if (PermissionManager.Instance != null)
            PermissionManager.Instance.OnPermissionUpdated -= OnPermissionUpdated;
    }

    void OnPermissionUpdated(AppPermission updated, bool granted)
    {
        if (updated == permission)
            Refresh();
    }

    void Request()
    {
        PermissionManager.Instance?.Request(permission);
        Refresh();
    }

    void Refresh()
    {
        bool granted = PermissionManager.Instance != null
            && PermissionManager.Instance.IsGranted(permission);

        if (statusText != null)
            statusText.text = granted ? "Granted" : "Required";

        if (requestButton != null)
            requestButton.interactable = !granted;
    }
}
