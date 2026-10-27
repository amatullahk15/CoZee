
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class CozeeRecommendationRequest
{
    public string dominant_color_1;
    public string dominant_color_2;
    public string dominant_color_3;
    public string style;
    public string room_type;
    public string space;
    public string lighting;
}

[Serializable]
public class CozeeRecommendationResponse
{
    public string recommended_furniture_color;
    public float confidence;
    public string model;
}

public class CozeeRecommendationClient : MonoBehaviour
{
    [Header("API Configuration")]
    [SerializeField]
    private string apiBaseUrl = "http://127.0.0.1:8000";

    [SerializeField]
    private float timeoutSeconds = 30f;

    public bool IsBusy { get; private set; }

    public event Action<CozeeRecommendationResponse> OnRecommendationReceived;
    public event Action<string> OnRecommendationError;

    public void RecommendFromFeatures(
        string color1,
        string color2,
        string color3,
        string style,
        string roomType,
        string space,
        string lighting)
    {
        if (IsBusy)
        {
            Debug.LogWarning("[CoZee AI] A recommendation is already running.");
            return;
        }

        CozeeRecommendationRequest data = new CozeeRecommendationRequest
        {
            dominant_color_1 = color1,
            dominant_color_2 = color2,
            dominant_color_3 = color3,
            style = style,
            room_type = roomType,
            space = space,
            lighting = lighting
        };

        string json = JsonUtility.ToJson(data);
        StartCoroutine(SendRecommendation(json));
    }

    private IEnumerator SendRecommendation(string json)
    {
        IsBusy = true;

        string url = apiBaseUrl.TrimEnd('/') + "/recommend";
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");
            request.timeout = Mathf.CeilToInt(timeoutSeconds);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string error =
                    "CoZee recommendation failed: " + request.error +
                    " (HTTP " + request.responseCode + ")";

                Debug.LogError(error);
                OnRecommendationError?.Invoke(error);
                IsBusy = false;
                yield break;
            }

            CozeeRecommendationResponse response = null;

            try
            {
                response = JsonUtility.FromJson<CozeeRecommendationResponse>(
                    request.downloadHandler.text);
            }
            catch (Exception ex)
            {
                string error = "Could not parse recommendation response: " + ex.Message;
                Debug.LogError(error);
                OnRecommendationError?.Invoke(error);
                IsBusy = false;
                yield break;
            }

            if (response == null ||
                string.IsNullOrEmpty(response.recommended_furniture_color))
            {
                string error = "The API returned no recommended furniture colour.";
                Debug.LogError(error);
                OnRecommendationError?.Invoke(error);
                IsBusy = false;
                yield break;
            }

            Debug.Log(
                "[CoZee AI] Recommended furniture colour: " +
                response.recommended_furniture_color);

            OnRecommendationReceived?.Invoke(response);
        }

        IsBusy = false;
    }

    public void SetApiBaseUrl(string url)
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            apiBaseUrl = url.Trim().TrimEnd('/');
        }
    }
}
