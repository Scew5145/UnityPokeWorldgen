using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ZoneFactory : MonoBehaviour
{
  // Base class for manipulating ZoneGenerators, and registering Zones with the ZoneStreamer. 
  /* "Zones" are basically indvidual unity scenes with some important properties:
  * Zones are loaded and unloaded by the ZoneStreamer at runtime
  * Zones are an explicit size, determined by what type of zone they are.
  * Zones have links to their adjacent zones, with accessibility modifiers, preferably
  */
  /* ZoneFactory #TODO: 
   * This needs to get written. If I need to start with just a base that Makes a zone, generates it, and then asks for the Streamer to load it,
   * that's good enough for now.
   * Once that's done, I'll need to write variants of this class: RouteZoneFactory, DungeonZoneFactory, etc.
   * Then this class will be the one that decides which ZoneGenerator to call for which zone. It'll be sick.
   */

  public ZoneGeneratorReferenceTable generators;

  public ZoneStreamer zoneStreamer;

  protected string _layer = "overworld";
  public string Layer => _layer; // Layer isn't being properly used atm. Important for when we add enterable structures

  private bool _running;
  public bool Running => _running;

  protected Queue<KeyValuePair<Vector2Int, string>> generateQueue = new Queue<KeyValuePair<Vector2Int, string>>();

  // Start is called before the first frame update
  void Start()
  {
    generators.Init();
    QueueGenerateZone("statictest", new Vector2Int(0, 0));
    QueueGenerateZone("statictest", new Vector2Int(0, 1));
    QueueGenerateZone("statictest", new Vector2Int(1, 0));
    QueueGenerateZone("statictest", new Vector2Int(1, 1));
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
    // TODO TESTING remove: this zone needs to be passed to the ZoneStreamer instead for management, and then saved and unloaded
    KeyValuePair<Vector2Int, string> generationInfo = generateQueue.Dequeue();
    ((StaticZoneGenerator)generators["statictest"]).rotation = new Vector3(0, 90 * (4 - generateQueue.Count), 0);
    Zone newZone = GenerateZone(generationInfo);
    SaveZone(newZone);
    zoneStreamer.RegisterZone(newZone);

    //
    //
  }

  void QueueGenerateZone(string generatorType, Vector2Int inOverworldCoordinates)
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
    generators[zoneToSave.ZoneType].SaveZone(GetZoneDataFolder() + zoneToSave.GetSceneName() + ".zone", zoneToSave);
    return true;
  }

  protected Zone LoadZone(KeyValuePair<Vector2Int, string> generationInfo)
  {
    Zone outZone = generators[generationInfo.Value].LoadZone(GetZoneDataFolder() + Zone.GetSceneNameFromLocation(generationInfo.Value, generationInfo.Key) + ".zone");
    return outZone;
  }

  public void BuildZone(Zone zoneToBuild)
  {
    generators[zoneToBuild.ZoneType].BuildZone(zoneToBuild);
  }

  protected Zone GenerateZone(KeyValuePair<Vector2Int, string> generationInfo)
  {
    if(!generators.ContainsKey(generationInfo.Value))
    {
      return null;
    }
    return generators[generationInfo.Value].GenerateZone(generationInfo.Key, Layer);
  }

  public string GetZoneDataFolder()
  {
    return "/ZoneData/" + Layer + "/";
  }
}