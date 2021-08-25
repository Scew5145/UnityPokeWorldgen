using System.Runtime.CompilerServices;
using UnityEngine;
using System;

[assembly: InternalsVisibleTo("StaticZone")]
[CreateAssetMenu(fileName = "NewZoneType", menuName = "ScriptableObjects/ZoneGenerator/Static Zone", order = 1)]
public class StaticZoneGenerator : ZoneGenerator
{
  public string prefabPath;
  public override Zone GenerateZone(Vector2Int inOverworldCoordinates, string inLayer = "overworld")
  {
    StaticZone newStaticZone = new StaticZone();
    if(prefabPath.Length == 0)
    {
      Debug.LogError("Got an empty prefab path for StaticZoneGenerator! This will crash if buildzone is called!");
    }
    newStaticZone.InitZone(inOverworldCoordinates, inLayer, prefabPath);
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
    GameObject prefabInstRoot = Instantiate(Resources.Load<GameObject>(staticZone.PrefabPath));
    prefabInstRoot.transform.SetParent(zone.Root.transform, false);
  }
  public override Type GetZoneType()
  {
    return typeof(StaticZone);
  }
}
