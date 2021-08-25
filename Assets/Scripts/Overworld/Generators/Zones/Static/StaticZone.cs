using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;

[DataContract(Name = "TestZone", Namespace = "http://schemas.datacontract.org/2004/07/UnityEngine")]
public class StaticZone : Zone
{
  [DataMember]
  string _prefabPath;

  public string PrefabPath => _prefabPath;

  internal void InitZone(Vector2Int inOverworldCoordinates, string inLayer, string inPrefabRoute)
  {
    base.InitZone(inOverworldCoordinates, inLayer);
    _prefabPath = inPrefabRoute;
  }
}
