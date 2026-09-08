using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A compact UI adapter for the existing AR furniture services.
public class ARCleanFurnitureControls : MonoBehaviour
{
    static readonly Color Teal = new Color(0.055f, 0.36f, 0.34f, 0.94f);
    static readonly Color Mint = new Color(0.82f, 0.91f, 0.87f, 1f);
    static readonly Color Ink = new Color(0.075f, 0.118f, 0.145f, 1f);
    static readonly Color Danger = new Color(0.94f, 0.27f, 0.27f, 1f);

    public static ARCleanFurnitureControls Create(Transform parent)
    {
        Transform existing = parent.Find("CleanFurnitureControls");
        if (existing != null)
        {
            ARCleanFurnitureControls existingControls = existing.GetComponent<ARCleanFurnitureControls>();
            existing.gameObject.SetActive(true);
            existingControls.ApplyPresentation();
            existing.SetAsLastSibling();
            return existingControls;
        }

        GameObject panel = new GameObject("CleanFurnitureControls", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(HorizontalLayoutGroup), typeof(ARCleanFurnitureControls));
        panel.transform.SetParent(parent, false);
        ARCleanFurnitureControls controls = panel.GetComponent<ARCleanFurnitureControls>();
        controls.AddButton("Sofa", "\uf4b8", Mint, controls.SelectSofa);
        controls.AddButton("Wardrobe", "\uf52b", Mint, controls.SelectWardrobe);
        controls.AddHoldButton("Left", "\uf2ea", Mint, true);
        controls.AddButton("Delete", "\uf1f8", Danger, controls.DeleteSelected);
        controls.AddHoldButton("Right", "\uf2f9", Mint, false);
        controls.ApplyPresentation();
        panel.transform.SetAsLastSibling();
        return controls;
    }

    // Reapply this every time Scan opens because Unity preserves the additive UI instance.
    void ApplyPresentation()
    {
        RectTransform panelRect = transform as RectTransform;
        panelRect.anchorMin = new Vector2(0.03f, 0f);
        panelRect.anchorMax = new Vector2(0.97f, 0.10f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = GetComponent<Image>();
        if (panelImage != null)
            panelImage.color = Teal;

        HorizontalLayoutGroup layout = GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
        {
            layout.padding = new RectOffset(10, 10, 8, 8);
            layout.spacing = 7f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
        }

        foreach (Button button in GetComponentsInChildren<Button>(true))
        {
            bool isDelete = button.name == "DeleteButton";
            Color background = isDelete ? Danger : Mint;
            button.transition = Selectable.Transition.None;

            Image image = button.GetComponent<Image>();
            if (image != null)
                image.color = background;

            TextMeshProUGUI icon = button.transform.Find("Icon")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI label = button.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            if (icon != null)
                icon.color = isDelete ? Color.white : Ink;
            if (label != null)
                label.color = isDelete ? Color.white : Ink;
        }
    }

    void AddButton(string label, string iconGlyph, Color background, UnityEngine.Events.UnityAction action)
    {
        Button button = CreateButton(label, iconGlyph, background);
        button.onClick.AddListener(action);
    }

    void AddHoldButton(string label, string iconGlyph, Color background, bool left)
    {
        Button button = CreateButton(label, iconGlyph, background);
        HoldButtonTrigger trigger = button.gameObject.AddComponent<HoldButtonTrigger>();
        trigger.onPress.AddListener(() => SetRotation(left, true));
        trigger.onRelease.AddListener(() => SetRotation(left, false));
    }

    Button CreateButton(string label, string iconGlyph, Color background)
    {
        GameObject buttonObject = new GameObject(label + "Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(transform, false);
        buttonObject.GetComponent<Image>().color = background;
        buttonObject.GetComponent<Button>().transition = Selectable.Transition.None;
        LayoutElement element = buttonObject.GetComponent<LayoutElement>();
        element.minWidth = 62f;
        element.flexibleWidth = 1f;

        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        iconObject.transform.SetParent(buttonObject.transform, false);
        TextMeshProUGUI icon = iconObject.GetComponent<TextMeshProUGUI>();
        icon.font = FontAwesomeIcons.FontAsset;
        icon.text = iconGlyph;
        icon.fontSize = 13f;
        icon.color = background == Danger ? Color.white : Ink;
        icon.alignment = TextAlignmentOptions.Center;
        icon.raycastTarget = false;
        Anchor(icon.rectTransform, new Vector2(0.12f, 0.48f), new Vector2(0.88f, 0.90f));

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);
        TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset font = CoZeeTypography.FontFor(label, 10f, FontStyles.Bold);
        if (font != null)
            text.font = font;
        text.text = label;
        text.fontSize = 10f;
        text.fontStyle = FontStyles.Bold;
        text.color = background == Danger ? Color.white : Ink;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        Anchor(text.rectTransform, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.48f));
        return buttonObject.GetComponent<Button>();
    }

    void SelectSofa()
    {
        FurnitureSelector selector = FindObjectOfType<FurnitureSelector>();
        selector?.SelectSofa();
        UIManager.Instance?.ShowToast(selector != null ? "Sofa selected. Tap a surface to place it." : "Furniture controls are loading.");
    }

    void SelectWardrobe()
    {
        FurnitureSelector selector = FindObjectOfType<FurnitureSelector>();
        selector?.SelectWardrobe();
        UIManager.Instance?.ShowToast(selector != null ? "Wardrobe selected. Tap a surface to place it." : "Furniture controls are loading.");
    }

    void SetRotation(bool left, bool active)
    {
        FurnitureRotation rotation = FindObjectOfType<FurnitureRotation>();
        if (rotation == null)
            return;

        if (left)
        {
            if (active) rotation.StartRotateLeft(); else rotation.StopRotateLeft();
        }
        else
        {
            if (active) rotation.StartRotateRight(); else rotation.StopRotateRight();
        }
    }

    void DeleteSelected()
    {
        DeleteFurniture deleteFurniture = FindObjectOfType<DeleteFurniture>();
        if (deleteFurniture != null && deleteFurniture.TryDeleteSelectedFurniture())
        {
            UIManager.Instance?.ShowToast("Furniture deleted.");
            return;
        }

        FurnitureInteraction interaction = FindObjectOfType<FurnitureInteraction>();
        if (interaction == null || interaction.selectedObject == null)
        {
            UIManager.Instance?.ShowToast("Double-tap furniture to select it first.");
            return;
        }

        GameObject selected = interaction.selectedObject;
        interaction.Deselect();
        Destroy(selected);
        UIManager.Instance?.ShowToast("Furniture deleted.");
    }

    static void Anchor(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
