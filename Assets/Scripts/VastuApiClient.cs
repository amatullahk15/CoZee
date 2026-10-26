using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// ══════════════════════════════════════════════════════════════════════════════
//  Response data classes — mirror the FastAPI /analyze response JSON exactly.
//  All names are snake_case to match server output.
//  [Serializable] + flat fields are required by Unity's JsonUtility.
// ══════════════════════════════════════════════════════════════════════════════

[Serializable]
public class VastuDetectionItem
{
    public string name;
    public float  confidence;
    /// <summary>
    /// Position of this furniture within the *photo frame* (e.g. "West",
    /// "South-East", "Center"). This is NOT a hardware compass reading.
    /// The server's direction_note field explains this distinction.
    /// </summary>
    public string image_zone;
    public int    box_x1;
    public int    box_y1;
    public int    box_x2;
    public int    box_y2;
    public int    center_x;
    public int    center_y;
}

[Serializable]
public class VastuResultItem
{
    public string furniture;
    public string image_zone;   // photo-frame zone, NOT compass bearing
    public int    score;        // 0-100
    public string status;       // "Excellent" | "Average" | "Poor"
    public string suggestion;
}

[Serializable]
public class VastuApiResponse
{
    public string             room_type;
    public int                image_width;
    public int                image_height;
    public VastuDetectionItem[] detections;
    public VastuResultItem[]    vastu_results;
    public int                overall_score;
    public string             summary;
    /// <summary>
    /// Human-readable note that image_zone is a photo-frame position,
    /// not an absolute compass direction. Display this in the UI.
    /// </summary>
    public string             direction_note;
}

// ══════════════════════════════════════════════════════════════════════════════
//  VastuApiClient
//  ==============
//  Captures a room image, sends it to POST /analyze on the FastAPI backend,
//  and forwards the structured response to VastuAssistantManager.
//
//  SETUP CHECKLIST (Inspector):
//  1. Attach this script to a persistent GameObject (e.g. AppManager).
//     AppManager.EnsureRequiredManagers() already creates one automatically.
//  2. Set "Server Url" to your PC's LAN IP:  http://192.168.x.x:8000
//     • Run  ipconfig  in a Windows terminal to find your Wi-Fi IPv4 address.
//     • Do NOT use "localhost" on a physical Android device — it won't reach
//       the server. "localhost" only works in the Unity Editor.
//  3. Both your PC and phone must be on the same Wi-Fi network.
//  4. The FastAPI server must be running:  double-click backend/run_server.bat
//  5. Android HTTP to LAN IPs is enabled via network_security_config.xml
//     (Assets/Plugins/Android/res/xml/) — no additional steps needed.
//
//  IMAGE CAPTURE STRATEGY:
//  • Primary:  ARCameraManager.TryAcquireLatestCpuImage (real AR frame)
//  • Fallback: Screen.width / ReadPixels (Editor and non-AR builds)
//  ScreenCapture is NOT used — it is unreliable on Android.
// ══════════════════════════════════════════════════════════════════════════════
public class VastuApiClient : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static VastuApiClient Instance { get; private set; }

    // ── Inspector fields ──────────────────────────────────────────────────────
    [Header("Server Configuration")]
    [Tooltip(
        "Full URL of your FastAPI server INCLUDING port.\n" +
        "Example: http://192.168.1.42:8000\n\n" +
        "Run  ipconfig  on your Windows PC → look for 'IPv4 Address' under your Wi-Fi adapter.\n" +
        "DO NOT use localhost or 127.0.0.1 on a physical Android device.")]
    [SerializeField] private string serverUrl = "http://192.168.1.100:8000";

    [Tooltip("Seconds before the request is abandoned (default 30 — increase for slow networks).")]
    [SerializeField] private float timeoutSeconds = 30f;

    [Tooltip("JPEG quality 1-100 sent to the server. Lower = smaller payload, faster upload (75 is a good balance).")]
    [SerializeField] [Range(50, 100)] private int jpegQuality = 75;

    [Tooltip("Run a /health check before every /analyze call. Shows a clear error if server is unreachable.")]
    [SerializeField] private bool checkHealthBeforeScan = true;

    [Header("AR References (auto-found if blank)")]
    [Tooltip("Leave blank — Start() will find ARCameraManager automatically. Only assign if auto-find fails.")]
    [SerializeField] private ARCameraManager arCameraManager;

    // ── Read-only public state ────────────────────────────────────────────────
    public bool   IsBusy    => _isBusy;
    public string ServerUrl => serverUrl;

    // ── Events ────────────────────────────────────────────────────────────────
    /// <summary>Fired after a successful /analyze response is parsed.</summary>
    public event Action<VastuApiResponse> OnAnalysisComplete;
    /// <summary>Fired when any step of the analysis fails. Arg is a human-readable error string.</summary>
    public event Action<string>           OnAnalysisError;

    // ── Private state ─────────────────────────────────────────────────────────
    private bool _isBusy = false;

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Auto-find ARCameraManager from the scene so the Inspector field
        // can stay blank in most setups.
        if (arCameraManager == null)
            arCameraManager = FindObjectOfType<ARCameraManager>();

        if (arCameraManager == null)
            Debug.LogWarning("[VastuApiClient] ARCameraManager not found in scene. " +
                             "AR camera capture will be skipped; screen-capture fallback will be used instead.");
        else
            Debug.Log("[VastuApiClient] Found ARCameraManager: " + arCameraManager.name);

        Debug.Log($"[VastuApiClient] Configured server: {serverUrl}  timeout: {timeoutSeconds}s");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Public API
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Captures the current camera frame and sends it to POST /analyze.
    /// Called by VastuDashboardView's "Scan with AI" button and
    /// VastuScreenController's "Check Layout" button.
    /// </summary>
    /// <param name="roomType">
    /// One of: "bedroom", "livingroom", "kitchen", "bathroom", "studyroom".
    /// Inferred automatically from the selected room's name in VastuDashboardView.
    /// </param>
    public void AnalyzeCurrentRoom(string roomType)
    {
        if (_isBusy)
        {
            Debug.LogWarning("[VastuApiClient] Analysis already running — ignoring duplicate call.");
            UIManager.Instance?.ShowToast("Scan already in progress…");
            return;
        }

        if (string.IsNullOrWhiteSpace(serverUrl) ||
            serverUrl == "http://192.168.1.100:8000")
        {
            // Warn loudly when the default placeholder IP is still set.
            Debug.LogWarning("[VastuApiClient] serverUrl appears to be the default placeholder. " +
                             "Update it in the Inspector to your PC's LAN IP.");
        }

        StartCoroutine(AnalysisCoroutine(roomType));
    }

    /// <summary>
    /// GET /health — quick connectivity check.
    /// Call before AnalyzeCurrentRoom to give a clear error if the server is down.
    /// </summary>
    public IEnumerator CheckServerHealth(Action<bool> callback)
    {
        string url = serverUrl.TrimEnd('/') + "/health";
        using UnityWebRequest req = UnityWebRequest.Get(url);
        req.timeout = 5;
        yield return req.SendWebRequest();
        bool ok = req.result == UnityWebRequest.Result.Success;
        if (!ok)
            Debug.LogWarning($"[VastuApiClient] Health check failed: {req.error}  url={url}");
        callback(ok);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Private coroutine pipeline
    // ══════════════════════════════════════════════════════════════════════════

    private IEnumerator AnalysisCoroutine(string roomType)
    {
        _isBusy = true;
        UIManager.Instance?.ShowLoading(true);

        // ── Optional: health check before the heavy upload ────────────────
        if (checkHealthBeforeScan)
        {
            bool serverOk = false;
            yield return StartCoroutine(CheckServerHealth(ok => serverOk = ok));
            if (!serverOk)
            {
                string err = $"Cannot reach server at {serverUrl}. " +
                             "Check: server is running, same Wi-Fi network, correct IP in Inspector.";
                Fail(err);
                yield break;
            }
        }

        // ── Step 1: Capture image ─────────────────────────────────────────
        byte[] jpegBytes  = null;
        string captureErr = null;

        yield return StartCoroutine(CaptureImageBytes(
            bytes => jpegBytes  = bytes,
            err   => captureErr = err));

        if (captureErr != null || jpegBytes == null || jpegBytes.Length == 0)
        {
            Fail(captureErr ?? "Image capture returned empty bytes.");
            yield break;
        }

        Debug.Log($"[VastuApiClient] Captured {jpegBytes.Length} bytes | room_type={roomType}");

        // ── Step 2: Build multipart/form-data POST ────────────────────────
        // Field names MUST match the FastAPI endpoint signature exactly:
        //   image     → UploadFile  (binary)
        //   room_type → Form str    (text)
        string url = serverUrl.TrimEnd('/') + "/analyze";
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", jpegBytes, "room.jpg", "image/jpeg");
        form.AddField("room_type", roomType);

        using UnityWebRequest request = UnityWebRequest.Post(url, form);
        request.timeout = Mathf.CeilToInt(timeoutSeconds);
        // Explicitly accept JSON so the server doesn't vary its Content-Type.
        request.SetRequestHeader("Accept", "application/json");

        Debug.Log($"[VastuApiClient] POST {url} ({jpegBytes.Length / 1024} KB)");

        // ── Step 3: Send ──────────────────────────────────────────────────
        yield return request.SendWebRequest();

        UIManager.Instance?.ShowLoading(false);

        // ── Step 4: Network-level errors ──────────────────────────────────
        if (request.result != UnityWebRequest.Result.Success)
        {
            string netErr = $"HTTP {request.responseCode}: {request.error}  [{request.result}]";
            // Provide actionable guidance for common Android failure modes.
            if (request.result == UnityWebRequest.Result.ConnectionError)
                netErr += "\n→ Is the server running? Are both devices on the same Wi-Fi? Correct LAN IP in Inspector?";
            else if (request.responseCode == 422)
                netErr += "\n→ 422 Unprocessable Entity: multipart field names or types are wrong. Check field names image / room_type.";
            else if (request.responseCode >= 500)
                netErr += "\n→ Server-side error. Check the Python console for a traceback.";
            Fail(netErr);
            yield break;
        }

        // ── Step 5: Parse JSON ────────────────────────────────────────────
        string jsonText = request.downloadHandler.text;
        Debug.Log("[VastuApiClient] Response body: " + jsonText);

        if (string.IsNullOrWhiteSpace(jsonText))
        {
            Fail("Server returned an empty response body.");
            yield break;
        }

        VastuApiResponse response = null;
        try
        {
            response = JsonUtility.FromJson<VastuApiResponse>(jsonText);
        }
        catch (Exception ex)
        {
            Fail("JSON parse error: " + ex.Message + "\nRaw: " + jsonText.Substring(0, Mathf.Min(200, jsonText.Length)));
            yield break;
        }

        if (response == null)
        {
            Fail("JsonUtility.FromJson returned null. Response may not match the expected schema.");
            yield break;
        }

        // ── Step 6: Validate non-trivial fields ───────────────────────────
        // JsonUtility silently leaves strings null if missing — guard them.
        if (response.vastu_results == null)
            response.vastu_results = Array.Empty<VastuResultItem>();
        if (response.detections == null)
            response.detections    = Array.Empty<VastuDetectionItem>();
        if (string.IsNullOrEmpty(response.summary))
            response.summary       = "No summary returned.";
        if (string.IsNullOrEmpty(response.direction_note))
            response.direction_note = "image_zone values represent position within the photo, not compass directions.";

        // ── Step 7: Deliver results ───────────────────────────────────────
        Debug.Log($"[VastuApiClient] ✓ score={response.overall_score}  items={response.vastu_results.Length}  summary={response.summary}");
        VastuAssistantManager.Instance?.InjectApiResults(response);
        OnAnalysisComplete?.Invoke(response);
        UIManager.Instance?.ShowToast($"Scan done! Vastu score: {response.overall_score}/100");

        _isBusy = false;
    }

    // ── Helper: fail with logging + events ───────────────────────────────────
    private void Fail(string message)
    {
        Debug.LogError("[VastuApiClient] " + message);
        UIManager.Instance?.ShowLoading(false);
        UIManager.Instance?.ShowToast("Scan failed — see console for details.");
        OnAnalysisError?.Invoke(message);
        _isBusy = false;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Image capture
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Tries to grab a JPEG from the AR camera.
    /// Falls back to a full-screen ReadPixels if AR is unavailable.
    /// </summary>
    private IEnumerator CaptureImageBytes(Action<byte[]> onSuccess, Action<string> onError)
    {
        // ── Path A: AR CPU image (preferred — no extra GPU readback) ──────
        if (arCameraManager != null && arCameraManager.enabled)
        {
            byte[] result    = null;
            string errMsg    = null;
            bool   acquired  = false;

            if (arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage cpuImage))
            {
                using (cpuImage)
                {
                    int w = cpuImage.width;
                    int h = cpuImage.height;

                    var conversionParams = new XRCpuImage.ConversionParams
                    {
                        inputRect        = new RectInt(0, 0, w, h),
                        outputDimensions = new Vector2Int(w, h),
                        outputFormat     = TextureFormat.RGBA32,
                        transformation   = XRCpuImage.Transformation.MirrorY,
                    };

                    int dataSize = cpuImage.GetConvertedDataSize(conversionParams);
                    if (dataSize > 0)
                    {
                        var rawData = new Unity.Collections.NativeArray<byte>(
                            dataSize, Unity.Collections.Allocator.Temp);
                        try
                        {
                            cpuImage.Convert(conversionParams, rawData);

                            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                            tex.LoadRawTextureData(rawData);
                            tex.Apply();
                            result   = tex.EncodeToJPG(jpegQuality);
                            Destroy(tex);
                            acquired = true;
                        }
                        catch (Exception ex)
                        {
                            errMsg = "XRCpuImage conversion failed: " + ex.Message;
                        }
                        finally
                        {
                            rawData.Dispose();
                        }
                    }
                    else
                    {
                        errMsg = "XRCpuImage.GetConvertedDataSize returned 0.";
                    }
                }

                if (acquired && result != null)
                {
                    onSuccess(result);
                    yield break;
                }

                Debug.LogWarning("[VastuApiClient] AR capture failed: " + errMsg +
                                 "  → trying screen-capture fallback.");
            }
            else
            {
                Debug.LogWarning("[VastuApiClient] TryAcquireLatestCpuImage returned false. " +
                                 "AR may not have a live frame yet → trying screen-capture fallback.");
            }
        }

        // ── Path B: Screen ReadPixels fallback ────────────────────────────
        // Works in the Unity Editor and in non-AR builds for quick testing.
        if (Camera.main == null)
        {
            onError("No AR frame and Camera.main is null. " +
                    "Ensure ARCameraManager has a live frame, or a Camera tagged 'MainCamera' exists.");
            yield break;
        }

        // Must wait for end-of-frame before ReadPixels.
        yield return new WaitForEndOfFrame();

        try
        {
            int sw = Screen.width;
            int sh = Screen.height;
            Texture2D screenTex = new Texture2D(sw, sh, TextureFormat.RGB24, false);
            screenTex.ReadPixels(new Rect(0, 0, sw, sh), 0, 0);
            screenTex.Apply();
            byte[] bytes = screenTex.EncodeToJPG(jpegQuality);
            Destroy(screenTex);

            if (bytes == null || bytes.Length == 0)
            {
                onError("Screen ReadPixels produced empty JPEG data.");
                yield break;
            }

            Debug.Log($"[VastuApiClient] Screen-capture fallback: {sw}x{sh} → {bytes.Length} bytes");
            onSuccess(bytes);
        }
        catch (Exception ex)
        {
            onError("Screen-capture fallback failed: " + ex.Message);
        }
    }
}
