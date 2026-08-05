
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Lightweight debugging utility specifically designed for AR Foundation 5.2.0.
/// Subscribes strictly to ARPlaneManager.planesChanged to log newly detected planes.
/// </summary>
[DisallowMultipleComponent]
public class PlaneDebugger : MonoBehaviour
{
    // ============================================================================
    // INSPECTOR REFERENCES
    // ============================================================================

    [Header("AR References")]
    [Tooltip("Reference to the ARPlaneManager. If unassigned, will attempt to acquire one automatically.")]
    [SerializeField] private ARPlaneManager planeManager;

    // ============================================================================
    // UNITY LIFECYCLE METHODS
    // ============================================================================

    private void Awake()
    {
        // Auto-acquire ARPlaneManager if not assigned in Inspector
        if (planeManager == null)
        {
            planeManager = GetComponent<ARPlaneManager>();
            if (planeManager == null)
            {
                planeManager = FindObjectOfType<ARPlaneManager>();
            }
        }

        if (planeManager == null)
        {
            Debug.LogWarning("[PlaneDebugger] ARPlaneManager reference not found on object or in scene!");
        }
    }

    private void OnEnable()
    {
        // Subscribe ONLY to planesChanged event
        if (planeManager != null)
        {
            planeManager.planesChanged += OnPlanesChanged;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from planesChanged event when disabled
        if (planeManager != null)
        {
            planeManager.planesChanged -= OnPlanesChanged;
        }
    }

    // ============================================================================
    // EVENT HANDLERS & LOGGING
    // ============================================================================

    /// <summary>
    /// Event handler for ARPlaneManager.planesChanged (ARPlanesChangedEventArgs).
    /// Logs details for each newly added ARPlane.
    /// </summary>
    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (args.added == null) return;

        // Iterate through all newly added planes
        foreach (ARPlane plane in args.added)
        {
            if (plane == null) continue;

            // Extract required properties
            TrackableId trackableId = plane.trackableId;
            PlaneAlignment alignment = plane.alignment;
            Vector2 size = plane.size;

            // Log plane info
            Debug.Log($"[PlaneDebugger] New AR Plane Detected:\n" +
                      $"  • Trackable ID : {trackableId}\n" +
                      $"  • Alignment    : {alignment}\n" +
                      $"  • Size (X, Z)  : {size}");
        }
    }
}
