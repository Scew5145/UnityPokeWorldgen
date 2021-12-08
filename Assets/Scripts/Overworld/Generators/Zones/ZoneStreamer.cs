using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

struct ZoneStreamingInfo
{
  List<string> adjacentZonesToLoad;
  Vector2Int zoneLocation;
  public ZoneStreamingInfo(Zone inputZone)
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

  private RuntimeTable<string, Zone> loadedZones;

  private RuntimeTable<string, ZoneStreamingInfo> registeredZones;

  public ZoneFactory zoneFactory;

  private string _currentZone;

  public string CurrentZone => _currentZone;

  public void Start()
  {
    //zoneFactory.QueueGenerateZone("statictest", new Vector2Int(0, 0));
    //zoneFactory.QueueGenerateZone("test", new Vector2Int(0, 1));
    //zoneFactory.QueueGenerateZone("statictest", new Vector2Int(1, 0));
    //zoneFactory.QueueGenerateZone("statictest", new Vector2Int(1, 1));
  }

  public bool ZoneExists(string zoneName)
  {
    if (!registeredZones)
      return false;
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
    if(loadedZones)
    {
      loadedZones.Add(zoneName, newZone);
    }
    if(registeredZones)
    {
      registeredZones.Add(zoneName, new ZoneStreamingInfo(newZone));
    }
    return true;
  }

  public WeakReference<Zone> GetZoneReference(string zoneName)
  {
    if(loadedZones && loadedZones.ContainsKey(zoneName))
    {
      return new WeakReference<Zone>(loadedZones[zoneName]);
    }
    return null;
  }

  // #TODO: zone loading / unloading
  /*
   * zone streamer has a string that represents the 'current zone' we're in. 
   * adj. zones needs to be filled out properly
   * concept: zones that're only visible but not traversable. should be linked like normal?
   * then, loading states need to be set up
   * States:
   *      Built (exists in game)
   *      Disabled (exists in game, all children disabled)
   *      Loaded (Zone struct is loaded, but not built)
   *      Unloaded (Exists solely as a zone link)
   */

  // Simulate a teleport by loading a zone from scratch, optionally emptying the loaded set of zones TODO
  public void LoadZone(string layer, Vector2Int zoneCoordinates, bool clearLoaded = false)
  {
    if(clearLoaded)
    {
      loadedZones.Clear(); // TODO: Zone needs a destructor for cleaning up trainers and stuff. Maybe.
    }
    //Zone loadedZone = null;
    if(!zoneFactory.IsZoneGenerated(layer, zoneCoordinates))
    {
      // TODO: zones that haven't been generated yet should be generated here. RuntimeTable ref to a coordinate --> generator pair
      // said table should be updated and edited by whatever the region generator is.
      //loadedZone = zoneFactory.GenerateZone()
    }
    else
    {
      //loadedZone = zoneFactory.LoadZone(layer, zoneCoordinates);
    }
  }
}

