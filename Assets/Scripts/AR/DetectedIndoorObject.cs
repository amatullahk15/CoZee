using System;
using UnityEngine;

/// <summary>
/// Data model for detected indoor objects using RenoVision YOLOv8 on Unity Sentis.
/// Compatible with Unity 2022.3 LTS.
/// </summary>
[Serializable]
public class DetectedIndoorObject
{
    [Tooltip("Detected object class label (e.g. sofa, chair, table, wardrobe, door, window)")]
    public string ClassName;

    [Tooltip("Detection confidence percentage score (0.0 to 1.0)")]
    public float Confidence;

    [Tooltip("Normalized screen space bounding box Rect [0.0 to 1.0]")]
    public Rect BoundingBox;

    /// <summary>
    /// Constructs a new DetectedIndoorObject instance.
    /// </summary>
    public DetectedIndoorObject(string className, float confidence, Rect boundingBox)
    {
        ClassName = className;
        Confidence = confidence;
        BoundingBox = boundingBox;
    }

    /// <summary>
    /// Formatted display label (e.g. "Sofa 96%")
    /// </summary>
    public string DisplayLabel => $"{ClassName} {Mathf.RoundToInt(Confidence * 100)}%";
}
