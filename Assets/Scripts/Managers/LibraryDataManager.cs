using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class LibraryItem
{
    public string id;
    public string title;
    public string category;
    public string thumbnailPath;
    public bool isFavorite;
    public string createdAt;
}

[Serializable]
public class LibraryCollection
{
    public List<LibraryItem> items = new List<LibraryItem>();
}

public class LibraryDataManager : MonoBehaviour
{
    public static LibraryDataManager Instance { get; private set; }

    const string FileName = "library_data.json";

    LibraryCollection collection = new LibraryCollection();
    string filePath;

    public event Action OnLibraryChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        filePath = Path.Combine(Application.persistentDataPath, FileName);
        Load();
    }

    public IReadOnlyList<LibraryItem> GetAll() => collection.items;

    public List<LibraryItem> GetByCategory(string category)
    {
        return collection.items.FindAll(i => i.category == category);
    }

    public List<LibraryItem> GetFavorites()
    {
        return collection.items.FindAll(i => i.isFavorite);
    }

    public LibraryItem AddItem(string title, string category, string thumbnailPath = null)
    {
        var item = new LibraryItem
        {
            id = Guid.NewGuid().ToString(),
            title = title,
            category = category,
            thumbnailPath = thumbnailPath,
            isFavorite = false,
            createdAt = DateTime.UtcNow.ToString("o")
        };

        collection.items.Insert(0, item);
        Save();
        OnLibraryChanged?.Invoke();
        return item;
    }

    public void ToggleFavorite(string id)
    {
        LibraryItem item = collection.items.Find(i => i.id == id);
        if (item == null)
            return;

        item.isFavorite = !item.isFavorite;
        Save();
        OnLibraryChanged?.Invoke();
    }

    public void RemoveItem(string id)
    {
        collection.items.RemoveAll(i => i.id == id);
        Save();
        OnLibraryChanged?.Invoke();
    }

    void Load()
    {
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                collection = JsonUtility.FromJson<LibraryCollection>(json) ?? new LibraryCollection();
            }
            catch (Exception e)
            {
                Debug.LogWarning("Library load failed: " + e.Message);
                collection = new LibraryCollection();
            }
        }

        if (collection.items == null || collection.items.Count == 0)
        {
            SeedDefaultItems();
        }
    }

    void SeedDefaultItems()
    {
        collection.items = new List<LibraryItem>
        {
            new LibraryItem
            {
                id = Guid.NewGuid().ToString(),
                title = "Living Room AR Measurement",
                category = "rooms",
                isFavorite = true,
                createdAt = DateTime.UtcNow.AddHours(-4).ToString("o")
            },
            new LibraryItem
            {
                id = Guid.NewGuid().ToString(),
                title = "Scandinavian Living Room Design",
                category = "concepts",
                isFavorite = true,
                createdAt = DateTime.UtcNow.AddHours(-12).ToString("o")
            },
            new LibraryItem
            {
                id = Guid.NewGuid().ToString(),
                title = "Master Bedroom Vastu Alignment",
                category = "rooms",
                isFavorite = false,
                createdAt = DateTime.UtcNow.AddDays(-1).ToString("o")
            },
            new LibraryItem
            {
                id = Guid.NewGuid().ToString(),
                title = "Modern Wardrobe & Sofa Setup",
                category = "concepts",
                isFavorite = false,
                createdAt = DateTime.UtcNow.AddDays(-2).ToString("o")
            }
        };
        Save();
    }

    void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(collection, true);
            File.WriteAllText(filePath, json);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Library save failed: " + e.Message);
        }
    }
}
