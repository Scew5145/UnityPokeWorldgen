using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator
{
  TerrainGenerator tGen;

  public readonly int seed;
  int numCities = 16;

  public readonly ZoneGeneratorData[,] zoneGeneratorData; // Note that only the reference value is readonly, but the array itself is good to be modified

  public List<ZoneGeneratorData> landBasedZones;
  public List<ZoneGeneratorData> cities;
  public CityGenerator(int _seed, ZoneGeneratorData[,] _zoneGeneratorData)
  {
    seed = _seed;
    zoneGeneratorData = _zoneGeneratorData;
    Random.InitState(seed);
    landBasedZones = new List<ZoneGeneratorData>();
    cities = new List<ZoneGeneratorData>();
  }

  public void GatherLandBasedZones()
  {
    int regionWidth = zoneGeneratorData.GetLength(0);
    int regionHeight = zoneGeneratorData.GetLength(1);
    for (int y = 0; y < regionHeight; y++)
    {
      for (int x = 0; x < regionWidth; x++)
      {
        if (zoneGeneratorData[x, y].tags.Contains("land"))
        {
          landBasedZones.Add(zoneGeneratorData[x, y]);
        }
      }
    }
    Debug.Log(landBasedZones);
  }

  public void Generate()
  {
    GatherLandBasedZones();
    for(int city = 0; city < numCities; city++)
    {
      int index = Random.Range(0, landBasedZones.Count);
      cities.Add(landBasedZones[index]);
      landBasedZones.RemoveAt(index);
    }
    landBasedZones.AddRange(cities); // Re-add the cities for future operations
  }

}
