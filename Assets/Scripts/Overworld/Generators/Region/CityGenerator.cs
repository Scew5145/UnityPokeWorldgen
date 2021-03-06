using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : RegionGenerator
{
  public int numCities = 9;
  public List<ZoneGeneratorData> validUntriedZones;
  public List<ZoneGeneratorData> cities;
  private int maxCityPlacementAttempts = 10;
  public float minDistanceBetweenCities = 4;
  public CityGenerator(RegionGeneratorData _regionData) : base(_regionData)
  {
    validUntriedZones = new List<ZoneGeneratorData>();
    cities = new List<ZoneGeneratorData>();
  }

  // TODO: if I reorder BiomeGenerator & CityGenerator, I can use the land clusters instead of GatherLandBasedZones here
  public void GatherLandBasedZones()
  {
    int regionWidth = regionData.regionDimensions.x;
    int regionHeight = regionData.regionDimensions.y;
    for (int y = 0; y < regionHeight; y++)
    {
      for (int x = 0; x < regionWidth; x++)
      {
        if (regionData.allZoneData[x + (y * regionWidth)].tags.Contains("land"))
        {
          validUntriedZones.Add(regionData.allZoneData[x + (y * regionWidth)]);
        }
      }
    }
  }
  public void Generate()
  {
    GatherLandBasedZones();
    if(numCities > validUntriedZones.Count)
    {
      Debug.LogError("Not enough space to place all towns! Zone count: " + validUntriedZones.Count + " City count: " + numCities);
      return;
    }
    List<ZoneGeneratorData> tempCities = new List<ZoneGeneratorData>();
    List<ZoneGeneratorData> triedZones = new List<ZoneGeneratorData>();
    for (int city = 0; city < numCities; city++)
    {
      int attempts = 0;
      // Attempt poisson disk sampling until we run out of attempts.
      while (attempts <= maxCityPlacementAttempts)
      {
        if(validUntriedZones.Count == 0)
        {
          Debug.LogWarning("Ran out of space to place cities with good spacing! D:");
          break;
        }
        int index = Random.Range(0, validUntriedZones.Count);
        // Debug.Log("Placing: " + validUntriedZones[index].OverworldCoordinates.x + " " + validUntriedZones[index].OverworldCoordinates.y);
        float distanceFromCity = float.MaxValue;
        for (int placedIndex = 0; placedIndex < tempCities.Count; placedIndex ++)
        {
          ZoneGeneratorData placedCity = tempCities[placedIndex];
          float distanceToPlaced = Vector2Int.Distance(validUntriedZones[index].OverworldCoordinates, placedCity.OverworldCoordinates);
          // Debug.Log("Checking: " + placedCity.OverworldCoordinates.x + " " + placedCity.OverworldCoordinates.y);
          // Debug.Log(distanceToPlaced);
          if (distanceToPlaced < distanceFromCity)
          {
            distanceFromCity = distanceToPlaced;
          }
        }
        // Debug.Log(distanceFromCity);
        if (distanceFromCity <= minDistanceBetweenCities)
        {
          // Debug.LogWarning("Failed a placement");
        }
        else
        {
          // Found a valid cell, add it to the list of cities
          tempCities.Add(validUntriedZones[index]);
          validUntriedZones.RemoveAt(index);
          break;
        }
        triedZones.Add(validUntriedZones[index]);
        validUntriedZones.RemoveAt(index);
        attempts += 1;
      }
      // If we fell all the way out of the attempt cycle without finding one, we'll add it afterwards.
      // The next city won't try the ones we've already attempted (because they're removed from validUntriedZones)
    }
    validUntriedZones.AddRange(triedZones); // Re-add the cities for future operations

    if(tempCities.Count < numCities)
    {
      // If we don't have as many cities as we hoped to generate, we place the remainder naively (no disk samples, full random)
      // We KNOW there's enough zones to assign because of the previous check (numCities > validUntriedZones.Count), so as long as we aren't
      // placing two cities on top of one another we can ensure this random placement will work.
      Debug.Log("Placing extra cities. Placed: " + tempCities.Count + " Remaining: " + (numCities - tempCities.Count));
      for (int city = 0; city < numCities - tempCities.Count; city++)
      {
        int index = Random.Range(0, validUntriedZones.Count);
        tempCities.Add(validUntriedZones[index]);
        validUntriedZones.RemoveAt(index);
      }
    }

    // ... and finally, finish restocking the validUntriedZones in case we need them later
    validUntriedZones.AddRange(tempCities);

    cities = tempCities;
    foreach (ZoneGeneratorData city in cities)
    {
      // WORTH NOTING: as of 5/20/2022 BiomeGenerator searches for this tag to place urban/rural biome info. Some cities may end up as biome centers
      city.tags.Add("city");
    }

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


  // CreateHeightMap (moved to biome generator) and CreateMaskedVoronoiMap are currently unused, because I'm using possion disk sampling for city placement atm.
  // These were for a alternate workflow, where we divide the region into subregions, and then move the cities to the center of each subregion.
  // See Lloyd Relaxation (though it's not a perfect 1-to-1 for this technique) for more info
  public Dictionary<Vector2Int, int> CreateMaskedVoronoiMap(float[,] heightMap, List<ZoneGeneratorData> citiesToUse)
  {
    Dictionary<Vector2Int, int> voronoiMap = new Dictionary<Vector2Int, int>();
    // TODO: Compute distance to each city, strip water verts out, add new point <x,y> with value (city index)
    return voronoiMap;
  }
}
