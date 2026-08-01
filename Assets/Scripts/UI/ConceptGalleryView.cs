using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConceptGalleryView : MonoBehaviour
{
    [SerializeField] Transform contentRoot;
    [SerializeField] GeneratedConceptCard cardPrefab;

    readonly List<GeneratedConceptCard> cards = new List<GeneratedConceptCard>();

    void Awake()
    {
        if (contentRoot == null)
            contentRoot = transform;
    }

    void Start()
    {
        Refresh();
    }

    void OnEnable()
    {
        if (DesignAIManager.Instance != null)
            DesignAIManager.Instance.OnConceptGenerated += AddConcept;

        Refresh();
    }

    void OnDisable()
    {
        if (DesignAIManager.Instance != null)
            DesignAIManager.Instance.OnConceptGenerated -= AddConcept;
    }

    void Refresh()
    {
        Clear();

        if (DesignAIManager.Instance == null)
            return;

        foreach (DesignConcept concept in DesignAIManager.Instance.GetConcepts())
            AddConcept(concept);
    }

    void AddConcept(DesignConcept concept)
    {
        if (contentRoot == null)
            return;

        GeneratedConceptCard card = cardPrefab != null
            ? Instantiate(cardPrefab, contentRoot)
            : RuntimeUIFactory.CreateConceptCard(contentRoot);

        card.Bind(concept);
        cards.Add(card);
    }

    void Clear()
    {
        foreach (GeneratedConceptCard card in cards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        cards.Clear();
    }
}
