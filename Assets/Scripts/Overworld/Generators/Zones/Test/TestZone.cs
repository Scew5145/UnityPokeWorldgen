using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using System.Xml;


[DataContract]
public class TestZone : Zone
{
  [DataMember]
  List<Tile> tiles;

  public TestZone(Vector2Int inOverworldCoordinates, string inLayer) : base(inOverworldCoordinates, inLayer)
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

  public class TestZoneGenerator : ZoneGenerator
  {
    // TestZoneGenerator makes flat, all-0-height plane zones. Because it's for testing.
    public TestZoneGenerator(int inSeed) : base(inSeed)
    {

    }
    public GameObject prefabTile;
    public override Zone GenerateZone(Vector2Int inOverworldCoordinates, string inLayer = "overworld")
    {
      TestZone newTestZone = new TestZone(inOverworldCoordinates, inLayer);
      GameObject rootObject = GetRootObject();
      rootObject.transform.position = Vector3.zero;
      foreach (Tile tile in newTestZone.tiles)
      {
        GameObject tileObject = Instantiate(prefabTile, 
          new Vector3(tile.position.x, tile.position.y, tile.position.z),
          prefabTile.transform.rotation, rootObject.transform);
      }
      return Zone;
    }
  }
}