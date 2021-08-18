using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using System.Xml;


[DataContract]
public class TestZone : Zone
{
  [DataMember]
  internal List<Tile> tiles = new List<Tile>();

  internal override void InitZone(Vector2Int inOverworldCoordinates, string inLayer)
  {
    base.InitZone(inOverworldCoordinates, inLayer);
  }
}