using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using System.Runtime;
using System.Xml;
using System;

public struct Tile
{
  // Tiles are 'static' in that they won't change based on player interaction.
  // Because of this, they don't need to implement any saving/loading interfaces outside their data.
  [DataMember]
  public Vector3 position;
  [DataMember]
  public Vector3 rotation;
  [DataMember]
  public Vector3 scale;
  [DataMember]
  public bool isTraversable;
  [DataMember]
  public int tileType;
  public Tile(Vector3 inPosition = new Vector3(), Vector3 inRotation = new Vector3(), Vector3 inScale = new Vector3(), 
    int inTileType = 0,bool inIsTraversable = false)
  {
    position = inPosition;
    rotation = inRotation;
    scale = inScale;
    isTraversable = inIsTraversable;
    tileType = inTileType;
  }
}

public struct ZoneLink
{
  [DataMember]
  public string zoneName; // Get a reference to the zone by using ZoneStreamer.GetZoneReference()

  [DataMember]
  public List<Tile> linkingTiles;

  public ZoneLink(Zone inZone, List<Tile> inLinkingTiles)
  {
    zoneName = inZone.GetSceneName();
    linkingTiles = inLinkingTiles;
  }
}

[DataContract]
public class Zone : ScriptableObject
{
  /**<summary>
   * <c>Zones</c> are the PKWorldGen equivalent of "chunks" in minecraft.
   * They represent a sub-region of a route or other macro-generator construct.
   * Zones themselves represent the data half of worldgen, with the <c>ZoneGenerator</c> representing the generation half.
   * Anything important for saving/loading zones should use DataContract and similar directives to denote anything that needs to be saved on load.
   * </summary>
   */

  [DataMember]
  protected Vector2Int _overworldCoordinates;
  public Vector2Int OverworldCoordinates => _overworldCoordinates;

  [DataMember]
  public string layer; // Used by the zone manager for level streaming. base overworld is just "overworld"

  private GameObject _root;
  public GameObject Root => _root;

  [DataMember]
  List<ZoneLink> adjacentZones;

  internal virtual void InitZone(Vector2Int inOverworldCoordinates, string inLayer)
  {
    _overworldCoordinates = inOverworldCoordinates;
    layer = inLayer;
  }

  public string GetSceneName()
  {
    return layer + "_" + _overworldCoordinates.x + "_" + _overworldCoordinates.y;
  }

  public virtual Vector3Int GetSize()
  {
    return new Vector3Int(24, 10, 24);
  }

  public virtual IReadOnlyList<int> SampleHeightMap(int xcoord, int ycoord)
  {
    return new List<int> { 0 };
  }

  public virtual IReadOnlyList<Tile> GetPathTiles()
  {
    return new List<Tile>();
  }

  public void SetSceneRoot(GameObject newRoot)
  {
    _root = newRoot;
  }

  public List<string> GetAdjacentZoneNames()
  {
    return new List<string>(from adjZone in adjacentZones select adjZone.zoneName);
  }
}
