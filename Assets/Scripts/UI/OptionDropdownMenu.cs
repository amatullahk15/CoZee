using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class OptionDropdownMenu
{
    static readonly Color Cream = new Color(1f, 0.992f, 0.965f, 1f);
    static readonly Color Mint = new Color(0.82f, 0.91f, 0.87f, 1f);
    static readonly Color Sand = new Color(0.93f, 0.86f, 0.73f, 1f);
    static readonly Color Ink = new Color(0.075f, 0.118f, 0.145f, 1f);

    public static void Toggle(Transform owner, string[] options, int selectedIndex, Action<int> onSelected)
    {
        if (owner == null || options == null || options.Length == 0)
            return;

        Transform host = FindSelectionCard(owner);
        Transform existing = host.Find("OptionDropdownMenu");
        if (existing != null)
        {
            RestoreSelectionRow(host);
            UnityEngine.Object.Destroy(existing.gameObject);
            return;
        }

        CloseOtherMenus(host);

        float menuHeight = options.Length * 42f + 8f;
        ReserveSelectionRow(host, menuHeight);

        GameObject menu = new GameObject("OptionDropdownMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        menu.transform.SetParent(host, false);
        menu.transform.SetAsLastSibling();

        RectTransform menuRect = menu.GetComponent<RectTransform>();
        menuRect.anchorMin = new Vector2(0f, 0f);
        menuRect.anchorMax = new Vector2(1f, 1f);
        menuRect.offsetMin = Vector2.zero;
        menuRect.offsetMax = Vector2.zero;
        menu.GetComponent<LayoutElement>().preferredHeight = menuHeight;
        menu.GetComponent<Image>().color = Cream;

        VerticalLayoutGroup layout = menu.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(4, 4, 4, 4);
        layout.spacing = 3f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        for (int i = 0; i < options.Length; i++)
        {
            int optionIndex = i;
            GameObject option = new GameObject("Option", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            option.transform.SetParent(menu.transform, false);
            option.GetComponent<Image>().color = i == selectedIndex ? Sand : Mint;
            option.GetComponent<LayoutElement>().preferredHeight = 39f;
            option.GetComponent<Button>().onClick.AddListener(() =>
            {
                onSelected?.Invoke(optionIndex);
                RestoreSelectionRow(host);
                UnityEngine.Object.Destroy(menu);
            });

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(option.transform, false);
            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            TMP_FontAsset font = CoZeeTypography.FontFor("Option", 13f, FontStyles.Bold);
            if (font != null)
                label.font = font;
            label.text = options[i];
            label.fontSize = 13f;
            label.fontStyle = FontStyles.Bold;
            label.color = Ink;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.raycastTarget = false;
            label.rectTransform.anchorMin = new Vector2(0.10f, 0f);
            label.rectTransform.anchorMax = new Vector2(0.90f, 1f);
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
        }
    }

    static void CloseOtherMenus(Transform currentHost)
    {
        Transform row = FindSelectionRow(currentHost);
        if (row == null)
            return;

        for (int i = 0; i < row.childCount; i++)
        {
            Transform card = row.GetChild(i);
            Transform openMenu = card.Find("OptionDropdownMenu");
            if (openMenu == null)
                continue;

            RestoreSelectionRow(card);
            UnityEngine.Object.Destroy(openMenu.gameObject);
        }
    }

    static void ReserveSelectionRow(Transform host, float menuHeight)
    {
        Transform row = FindSelectionRow(host);
        if (row == null)
            return;

        float expandedHeight = 98f + menuHeight + 8f;
        LayoutElement rowLayout = row.GetComponent<LayoutElement>();
        if (rowLayout != null)
            rowLayout.preferredHeight = expandedHeight;

        SetSelectionCardHeights(row, expandedHeight);
    }

    static void RestoreSelectionRow(Transform host)
    {
        Transform row = FindSelectionRow(host);
        if (row == null)
            return;

        LayoutElement rowLayout = row.GetComponent<LayoutElement>();
        if (rowLayout != null)
            rowLayout.preferredHeight = 98f;

        SetSelectionCardHeights(row, 98f);
    }

    static Transform FindSelectionRow(Transform transform)
    {
        Transform current = transform;
        while (current != null)
        {
            if (current.name == "DesignSelectionRow")
                return current;
            current = current.parent;
        }

        return null;
    }

    static Transform FindSelectionCard(Transform transform)
    {
        Transform current = transform;
        while (current != null)
        {
            if (current.parent != null && current.parent.name == "DesignSelectionRow")
                return current;
            current = current.parent;
        }

        return transform.parent != null ? transform.parent : transform;
    }

    static void SetSelectionCardHeights(Transform row, float height)
    {
        for (int i = 0; i < row.childCount; i++)
        {
            LayoutElement cardLayout = row.GetChild(i).GetComponent<LayoutElement>();
            if (cardLayout != null)
                cardLayout.preferredHeight = height;
        }
    }
}
