
using UnityEngine;

public class CozeeFurnitureRecommendation : MonoBehaviour
{
    [Header("AI Recommendation")]
    [SerializeField] private CozeeRecommendationClient recommendationClient;
    [SerializeField] private FurnitureColourApplicator colourApplicator;

    [Header("Selected Furniture")]
    [SerializeField] private GameObject selectedFurniture;

    [Header("Room Details")]
    [SerializeField] private string dominantColor1 = "beige";
    [SerializeField] private string dominantColor2 = "white";
    [SerializeField] private string dominantColor3 = "brown";
    [SerializeField] private string interiorStyle = "modern";
    [SerializeField] private string roomType = "living";
    [SerializeField] private string roomSpace = "medium";
    [SerializeField] private string lighting = "bright";

    private void OnEnable()
    {
        if (recommendationClient == null)
            return;

        recommendationClient.OnRecommendationReceived += OnRecommendationReceived;
        recommendationClient.OnRecommendationError += OnRecommendationError;
    }

    private void OnDisable()
    {
        if (recommendationClient == null)
            return;

        recommendationClient.OnRecommendationReceived -= OnRecommendationReceived;
        recommendationClient.OnRecommendationError -= OnRecommendationError;
    }

    public void RecommendFurnitureColour()
    {
        if (recommendationClient == null)
        {
            Debug.LogError("Assign the CozeeRecommendationClient in the Inspector.");
            return;
        }

        if (colourApplicator == null)
        {
            Debug.LogError("Assign the FurnitureColourApplicator in the Inspector.");
            return;
        }

        if (selectedFurniture == null)
        {
            Debug.LogError("Select the furniture GameObject in the Inspector.");
            return;
        }

        recommendationClient.RecommendFromFeatures(
            dominantColor1,
            dominantColor2,
            dominantColor3,
            interiorStyle,
            roomType,
            roomSpace,
            lighting
        );
    }

    private void OnRecommendationReceived(CozeeRecommendationResponse response)
    {
        if (selectedFurniture == null || colourApplicator == null)
        {
            Debug.LogError("Furniture or colour applicator is missing.");
            return;
        }

        colourApplicator.ApplyRecommendedColour(
            selectedFurniture,
            response.recommended_furniture_color
        );
    }

    private void OnRecommendationError(string error)
    {
        Debug.LogError("[CoZee AI] " + error);
    }
}
