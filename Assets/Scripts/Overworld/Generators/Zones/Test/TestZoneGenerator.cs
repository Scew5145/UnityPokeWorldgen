using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestZoneGenerator : ZoneGenerator
{
  List<Vector3Int> tiles; 
  public TestZoneGenerator(int inSeed, Vector2Int inOverworldCoordinates, string inLayer) : base(inSeed, inOverworldCoordinates, inLayer)
  {
    for(int i = 0; i < size.x; i++)
    {
      for (int j = 0; j < size.y; j++)
      {

      }
    }
  }

}
