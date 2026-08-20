using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ScanARScreenController : ScreenBase
{
    const string ArSceneName = "SampleScene";

    [SerializeField] Button backButton;
    [SerializeField] Button saveRoomButton;
    [SerializeField] ARSessionBridge bridge;

    Image shellBgImage;
    TextMeshProUGUI saveRoomButtonLabel;
    Color originalBgColor = new Color(0.06f, 0.09f, 0.16f, 1f);
    bool arLoaded;
    bool arLoading;

    protected override void OnShow()
    {
        SetShellBackgroundTransparent(true);
        LoadAR();
    }

    protected override void OnHide()
    {
        SetShellBackgroundTransparent(false);
        UnloadAR();
    }

    void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(() => NavigationManager.Instance?.SelectTab(AppTab.Home));

        if (saveRoomButton != null)
            saveRoomButton.onClick.AddListener(OnPrimaryActionClicked);
    }

    void Update()
    {
        RefreshPrimaryAction();
    }

    void SetShellBackgroundTransparent(bool transparent)
    {
        if (shellBgImage == null)
        {
            var canvasBg = GameObject.Find("CanvasBackground");
            if (canvasBg != null)
                shellBgImage = canvasBg.GetComponent<Image>();
        }

        if (shellBgImage != null)
        {
            shellBgImage.color = transparent ? Color.clear : originalBgColor;
        }
    }

    void LoadAR()
    {
        if (arLoaded || arLoading || SceneLoader.Instance == null)
            return;

        if (SceneLoader.IsSceneLoaded(ArSceneName))
        {
            arLoaded = true;
            FinalizeARScene();
            return;
        }

        arLoading = true;
        SceneLoader.Instance.LoadSceneAdditive(ArSceneName, () =>
        {
            arLoading = false;
            arLoaded = true;
            FinalizeARScene();
        });
    }

    void FinalizeARScene()
    {
        DisableDuplicateEventSystems();
        SetSampleSceneActive(true);
    }

    void DisableDuplicateEventSystems()
    {
        EventSystem[] systems = FindObjectsOfType<EventSystem>();
        EventSystem keep = EventSystem.current;

        if (keep == null && systems.Length > 0)
            keep = systems[0];

        foreach (EventSystem system in systems)
        {
            if (system != keep)
                system.gameObject.SetActive(false);
        }
    }

    void SetSampleSceneActive(bool active)
    {
        Scene scene = SceneManager.GetSceneByName(ArSceneName);
        if (!scene.isLoaded)
            return;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            root.SetActive(active);
        }
    }

    void UnloadAR()
    {
        if (!arLoaded && !arLoading)
            return;

        if (SceneLoader.Instance == null)
            return;

        SceneLoader.Instance.UnloadScene(ArSceneName, () =>
        {
            arLoaded = false;
            arLoading = false;
        });
    }

    void SaveRoom()
    {
        if (LibraryDataManager.Instance == null)
            return;

        string title = bridge != null && bridge.IsMeasurementComplete
            ? "Saved Room"
            : "Room Scan";

        LibraryDataManager.Instance.AddItem(title, "rooms");
        UIManager.Instance?.ShowToast("Room saved to Library");
    }

    void OnPrimaryActionClicked()
    {
        if (bridge == null)
            return;

        if (bridge.IsScanning)
        {
            bool completed = bridge.StopScan();
            UIManager.Instance?.ShowToast(completed
                ? "Surface scan saved"
                : "No stable wall or floor was captured");
            return;
        }

        if (bridge.IsMeasurementComplete && !bridge.CanStartScan)
        {
            SaveRoom();
            return;
        }

        if (bridge.StartScan())
        {
            UIManager.Instance?.ShowToast("Scanning started");
            return;
        }

        UIManager.Instance?.ShowToast("Move the camera until a wall or floor is detected");
    }

    void RefreshPrimaryAction()
    {
        if (saveRoomButton == null)
            return;

        if (saveRoomButtonLabel == null)
            saveRoomButtonLabel = saveRoomButton.GetComponentInChildren<TextMeshProUGUI>(true);

        bool canShow = bridge != null && (bridge.CanStartScan || bridge.IsScanning || bridge.IsMeasurementComplete);

        saveRoomButton.gameObject.SetActive(canShow);
        saveRoomButton.interactable = bridge != null
            && (bridge.IsScanning || bridge.CanStartScan || bridge.IsMeasurementComplete);

        if (saveRoomButtonLabel != null)
            saveRoomButtonLabel.text = bridge != null ? bridge.GetPrimaryActionLabel() : "Searching...";
    }
}
