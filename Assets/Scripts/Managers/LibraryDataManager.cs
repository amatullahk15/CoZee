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
    public string detailsText;
    public RoomScanData roomScanData;
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

        return AddPreparedItem(item);
    }

    public LibraryItem AddRoomScan(string title, RoomScanData roomScanData, string thumbnailPath = null)
    {
        if (roomScanData == null)
            return AddItem(string.IsNullOrWhiteSpace(title) ? "Saved Room" : title, "rooms", thumbnailPath);

        var item = new LibraryItem
        {
            id = Guid.NewGuid().ToString(),
            title = string.IsNullOrWhiteSpace(title)
                ? BuildRoomTitle(roomScanData)
                : title,
            category = "rooms",
            thumbnailPath = thumbnailPath,
            isFavorite = false,
            createdAt = DateTime.UtcNow.ToString("o"),
            detailsText = roomScanData.summary,
            roomScanData = CloneRoomScanData(roomScanData)
        };

        return AddPreparedItem(item);
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
                createdAt = DateTime.UtcNow.AddHours(-4).ToString("o"),
                detailsText = "2 walls • 1 floor • 18.40 m2",
                roomScanData = new RoomScanData
                {
                    roomId = Guid.NewGuid().ToString(),
                    summary = "2 walls • 1 floor • 18.40 m2",
                    wallCount = 2,
                    floorCount = 1,
                    totalWallAreaSquareMeters = 12.2f,
                    totalFloorAreaSquareMeters = 6.2f,
                    surfaces = new List<ScannedSurfaceRecord>
                    {
                        new ScannedSurfaceRecord
                        {
                            label = "Wall 1",
                            surfaceType = "Wall",
                            primaryDimensionMeters = 3.40f,
                            secondaryDimensionMeters = 2.60f,
                            areaSquareMeters = 8.84f
                        },
                        new ScannedSurfaceRecord
                        {
                            label = "Wall 2",
                            surfaceType = "Wall",
                            primaryDimensionMeters = 1.30f,
                            secondaryDimensionMeters = 2.60f,
                            areaSquareMeters = 3.38f
                        },
                        new ScannedSurfaceRecord
                        {
                            label = "Floor 1",
                            surfaceType = "Floor",
                            primaryDimensionMeters = 3.10f,
                            secondaryDimensionMeters = 2.00f,
                            areaSquareMeters = 6.20f
                        }
                    }
                }
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
                createdAt = DateTime.UtcNow.AddDays(-1).ToString("o"),
                detailsText = "1 wall • 1 floor • 14.28 m2",
                roomScanData = new RoomScanData
                {
                    roomId = Guid.NewGuid().ToString(),
                    summary = "1 wall • 1 floor • 14.28 m2",
                    wallCount = 1,
                    floorCount = 1,
                    totalWallAreaSquareMeters = 7.08f,
                    totalFloorAreaSquareMeters = 7.20f,
                    surfaces = new List<ScannedSurfaceRecord>
                    {
                        new ScannedSurfaceRecord
                        {
                            label = "Wall 1",
                            surfaceType = "Wall",
                            primaryDimensionMeters = 2.95f,
                            secondaryDimensionMeters = 2.40f,
                            areaSquareMeters = 7.08f
                        },
                        new ScannedSurfaceRecord
                        {
                            label = "Floor 1",
                            surfaceType = "Floor",
                            primaryDimensionMeters = 3.00f,
                            secondaryDimensionMeters = 2.40f,
                            areaSquareMeters = 7.20f
                        }
                    }
                }
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

    LibraryItem AddPreparedItem(LibraryItem item)
    {
        collection.items.Insert(0, item);
        Save();
        OnLibraryChanged?.Invoke();
        return item;
    }

    static RoomScanData CloneRoomScanData(RoomScanData source)
    {
        if (source == null)
            return null;

        var clone = new RoomScanData
        {
            roomId = source.roomId,
            summary = source.summary,
            wallCount = source.wallCount,
            floorCount = source.floorCount,
            totalWallAreaSquareMeters = source.totalWallAreaSquareMeters,
            totalFloorAreaSquareMeters = source.totalFloorAreaSquareMeters,
            surfaces = new List<ScannedSurfaceRecord>()
        };

        if (source.surfaces != null)
        {
            foreach (ScannedSurfaceRecord record in source.surfaces)
            {
                clone.surfaces.Add(new ScannedSurfaceRecord
                {
                    label = record.label,
                    surfaceType = record.surfaceType,
                    primaryDimensionMeters = record.primaryDimensionMeters,
                    secondaryDimensionMeters = record.secondaryDimensionMeters,
                    areaSquareMeters = record.areaSquareMeters
                });
            }
        }

        return clone;
    }

    static string BuildRoomTitle(RoomScanData roomScanData)
    {
        if (roomScanData == null)
            return "Saved Room";

        if (roomScanData.wallCount > 0)
            return $"Saved Room ({roomScanData.wallCount} Wall{(roomScanData.wallCount == 1 ? "" : "s")})";

        if (roomScanData.floorCount > 0)
            return $"Saved Room ({roomScanData.floorCount} Floor{(roomScanData.floorCount == 1 ? "" : "s")})";

        return "Saved Room";
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
