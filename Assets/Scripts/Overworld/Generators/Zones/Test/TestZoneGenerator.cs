using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("TestZone")]
[CreateAssetMenu(fileName = "NewZoneType", menuName = "ScriptableObjects/ZoneGenerator/Test Zone", order = 1)]
public class TestZoneGenerator : ZoneGenerator
{
  // TestZoneGenerator makes flat, all-0-height plane zones. Because it's for testing.
  public TestZoneGenerator() : base()
  {
  }
  public override Zone GenerateZone(Vector2Int inOverworldCoordinates, string inLayer = "overworld")
  {
    TestZone newTestZone = new TestZone();
    newTestZone.InitZone(inOverworldCoordinates, inLayer);    
    for (int i = 0; i < newTestZone.GetSize().x; i++)
    {
      for (int j = 0; j < newTestZone.GetSize().z; j++)
      {
        Tile newTile = new Tile(new Vector3(i, 0, j));
        newTile.position.y = UnityEngine.Random.Range(0.0f, 1.0f);
        newTestZone.tiles.Add(newTile);
      }
    }
    
    return newTestZone;
  }

  public override void BuildZone(in Zone zone)
  {
    if(zone.GetType() != typeof(TestZone))
    {
      Debug.LogError("Sent an incorrectly classed zone to TestZone::BuildZone");
      return;
    }
    TestZone testZone = (TestZone)zone;
    base.BuildZone(zone); // do our root setup
    GameObject tilePrefab = GetTilePrefab(0);
    for (int i = 0; i < testZone.tiles.Count; i++)
    {
      Tile tile = testZone.tiles[i];
      GameObject tileObject = Instantiate(tilePrefab,
        new Vector3(tile.position.x, tile.position.y, tile.position.z),
        tilePrefab.transform.rotation);
      tileObject.transform.SetParent(testZone.Root.transform, false);
    }
  }

  public override Type GetZoneType()
  {
    return typeof(TestZone);
  }
}
