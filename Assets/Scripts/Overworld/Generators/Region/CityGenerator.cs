using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator
{
  int numCities = 8;
  public readonly RegionGeneratorData regionData; // Note that only the reference value is readonly, but the array itself is good to be modified

  public Texture2D generatedTexture;
  public List<ZoneGeneratorData> landBasedZones;
  public List<ZoneGeneratorData> cities;
  public CityGenerator(RegionGeneratorData _regionData)
  {
    regionData = _regionData;
    Random.InitState(regionData.seed);
    landBasedZones = new List<ZoneGeneratorData>();
    cities = new List<ZoneGeneratorData>();
    generatedTexture = new Texture2D(regionData.regionDimensions.x * regionData.zoneDimensions.x, regionData.regionDimensions.y * regionData.zoneDimensions.y);
  }

  public void GatherLandBasedZones()
  {
    int regionWidth = regionData.allZoneData.GetLength(0);
    int regionHeight = regionData.allZoneData.GetLength(1);
    for (int y = 0; y < regionHeight; y++)
    {
      for (int x = 0; x < regionWidth; x++)
      {
        if (regionData.allZoneData[x, y].tags.Contains("land"))
        {
          landBasedZones.Add(regionData.allZoneData[x, y]);
        }
      }
    }
  }


  // a transformation mostly for the simplification of code, so that I can read this in a month without going crazy
  // I don't know if this is actually slower, or if the extra you get from not having to load each zone as individually is faster, but frankly, I don't care ;.;
  // TODO: if worldgen things get slow, try removing this
  public float[,] CreateHeightMap()
  {
    float[,] newHeightMap = new float[regionData.regionDimensions.x * regionData.zoneDimensions.x, regionData.regionDimensions.y * regionData.zoneDimensions.y];
    for (int overworldY = 0; overworldY < regionData.regionDimensions.y; overworldY++)
    {
      for (int overworldX = 0; overworldX < regionData.regionDimensions.x; overworldX++)
      {
        for (int zoneY = 0; zoneY < regionData.zoneDimensions.y; zoneY++)
        {
          for (int zoneX = 0; zoneX < regionData.zoneDimensions.x; zoneX++)
          {
            newHeightMap[overworldX + zoneX, overworldY + zoneY] = regionData.allZoneData[overworldX, overworldY].heightMap[zoneX, zoneY];
          }
        }
      }
    }
    return newHeightMap;
  }
  public void Generate()
  {
    GatherLandBasedZones();
    List<ZoneGeneratorData> tempCities = new List<ZoneGeneratorData>();
    for (int city = 0; city < numCities; city++)
    {
      int index = Random.Range(0, landBasedZones.Count);
      tempCities.Add(landBasedZones[index]);
      landBasedZones.RemoveAt(index);
    }
    landBasedZones.AddRange(tempCities); // Re-add the cities for future operations

    float[,] heightmap = CreateHeightMap();
    Dictionary<Vector2Int, int> maskedVoronoiMap = CreateMaskedVoronoiMap(heightmap, tempCities);


    cities = tempCities;
    Color[] pix = new Color[generatedTexture.width * generatedTexture.height]; // This should be a greyscale texture to go fast, but... L A Z Y
    int y = 0;
    // Init values to black
    while (y < generatedTexture.height)
    {
      int x = 0;
      while (x < generatedTexture.width)
      {
        pix[y * generatedTexture.width + x] = Color.black;
        x++;
      }
      y++;
    }
    foreach(ZoneGeneratorData city in cities)
    {
      for(int zonePixelY = 0; zonePixelY < regionData.zoneDimensions.y; zonePixelY++)
      {
        for (int zonePixelX = 0; zonePixelX < regionData.zoneDimensions.x; zonePixelX++)
        {
          int pixelCoordinateY = regionData.zoneDimensions.y * city.OverworldCoordinates.y + zonePixelY;
          int pixelCoordinateX = regionData.zoneDimensions.x * city.OverworldCoordinates.x + zonePixelX;
          pix[pixelCoordinateY * generatedTexture.width + pixelCoordinateX] = Color.white;
        }
      }
    }
    generatedTexture.SetPixels(pix);
    generatedTexture.Apply();
  }

  public Dictionary<Vector2Int, int> CreateMaskedVoronoiMap(float[,] heightMap, List<ZoneGeneratorData> citiesToUse)
  {
    Dictionary<Vector2Int, int> voronoiMap = new Dictionary<Vector2Int, int>();
    // TODO: Compute distance to each city, strip water verts out, add new point <x,y> with value (city index)
    return voronoiMap;
  }
}
