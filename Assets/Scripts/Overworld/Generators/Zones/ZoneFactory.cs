using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ZoneFactory : MonoBehaviour
{
  // Base class for manipulating ZoneGenerators, and registering Zones with the ZoneStreamer. 
  /* "Zones" are basically indvidual unity objects with some important properties:
  * Zones are loaded and unloaded by the ZoneStreamer at runtime
  * Zones are an explicit x by y by z size, determined by what type of zone they are.
  * Zones have links to their adjacent zones, with correct accessibility modifiers, preferably.
  */

  public ZoneGeneratorReferenceTable generators;

  public ZoneStreamer zoneStreamer;

  protected string _layer = "overworld";
  public string Layer => _layer; // Layer isn't being properly used atm. Important for when we add enterable structures. Not sure the best way to handle it atm

  private bool _running;
  public bool Running => _running;

  protected Queue<KeyValuePair<Vector2Int, string>> generateQueue = new Queue<KeyValuePair<Vector2Int, string>>();

  // Start is called before the first frame update
  void Start()
  {
    generators.Init();
  }

  // Update is called once per frame
  void Update()
  {
    if (!_running)
    {
      return;
    }
    if (generateQueue.Count == 0)
    {
      _running = false;
      return;
    }
    KeyValuePair<Vector2Int, string> generationInfo = generateQueue.Dequeue();
    // TODO TESTING remove: this zone needs to be passed to the ZoneStreamer instead for management, and then saved and unloaded
    ((StaticZoneGenerator)generators["statictest"]).rotation = new Vector3(0, 90 * (4 - generateQueue.Count), 0);
    // TODO TESTING end
    Zone newZone = GenerateZone(generationInfo);

    //
    //
  }

  // TODO: templated variant of this where it takes a struct of generator data
  public void QueueGenerateZone(string generatorType, Vector2Int inOverworldCoordinates)
  {
    generateQueue.Enqueue(new KeyValuePair<Vector2Int, string>(inOverworldCoordinates, generatorType));
    _running = true;
  }

  public bool SaveZone(Zone zoneToSave)
  {
    if (!generators.ContainsKey(zoneToSave.ZoneType))
    {
      Debug.LogError("Failed to find zone of type " + zoneToSave.ZoneType + " for zone " + zoneToSave.GetSceneName());
      return false;
    }
    generators[zoneToSave.ZoneType].SaveZone(GetZoneFilepath(zoneToSave), zoneToSave);
    return true;
  }

  public Zone LoadZone(KeyValuePair<Vector2Int, string> generationInfo)
  {
    Zone outZone = generators[generationInfo.Value].LoadZone(GetZoneFilepath(generationInfo.Value, generationInfo.Key));
    return outZone;
  }

  public void BuildZone(Zone zoneToBuild)
  {
    generators[zoneToBuild.ZoneType].BuildZone(zoneToBuild);
  }

  public Zone GenerateZone(KeyValuePair<Vector2Int, string> generationInfo) 
  {
    // Only call this directly if you depend on the zone being loaded, otherwise use QueueGenerateZone
    if (!generators.ContainsKey(generationInfo.Value))
    {
      return null;
    }
    Zone newZone = generators[generationInfo.Value].GenerateZone(generationInfo.Key, Layer);
    SaveZone(newZone);
    zoneStreamer.RegisterZone(newZone);
    return newZone;
  }

  public string GetZoneDataFolder()
  {
    return "/ZoneData/" + Layer + "/";
  }

  public bool IsZoneGenerated(string inLayer, Vector2Int inCoordinates) // Don't call this often, if possible - context switching is slow
  {
    return System.IO.File.Exists(GetZoneFilepath(inLayer, inCoordinates));
  }

  public string GetZoneFilepath(string inLayer, Vector2Int inCoordinates)
  {
    string path = GetZoneDataFolder() + Zone.GetSceneNameFromLocation(inLayer, inCoordinates) + ".zone";
    return path;
  }

  public string GetZoneFilepath(Zone inZone)
  {
    return GetZoneDataFolder() + inZone.GetSceneName() + ".zone";
  }
}