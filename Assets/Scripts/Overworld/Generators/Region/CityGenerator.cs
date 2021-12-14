using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator
{
  TerrainGenerator tGen;

  public readonly int seed;

  public readonly ZoneGeneratorData[,] zoneGeneratorData; // Note that only the reference value is readonly, but the array itself is good to be modified
  public CityGenerator(int _seed, ZoneGeneratorData[,] _zoneGeneratorData)
  {
    seed = _seed;
    zoneGeneratorData = _zoneGeneratorData;
    Random.InitState(seed);
  }

  public void Generate()
  {

  }

}
