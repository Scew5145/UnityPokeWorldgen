using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;


[DataContract(Name = "TestZone", Namespace = "http://schemas.datacontract.org/2004/07/UnityEngine")]
public class TestZone : Zone
{
  [DataMember]
  internal List<Tile> tiles = new List<Tile>();

}