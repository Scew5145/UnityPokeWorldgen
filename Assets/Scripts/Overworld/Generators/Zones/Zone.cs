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
  public Vector3Int position;
  public bool isTraversable;
  public Tile(Vector3Int inPosition, bool inIsTraversable)
  {
    position = inPosition;
    isTraversable = inIsTraversable;
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
  /*
   * Zones are the PKWorldGen equivalent of "chunks" in minecraft.
   * They represent a sub-region of a route or other macro-generator construct.
   */
  [DataMember]
  protected readonly Vector2Int _overworldCoordinates;
  public Vector2Int OverworldCoordinates => _overworldCoordinates;

  [DataMember]
  protected string layer; // Used by the zone manager for level streaming. base overworld is just "overworld"

  [DataMember]
  private GameObject root;

  List<ZoneLink> adjacentZones;

  public Zone(Vector2Int inOverworldCoordinates, string inLayer)
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

  public virtual GameObject GetSceneRoot()
  {
    return root;
  }

  public void SetSceneRoot(GameObject newRoot)
  {
    root = newRoot;
  }

  public List<string> GetAdjacentZoneNames()
  {
    return new List<string>(from adjZone in adjacentZones select adjZone.zoneName);
  }
}
