using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestZoneGenerator : ZoneGenerator
{
  List<Tile> tiles;
  public GameObject prefabTile;
  // TestZoneGenerator makes flat, all-0-height planes with no textures.
  public Tile? GetTile(int x, int z)
  {
    if(x + z * GetSize().z >= tiles.Count)
    {
      return null;
    }
    return tiles[x + z * GetSize().z];
  }
  public TestZoneGenerator(int inSeed, Vector2Int inOverworldCoordinates, string inLayer) : base(inSeed, inOverworldCoordinates, inLayer)
  {
    for(int i = 0; i < GetSize().x; i++)
    {
      for (int j = 0; j < GetSize().z; j++)
      {
        Tile newTile = new Tile(new Vector3Int(i,0,j), true);
        tiles.Add(newTile);
      }
    }
  }
  public override IReadOnlyList<int> SampleHeightMap(int xcoord, int ycoord)
  {
    return new List<int> { 0 };
  }

  public override IReadOnlyList<Tile> GetPathTiles()
  {
    return new List<Tile>();
  }

  public override GameObject GenerateScene()
  {
    GameObject rootObject = new GameObject("GENROOT");
    rootObject.transform.position = Vector3.zero;
    foreach(Tile tile in tiles)
    {
      GameObject tileObject = Instantiate(prefabTile, new Vector3(tile.position.x, tile.position.y, tile.position.z), prefabTile.transform.rotation, rootObject.transform);
    }
    return rootObject;
  }

}
