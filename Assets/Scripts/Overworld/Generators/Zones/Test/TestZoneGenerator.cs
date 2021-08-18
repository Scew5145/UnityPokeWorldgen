using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewZoneType", menuName = "ScriptableObjects/ZoneGenerator/Test Zone", order = 1)]
public class TestZoneGenerator : ZoneGenerator
{
  // TestZoneGenerator makes flat, all-0-height plane zones. Because it's for testing.
  public TestZoneGenerator() : base()
  {

  }
  public override Zone GenerateZone(Vector2Int inOverworldCoordinates, string inLayer = "overworld")
  {
    TestZone newTestZone = CreateInstance<TestZone>();
    newTestZone.InitZone(inOverworldCoordinates, inLayer);
    newTestZone.SetSceneRoot(CreateSceneRoot());
    newTestZone.Root.transform.position = Vector3.zero;
    
    Debug.Log(newTestZone);
    for (int i = 0; i < newTestZone.GetSize().x; i++)
    {
      for (int j = 0; j < newTestZone.GetSize().z; j++)
      {
        Tile newTile = new Tile(new Vector3(i, 0, j));
        newTestZone.tiles.Add(newTile);
      }
    }
    GameObject tilePrefab = GetTilePrefab(0);
    for (int i = 0; i < newTestZone.tiles.Count; i++)
    {
      Tile tile = newTestZone.tiles[i];
      tile.position.y = Random.Range(0.0f, 1.0f);
      GameObject tileObject = Instantiate(GetTilePrefab(0),
        new Vector3(tile.position.x, tile.position.y, tile.position.z),
        GetTilePrefab(0).transform.rotation, newTestZone.Root.transform);
    }
    return newTestZone;
  }

  public override string GetRootName()
  {
    return "Test Zone";
  }
}
