using System;
using System.Collections.Generic;

[Serializable]
public class ScannedSurfaceRecord
{
    public string label;
    public string surfaceType;
    public float primaryDimensionMeters;
    public float secondaryDimensionMeters;
    public float areaSquareMeters;
}

[Serializable]
public class RoomScanData
{
    public string roomId;
    public string summary;
    public int wallCount;
    public int floorCount;
    public float totalWallAreaSquareMeters;
    public float totalFloorAreaSquareMeters;
    public List<ScannedSurfaceRecord> surfaces = new List<ScannedSurfaceRecord>();
}
