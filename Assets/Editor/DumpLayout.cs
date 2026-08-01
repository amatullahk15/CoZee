using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.IO;
using UnityEditor.SceneManagement;

public static class DumpLayout
{
    [MenuItem("ARFurniture/Dump UI Layout")]
    public static void Dump()
    {
        MobileAppSceneBuilder.SetupAllScenes();
        EditorSceneManager.OpenScene("Assets/Scenes/UI/MainShell.unity");
        Canvas.ForceUpdateCanvases();
        
        GameObject homeTab = GameObject.Find("HomeTab");
        if (homeTab == null)
        {
            File.WriteAllText("e:/Unity/CoZee/dump.txt", "HomeTab not found.");
            return;
        }
        
        StringBuilder sb = new StringBuilder();
        DumpTransform(homeTab.GetComponent<RectTransform>(), sb, 0);
        File.WriteAllText("e:/Unity/CoZee/dump.txt", sb.ToString());
        Debug.Log("Dumped layout to e:/Unity/CoZee/dump.txt");
    }
    
    static void DumpTransform(RectTransform t, StringBuilder sb, int indent)
    {
        string ind = new string(' ', indent * 2);
        sb.AppendLine($"{ind}- {t.name} | rect: {t.rect} | sizeDelta: {t.sizeDelta} | anchors: {t.anchorMin}->{t.anchorMax} | pos: {t.anchoredPosition}");
        
        var vlg = t.GetComponent<VerticalLayoutGroup>();
        if (vlg) sb.AppendLine($"{ind}  [VLG] childControlWidth:{vlg.childControlWidth}, childControlHeight:{vlg.childControlHeight}, forceExpW:{vlg.childForceExpandWidth}, forceExpH:{vlg.childForceExpandHeight}");
        
        var hlg = t.GetComponent<HorizontalLayoutGroup>();
        if (hlg) sb.AppendLine($"{ind}  [HLG] childControlWidth:{hlg.childControlWidth}, childControlHeight:{hlg.childControlHeight}, forceExpW:{hlg.childForceExpandWidth}, forceExpH:{hlg.childForceExpandHeight}");
        
        var le = t.GetComponent<LayoutElement>();
        if (le) sb.AppendLine($"{ind}  [LE] prefW:{le.preferredWidth}, prefH:{le.preferredHeight}, flexW:{le.flexibleWidth}, flexH:{le.flexibleHeight}");
        
        var csf = t.GetComponent<ContentSizeFitter>();
        if (csf) sb.AppendLine($"{ind}  [CSF] horz:{csf.horizontalFit}, vert:{csf.verticalFit}");

        var txt = t.GetComponent<TMPro.TextMeshProUGUI>();
        if (txt) sb.AppendLine($"{ind}  [TMP] text:'{txt.text.Replace("\n", "\\n")}' | bounds: {txt.bounds}");
        
        foreach (Transform child in t)
        {
            DumpTransform(child.GetComponent<RectTransform>(), sb, indent + 1);
        }
    }
}
