using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DesignConcept
{
    public string id;
    public string prompt;
    public string style;
    public string createdAt;
}

public class DesignAIManager : MonoBehaviour
{
    public static DesignAIManager Instance { get; private set; }

    public event Action<DesignConcept> OnConceptGenerated;

    readonly List<DesignConcept> concepts = new List<DesignConcept>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SeedInitialConcepts();
    }

    void SeedInitialConcepts()
    {
        if (concepts.Count > 0)
            return;

        concepts.Add(new DesignConcept
        {
            id = Guid.NewGuid().ToString(),
            prompt = "Scandinavian living room with warm wooden furniture, neutral linen sofa, and ambient lighting",
            style = "Scandinavian",
            createdAt = DateTime.UtcNow.AddHours(-2).ToString("o")
        });

        concepts.Add(new DesignConcept
        {
            id = Guid.NewGuid().ToString(),
            prompt = "Modern minimalist studio space with sleek wardrobe storage and monochrome color palette",
            style = "Modern",
            createdAt = DateTime.UtcNow.AddHours(-6).ToString("o")
        });

        concepts.Add(new DesignConcept
        {
            id = Guid.NewGuid().ToString(),
            prompt = "Bohemian interior layout with indoor greenery, textured fabrics, and oak furniture accents",
            style = "Boho",
            createdAt = DateTime.UtcNow.AddHours(-18).ToString("o")
        });
    }

    public IReadOnlyList<DesignConcept> GetConcepts() => concepts;

    public void GenerateConcept(string prompt, string style, Action<DesignConcept> onComplete = null)
    {
        StartCoroutine(GenerateRoutine(prompt, style, onComplete));
    }

    IEnumerator GenerateRoutine(string prompt, string style, Action<DesignConcept> onComplete)
    {
        yield return new WaitForSeconds(1.2f);

        var concept = new DesignConcept
        {
            id = Guid.NewGuid().ToString(),
            prompt = prompt,
            style = style,
            createdAt = DateTime.UtcNow.ToString("o")
        };

        concepts.Insert(0, concept);
        OnConceptGenerated?.Invoke(concept);
        onComplete?.Invoke(concept);

        if (LibraryDataManager.Instance != null)
        {
            LibraryDataManager.Instance.AddItem(
                $"Design: {style}",
                "designs",
                null);
        }
    }
}
