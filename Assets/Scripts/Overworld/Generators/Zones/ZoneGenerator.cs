using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface ZoneCreator
{
  IReadOnlyList<int> SampleHeightMap(int xcoord, int ycoord);
  IReadOnlyList<Tile> GetPathTiles();
  GameObject GenerateScene();
}

public struct Tile
{
  Vector3Int transform { get; set; }
  bool isTraversable { get; set; }
  Tile(Vector3Int inTransform, bool inIsTraversable)
  {
    transform = inTransform;
    isTraversable = inIsTraversable;
  }
}

public class ZoneGenerator : ScriptableObject, ZoneCreator
{
  // Zones:
  /*
   * Zones are the PKWorldGen equivalent of "chunks" in minecraft.
   * They represent a sub-region of a route or other macro-generator construct.
   * To properly create a generator subclass, the inheritor must do the following:
   *    * Implement the ZoneCreator interface
   *    * Implement GenerateScene, calling this base class first
   *    * Have the generated route pass the "continuity" and "traversable" criteria (more on this in the ZoneTester class)
   */
  protected readonly Vector2Int overworldCoordinates;
  protected readonly int seed; // to be used with all random calls. Base constructor sets this up, make sure to call it in child classes
  protected string layer; // Used by the zone manager for level streaming. base overworld is just "overworld"
  public static Vector3Int size = new Vector3Int(24, 24, 10);

  public ZoneGenerator(int inSeed, Vector2Int inOverworldCoordinates, string inLayer = "overworld")
  {
    seed = inSeed;
    overworldCoordinates = inOverworldCoordinates;
    layer = inLayer;
    Random.InitState(seed);
  }

  IReadOnlyList<int> ZoneCreator.SampleHeightMap(int xcoord, int ycoord)
  {
    return new List<int> { 0 };
  }

  IReadOnlyList<Tile> ZoneCreator.GetPathTiles()
  {
    return new List<Tile>();
  }

  GameObject ZoneCreator.GenerateScene()
  {
    return new GameObject("DEFAULT_GEN");
  }

  public string GetSceneName()
  {
    return layer + "_" + overworldCoordinates.x + "_" + overworldCoordinates.y;
  }
}
