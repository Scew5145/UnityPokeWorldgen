using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

struct ZoneStreamingInfo
{
  List<string> adjacentZonesToLoad;
  Vector2Int zoneLocation;
  ZoneStreamingInfo(Zone inputZone)
  {
    adjacentZonesToLoad = inputZone.GetAdjacentZoneNames();
    zoneLocation = inputZone.OverworldCoordinates;
  }
}


// ZoneStreamer is a singleton class used for managing the world level streaming at runtime.
// Primarily, it's job is to hold information on which zones are streamed in and which ones aren't, As well as handle events
// When a zone is entered or exited. 
// The ZoneFactory class is responsible for registering any created scenes with this streamer, and for interfacing with ZoneGenerators.
// The ZoneGenerator class is responsible for populating each scene registered here.
public sealed class ZoneStreamer : MonoBehaviour
{
  /* ZoneStreamer #TODO: Connect this with the save manager. 
   * Two main things have to be done for the ZoneStreamer to be considered fully functional:
   *    * During play, levels must be able to load and unload without losing mutable data.
   *    * On first generation, we need to have a static set of data that can be used to recreate a Zone.
   * That means I need to write saving and loading functions for zones.
   * it also means I have to write functionality for saving out the registeredZones info, since that's the primary zone list
   */

  private Dictionary<string, Zone> loadedZones;

  private Dictionary<string, ZoneStreamingInfo> registeredZones;

  public bool ZoneExists(string zoneName)
  {
    return registeredZones.ContainsKey(zoneName);
  }

  public bool RegisterZone(Zone newZone)
  {
    string zoneName = newZone.GetSceneName();
    if (ZoneExists(zoneName))
    {
      Debug.LogError("Zone " + newZone.GetSceneName() + " was doubly registered!");
      return false;
    }
    return true; //newZone->Save();
  }

  public WeakReference<Zone> GetZoneReference(string zoneName)
  {
    if(loadedZones.ContainsKey(zoneName))
    {
      return new WeakReference<Zone>(loadedZones[zoneName]);
    }
    return null;
  }
}

