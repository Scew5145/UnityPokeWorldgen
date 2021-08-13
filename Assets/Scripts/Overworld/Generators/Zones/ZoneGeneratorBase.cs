using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ZoneCreator
{
  IReadOnlyList<int> SampleHeightMap(int xcoord, int ycoord);
  IReadOnlyList<Vector3Int> GetPathTiles();
}

public class ZoneGeneratorBase : ScriptableObject, ZoneCreator
{
  // Zones:
  /*
   * Zones are the PKWorldGen equivalent of "chunks" in minecraft.
   * They represent a sub-region of a route or other macro-generator construct.
   * To properly create a generator subclass, the inheritor must do the following:
   *    * Implement the ZoneCreator interface
   *    * Have the generated route pass the "continuity" and "traversable" criteria (more on this in the ZoneTester class)
   */
  IReadOnlyList<int> ZoneCreator.SampleHeightMap(int xcoord, int ycoord)
  {
    return new List<int> { 0 };
  }

  IReadOnlyList<Vector3Int> ZoneCreator.GetPathTiles()
  {
    return new List<Vector3Int>();
  }
}
