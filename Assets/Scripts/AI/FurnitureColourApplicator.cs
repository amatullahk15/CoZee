
using UnityEngine;

public class FurnitureColourApplicator : MonoBehaviour
{
    [Header("Furniture Settings")]
    [SerializeField] private Renderer[] furnitureRenderers;

    [Header("Optional")]
    [SerializeField] private bool includeChildRenderers = true;

    public void ApplyRecommendedColour(GameObject furniture, string colourName)
    {
        if (furniture == null)
        {
            Debug.LogError("[CoZee AI] No furniture object selected.");
            return;
        }

        Color colour;

        switch (colourName.Trim().ToLowerInvariant())
        {
            case "beige":
                colour = new Color(0.82f, 0.75f, 0.61f);
                break;
            case "cream":
                colour = new Color(0.96f, 0.92f, 0.80f);
                break;
            case "grey":
                colour = new Color(0.50f, 0.50f, 0.50f);
                break;
            case "white":
                colour = Color.white;
                break;
            default:
                Debug.LogWarning("[CoZee AI] Unknown colour: " + colourName);
                return;
        }

        Renderer[] targets = includeChildRenderers
            ? furniture.GetComponentsInChildren<Renderer>(true)
            : furniture.GetComponents<Renderer>();

        if (targets.Length == 0)
        {
            Debug.LogWarning("[CoZee AI] No Renderer found on the furniture.");
            return;
        }

        foreach (Renderer target in targets)
        {
            Material[] materials = target.materials;

            foreach (Material material in materials)
            {
                if (material.HasProperty("_BaseColor"))
                    material.SetColor("_BaseColor", colour);
                else if (material.HasProperty("_Color"))
                    material.SetColor("_Color", colour);
            }
        }

        Debug.Log("[CoZee AI] Applied " + colourName + " to " + furniture.name);
    }
}
