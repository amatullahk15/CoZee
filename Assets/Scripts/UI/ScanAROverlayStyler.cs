using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LegacyText = UnityEngine.UI.Text;

// Presentation only: keeps the existing AR scene controls and their bindings intact.
public static class ScanAROverlayStyler
{
    static readonly Color Teal = new Color(0.055f, 0.36f, 0.34f, 0.94f);
    static readonly Color Mint = new Color(0.82f, 0.91f, 0.87f, 1f);
    static readonly Color DisabledMint = new Color(0.42f, 0.52f, 0.50f, 0.92f);
    static readonly Color Sand = new Color(0.93f, 0.86f, 0.73f, 1f);
    static readonly Color DisabledSand = new Color(0.56f, 0.51f, 0.43f, 0.92f);
    static readonly Color Ink = new Color(0.075f, 0.118f, 0.145f, 1f);
    static readonly Color Danger = new Color(0.94f, 0.27f, 0.27f, 1f);

    public static bool Apply(Transform scanTab, Button scanActionButton, Button saveRoomButton)
    {
        if (scanTab == null || scanActionButton == null || saveRoomButton == null)
            return false;

        StyleTopBar(scanTab);
        HideMainShellDuplicates(scanTab);
        Transform actionTray = MoveScanActionTrayToRoot(scanTab);
        StyleScanActions(actionTray, scanActionButton, saveRoomButton);
        RemoveLegacyCleanFurniturePanel(scanTab);
        ARCleanFurnitureControls.Create(scanTab.root);
        ScanMeasurementPill.Create(scanTab.root);
        HideLegacyDistanceText();
        HideLegacyArBottomPanel();

        bool arControlsFound = false;
        foreach (Button button in Object.FindObjectsOfType<Button>(true))
        {
            if (button.gameObject.scene.name != "SampleScene")
                continue;

            if (button.gameObject.name == "SofaItem")
            {
                StyleArButton(button, "Sofa", "\uf1b0", Mint);
                arControlsFound = true;
            }
            else if (button.gameObject.name == "WardrobeItem")
            {
                StyleArButton(button, "Wardrobe", "\uf49e", Mint);
                arControlsFound = true;
            }
            else if (button.gameObject.name == "RotateLeftButton")
            {
                StyleArButton(button, "Rotate left", "\uf2ea", Mint);
                arControlsFound = true;
            }
            else if (button.gameObject.name == "RotateRightButton")
            {
                StyleArButton(button, "Rotate right", "\uf2f9", Mint);
                arControlsFound = true;
            }
            else if (button.gameObject.name == "DeleteButton")
            {
                StyleArButton(button, "Delete", "\uf1f8", Danger);
                arControlsFound = true;
            }
        }

        return arControlsFound || scanTab.root.Find("CleanFurnitureControls") != null;
    }

    static void RemoveLegacyCleanFurniturePanel(Transform scanTab)
    {
        Transform legacyPanel = scanTab.Find("CleanFurnitureControls");
        if (legacyPanel != null && legacyPanel.parent != scanTab.root)
            Object.Destroy(legacyPanel.gameObject);
    }

    static void HideMainShellDuplicates(Transform scanTab)
    {
        SetActive(scanTab.Find("ARControls"), false);
        SetActive(scanTab.Find("FurnitureTray/SofaBtn"), false);
        SetActive(scanTab.Find("FurnitureTray/WardrobeBtn"), false);
        SetActive(scanTab.Find("FurnitureTray/MeasureBtn"), false);
    }

    static void StyleTopBar(Transform scanTab)
    {
        Transform topBar = scanTab.Find("ARTopBar");
        if (topBar == null)
            return;

        Image topBarImage = topBar.GetComponent<Image>();
        if (topBarImage != null)
        {
            topBarImage.color = Color.clear;
            topBarImage.raycastTarget = false;
        }

        HorizontalLayoutGroup layout = topBar.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
            layout.enabled = false;

        SetActive(topBar.Find("ARSettingsBtn"), false);
        SetActive(topBar.Find("MeasurementStatusCard"), false);

        Button homeButton = topBar.Find("ARBackButton")?.GetComponent<Button>();
        if (homeButton == null)
            return;

        RectTransform homeRect = homeButton.transform as RectTransform;
        SetActionRect(homeRect, new Vector2(0f, 0.15f), new Vector2(0.20f, 0.85f));

        Image homeImage = homeButton.GetComponent<Image>();
        if (homeImage != null)
            homeImage.color = Mint;

        TextMeshProUGUI label = homeButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
        {
            label.text = "Home";
            label.fontSize = 13f;
            label.fontStyle = FontStyles.Bold;
            label.color = Ink;
            label.alignment = TextAlignmentOptions.MidlineRight;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
            SetActionRect(label.rectTransform, new Vector2(0.37f, 0f), new Vector2(0.90f, 1f));
        }

        Transform iconTransform = homeButton.transform.Find("HomeIcon");
        TextMeshProUGUI icon = iconTransform != null ? iconTransform.GetComponent<TextMeshProUGUI>() : null;
        if (icon == null)
        {
            GameObject iconObject = new GameObject("HomeIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            iconObject.transform.SetParent(homeButton.transform, false);
            icon = iconObject.GetComponent<TextMeshProUGUI>();
        }

        icon.font = FontAwesomeIcons.FontAsset;
        icon.text = "\uf015";
        icon.fontSize = 14f;
        icon.color = Ink;
        icon.alignment = TextAlignmentOptions.Center;
        icon.raycastTarget = false;
        SetActionRect(icon.rectTransform, new Vector2(0.12f, 0f), new Vector2(0.35f, 1f));
    }

    // The action tray shares the root canvas with furniture controls so both align to the device edge.
    static Transform MoveScanActionTrayToRoot(Transform scanTab)
    {
        Transform root = scanTab.root;
        Transform tray = root.Find("ScanActionTray");
        if (tray != null)
        {
            tray.gameObject.SetActive(true);
            return tray;
        }

        tray = scanTab.Find("FurnitureTray");
        if (tray == null)
            return null;

        tray.SetParent(root, false);
        tray.name = "ScanActionTray";
        tray.gameObject.SetActive(true);
        tray.SetAsLastSibling();
        return tray;
    }

    static void StyleScanActions(Transform tray, Button scanActionButton, Button saveRoomButton)
    {
        if (tray != null)
        {
            HorizontalLayoutGroup layout = tray.GetComponent<HorizontalLayoutGroup>();
            if (layout != null)
                layout.enabled = false;

            Image trayImage = tray.GetComponent<Image>();
            if (trayImage != null)
                trayImage.color = Teal;
        }

        RectTransform trayRect = tray as RectTransform;
        if (trayRect != null)
            SetActionRect(trayRect, new Vector2(0.03f, 0.115f), new Vector2(0.97f, 0.20f));

        SetActionRect(scanActionButton.transform as RectTransform, new Vector2(0.08f, 0.18f), new Vector2(0.47f, 0.82f));
        SetActionRect(saveRoomButton.transform as RectTransform, new Vector2(0.53f, 0.18f), new Vector2(0.92f, 0.82f));
        StyleActionButton(scanActionButton, Mint);
        StyleActionButton(saveRoomButton, Sand);
    }

    public static void UpdateActionAvailability(Button scanActionButton, Button saveRoomButton)
    {
        SetActionAvailability(scanActionButton, scanActionButton != null && scanActionButton.interactable, Mint, DisabledMint);
        SetActionAvailability(saveRoomButton, saveRoomButton != null && saveRoomButton.interactable, Sand, DisabledSand);
    }

    static void SetActionAvailability(Button button, bool active, Color activeColor, Color disabledColor)
    {
        if (button == null)
            return;

        Image image = button.GetComponent<Image>();
        if (image != null)
            image.color = active ? activeColor : disabledColor;

        TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
            label.color = active ? Ink : new Color(0.78f, 0.82f, 0.80f, 1f);
    }

    static void StyleActionButton(Button button, Color background)
    {
        Image image = button.GetComponent<Image>();
        if (image != null)
            image.color = background;

        TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
        {
            label.fontSize = 14f;
            label.fontStyle = FontStyles.Bold;
            label.color = Ink;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
        }
    }

    static void StyleArButton(Button button, string label, string iconGlyph, Color background)
    {
        Image image = button.GetComponent<Image>();
        if (image != null)
            image.color = background;

        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text != null)
        {
            text.text = label;
            text.fontSize = 14f;
            text.fontStyle = FontStyles.Bold;
            text.color = background == Danger ? Color.white : Ink;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.rectTransform.anchorMin = new Vector2(0.34f, 0f);
            text.rectTransform.anchorMax = new Vector2(0.94f, 1f);
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
        }

        LegacyText legacyText = button.GetComponentInChildren<LegacyText>(true);
        if (legacyText != null)
        {
            legacyText.text = label;
            legacyText.fontSize = 14;
            legacyText.fontStyle = FontStyle.Bold;
            legacyText.color = background == Danger ? Color.white : Ink;
            legacyText.alignment = TextAnchor.MiddleLeft;
            RectTransform legacyRect = legacyText.rectTransform;
            legacyRect.anchorMin = new Vector2(0.34f, 0f);
            legacyRect.anchorMax = new Vector2(0.94f, 1f);
            legacyRect.offsetMin = Vector2.zero;
            legacyRect.offsetMax = Vector2.zero;
        }

        Transform existingIcon = button.transform.Find("OverlayIcon");
        TextMeshProUGUI icon = existingIcon != null ? existingIcon.GetComponent<TextMeshProUGUI>() : null;
        if (icon == null)
        {
            GameObject iconObject = new GameObject("OverlayIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            iconObject.transform.SetParent(button.transform, false);
            icon = iconObject.GetComponent<TextMeshProUGUI>();
        }

        icon.font = FontAwesomeIcons.FontAsset;
        icon.text = iconGlyph;
        icon.fontSize = 15f;
        icon.color = background == Danger ? Color.white : Ink;
        icon.alignment = TextAlignmentOptions.Center;
        icon.raycastTarget = false;
        icon.rectTransform.anchorMin = new Vector2(0.10f, 0f);
        icon.rectTransform.anchorMax = new Vector2(0.28f, 1f);
        icon.rectTransform.offsetMin = Vector2.zero;
        icon.rectTransform.offsetMax = Vector2.zero;
        icon.transform.SetAsFirstSibling();
    }

    static void HideLegacyArBottomPanel()
    {
        foreach (Image image in Object.FindObjectsOfType<Image>(true))
        {
            if (image.gameObject.scene.name != "SampleScene")
                continue;

            if (image.gameObject.name == "BottomPanel")
                image.gameObject.SetActive(false);
        }
    }

    static void HideLegacyDistanceText()
    {
        foreach (TextMeshProUGUI text in Object.FindObjectsOfType<TextMeshProUGUI>(true))
        {
            if (text.gameObject.scene.name == "SampleScene" && text.gameObject.name == "DistanceText")
                text.gameObject.SetActive(false);
        }
    }

    static void SetActionRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        if (rect == null)
            return;

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static void SetActive(Transform target, bool active)
    {
        if (target != null && target.gameObject.activeSelf != active)
            target.gameObject.SetActive(active);
    }
}
