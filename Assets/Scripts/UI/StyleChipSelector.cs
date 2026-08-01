using UnityEngine;
using UnityEngine.UI;

public class StyleChipSelector : MonoBehaviour
{
    [SerializeField] Toggle[] styleToggles;
    [SerializeField] string[] styleNames;

    public string SelectedStyle
    {
        get
        {
            if (styleToggles == null || styleNames == null)
                return "Modern";

            for (int i = 0; i < styleToggles.Length && i < styleNames.Length; i++)
            {
                if (styleToggles[i] != null && styleToggles[i].isOn)
                    return styleNames[i];
            }

            return styleNames.Length > 0 ? styleNames[0] : "Modern";
        }
    }
}
