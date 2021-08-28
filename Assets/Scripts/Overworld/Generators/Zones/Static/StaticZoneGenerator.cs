using System.Runtime.CompilerServices;
using UnityEngine;
using System;
using System.Collections.Generic;

[assembly: InternalsVisibleTo("StaticZone")]
[CreateAssetMenu(fileName = "NewZoneType", menuName = "ScriptableObjects/ZoneGenerator/Static Zone", order = 1)]
public class StaticZoneGenerator : ZoneGenerator
{
  public string prefabPath;
  // Rotation is a member of the generator rather than the zone itself for two reasons:
  // 1. it's needed on instatiation if we were saving it as zone data
  // 2. Static zones are *static*, which means they need to be generated the same every time. If we need multiple static zones, 
  //     each one would be need to be generated with a separate factory anyways, since each one needs a separate prefab
  //     in theory, we could do some overengineered system for holding multiple prefabs in a single static generator... but why bother?
  public Vector3 rotation;

  // too lazy to calculate this from rotation lmao
  public static Dictionary<Vector3, Vector3> rotationToOffsetDict = new Dictionary<Vector3, Vector3>{
    { new Vector3(0.0f,90.0f,0.0f), new Vector3(0.0f,0.0f,1.0f)},
    { new Vector3(0.0f,180.0f,0.0f), new Vector3(1.0f,0.0f,1.0f)},
    { new Vector3(0.0f,270.0f,0.0f), new Vector3(1.0f,0.0f,0.0f)},
  };
  public override Zone GenerateZone(Vector2Int inOverworldCoordinates, string inLayer = "overworld")
  {
    StaticZone newStaticZone = new StaticZone();
    if(prefabPath.Length == 0)
    {
      Debug.LogError("Got an empty prefab path for StaticZoneGenerator! This will crash if buildzone is called!");
    }
    newStaticZone.InitZone(inOverworldCoordinates, inLayer);
    return newStaticZone;
  }

  public override void BuildZone(in Zone zone)
  {
    // TODO: this startup logic can be moved into a helper function or a wrapper for BuildZone's main logic
    // No point in writing the same code over and over here
    if (zone.GetType() != GetZoneType())
    {
      Debug.LogError("Sent an incorrectly classed zone to StaticZone::BuildZone");
      return;
    }
    StaticZone staticZone = (StaticZone)zone;
    base.BuildZone(zone);
    GameObject prefabInstRoot = Instantiate(Resources.Load<GameObject>(prefabPath));
    prefabInstRoot.transform.SetParent(zone.Root.transform, false);
    prefabInstRoot.transform.rotation = Quaternion.Euler(rotation);
    if(rotationToOffsetDict.ContainsKey(rotation))
    {
      Vector3 zoneSize = new Vector3(zone.GetSize().x - 1, zone.GetSize().y - 1, zone.GetSize().z - 1);
      prefabInstRoot.transform.position += new Vector3(
        rotationToOffsetDict[rotation].x * zoneSize.x,
        rotationToOffsetDict[rotation].y * zoneSize.y,
        rotationToOffsetDict[rotation].z * zoneSize.z);
    }
    // 
  }
  public override Type GetZoneType()
  {
    return typeof(StaticZone);
  }
}
