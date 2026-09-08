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
    Button scanActionButton;
    TextMeshProUGUI scanActionButtonLabel;
    TextMeshProUGUI saveRoomButtonLabel;
    Color originalBgColor = new Color(0.06f, 0.09f, 0.16f, 1f);
    bool arLoaded;
    bool arLoading;
    bool overlayStyled;

    protected override void OnShow()
    {
        SetShellBackgroundTransparent(true);
        SetBottomNavigationVisible(false);
        SetCleanFurnitureControlsVisible(true);
        SetScanActionTrayVisible(true);
        SetScanMeasurementPillVisible(true);
        LoadAR();
    }

    protected override void OnHide()
    {
        SetShellBackgroundTransparent(false);
        SetBottomNavigationVisible(true);
        SetCleanFurnitureControlsVisible(false);
        SetScanActionTrayVisible(false);
        SetScanMeasurementPillVisible(false);
        UnloadAR();
        overlayStyled = false;
    }

    void Start()
    {
        EnsureActionButtons();

        if (backButton != null)
            backButton.onClick.AddListener(() => NavigationManager.Instance?.SelectTab(AppTab.Home));

        if (saveRoomButton != null)
        {
            saveRoomButton.onClick.RemoveAllListeners();
            saveRoomButton.onClick.AddListener(SaveRoom);
        }

        if (scanActionButton != null)
        {
            scanActionButton.onClick.RemoveAllListeners();
            scanActionButton.onClick.AddListener(OnScanActionClicked);
        }
    }

    void Update()
    {
        EnsureActionButtons();
        RefreshActionButtons();

        if (!overlayStyled && arLoaded)
            overlayStyled = ScanAROverlayStyler.Apply(transform, scanActionButton, saveRoomButton);
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
        overlayStyled = false;
    }

    void SetBottomNavigationVisible(bool visible)
    {
        BottomNavBar bottomNav = FindObjectOfType<BottomNavBar>(true);
        if (bottomNav != null)
            bottomNav.gameObject.SetActive(visible);
    }

    void SetCleanFurnitureControlsVisible(bool visible)
    {
        Transform controls = transform.root.Find("CleanFurnitureControls");
        if (controls != null)
            controls.gameObject.SetActive(visible);
    }

    void SetScanActionTrayVisible(bool visible)
    {
        Transform tray = transform.root.Find("ScanActionTray");
        if (tray != null)
            tray.gameObject.SetActive(visible);
    }

    void SetScanMeasurementPillVisible(bool visible)
    {
        Transform pill = transform.root.Find("ScanMeasurementPill");
        if (pill != null)
            pill.gameObject.SetActive(visible);
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

        if (bridge != null && bridge.IsMeasurementComplete)
        {
            RoomScanData roomScanData = bridge.BuildRoomScanData();
            LibraryDataManager.Instance.AddRoomScan(null, roomScanData);
            UIManager.Instance?.ShowToast("Room with wall dimensions saved to Library");
            return;
        }

        LibraryDataManager.Instance.AddItem("Room Scan", "rooms");
        UIManager.Instance?.ShowToast("Room saved to Library");
    }

    void OnScanActionClicked()
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

        if (bridge.StartScan())
        {
            UIManager.Instance?.ShowToast("Scanning started");
            return;
        }

        UIManager.Instance?.ShowToast("Move the camera until a wall or floor is detected");
    }

    void EnsureActionButtons()
    {
        if (saveRoomButton == null || scanActionButton != null)
            return;

        scanActionButton = Instantiate(saveRoomButton, saveRoomButton.transform.parent);
        scanActionButton.name = "ScanActionBtn";
        scanActionButton.transform.SetSiblingIndex(saveRoomButton.transform.GetSiblingIndex());

        RectTransform scanRect = scanActionButton.transform as RectTransform;
        RectTransform saveRect = saveRoomButton.transform as RectTransform;
        if (scanRect != null && saveRect != null && scanRect.parent == saveRect.parent)
        {
            scanRect.anchorMin = saveRect.anchorMin;
            scanRect.anchorMax = saveRect.anchorMax;
            scanRect.pivot = saveRect.pivot;
            scanRect.sizeDelta = saveRect.sizeDelta;
            scanRect.anchoredPosition = saveRect.anchoredPosition + new Vector2(-220f, 0f);
        }

        scanActionButtonLabel = scanActionButton.GetComponentInChildren<TextMeshProUGUI>(true);
        saveRoomButtonLabel = saveRoomButton.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    void RefreshActionButtons()
    {
        if (saveRoomButton == null)
            return;

        if (scanActionButton == null)
            return;

        bool canScan = bridge != null && (bridge.CanStartScan || bridge.IsScanning || bridge.IsMeasurementComplete);
        bool canSave = bridge != null && bridge.IsMeasurementComplete && !bridge.IsScanning;

        scanActionButton.gameObject.SetActive(true);
        scanActionButton.interactable = bridge != null && (bridge.IsScanning || bridge.CanStartScan);

        saveRoomButton.gameObject.SetActive(true);
        saveRoomButton.interactable = canSave;

        if (scanActionButtonLabel != null)
            scanActionButtonLabel.text = bridge != null
                ? (bridge.IsScanning ? "Stop Scan" : "Start Scan")
                : "Searching...";

        if (saveRoomButtonLabel == null)
            saveRoomButtonLabel = saveRoomButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (saveRoomButtonLabel != null)
            saveRoomButtonLabel.text = "Save Room";

        ScanAROverlayStyler.UpdateActionAvailability(scanActionButton, saveRoomButton);
    }
}
