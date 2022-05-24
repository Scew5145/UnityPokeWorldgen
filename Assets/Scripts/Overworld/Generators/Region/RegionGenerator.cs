using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// base class for region-wide generator constructs. Generally in charge of generating a single feature of the biome (terrain, biome, city locations, etc)
public class RegionGenerator
{
  public Texture2D generatedTexture;
  public readonly RegionGeneratorData regionData;

  public RegionGenerator(RegionGeneratorData _regionData)
  {
    regionData = _regionData;
    UnityEngine.Random.InitState(regionData.seed);
    generatedTexture = new Texture2D(regionData.regionDimensions.x * regionData.zoneDimensions.x, regionData.regionDimensions.y * regionData.zoneDimensions.y);
  }

  public Vector2Int TilePositionToZone(Vector2Int TileCoordinates)
  {
    Vector2Int OutputCoordinates = new Vector2Int();
    OutputCoordinates.x = TileCoordinates.x / regionData.zoneDimensions.x;
    OutputCoordinates.y = TileCoordinates.y / regionData.zoneDimensions.y;

    return OutputCoordinates;
  }

  public int FlattenZoneCoordinates(Vector2Int ZoneCoordinates)
  {
    return ZoneCoordinates.x + (ZoneCoordinates.y * regionData.regionDimensions.x);
  }

  public ZoneGeneratorData GetZoneData(Vector2Int ZoneCoordinates)
  {
    return regionData.allZoneData[FlattenZoneCoordinates(ZoneCoordinates)];
  }

  // Helper functions for ray-based masks
  public Vector2 Normalized2D(Vector2 vector)
  {
    float mag = vector.magnitude;
    if (mag > .00001)
    {
      return (vector / mag);
    }
    return Vector2.zero;
  }

  public Vector2 ClosestPointOnLine(Vector2 vA, Vector2 vB, Vector2 vPoint)
  {
    Vector2 vVector1 = vPoint - vA;
    Vector2 vVector2 = Normalized2D(vB - vA);

    float d = Vector2.Distance(vA, vB);
    float t = Vector2.Dot(vVector2, vVector1);

    if (t <= 0)
      return vA;

    if (t >= d)
      return vB;

    Vector2 vVector3 = vVector2 * t;

    Vector2 vClosestPoint = vA + vVector3;

    return vClosestPoint;
  }

  public List<ZoneGeneratorData> GatherZonesOfTag(String tag)
  {
    List<ZoneGeneratorData> outputList = new List<ZoneGeneratorData>();
    foreach (ZoneGeneratorData zone in regionData.allZoneData)
    {
      if (zone.tags.Contains(tag))
      {
        outputList.Add(zone);
      }
    }
    return outputList;
  }
}