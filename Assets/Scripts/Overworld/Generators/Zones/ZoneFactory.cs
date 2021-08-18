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
  
  protected string _layer = "overworld";
  public string Layer => _layer;

  private bool _running;
  public bool Running => _running;

  protected Queue<KeyValuePair<Vector2Int, string>> generateQueue = new Queue<KeyValuePair<Vector2Int, string>>();

  // Start is called before the first frame update
  void Start()
  {
    generators.Init();
    QueueGenerateZone("test", new Vector2Int(0, 0));
  }

  // Update is called once per frame
  void Update()
  {
    if(!_running)
    {
      return;
    }
    if(generateQueue.Count == 0)
    {
      _running = false;
      return;
    }
    Zone newZone = GenerateZone(generateQueue.Dequeue());
    // TODO TESTING remove: this zone needs to be passed to the ZoneStreamer instead for management, and then saved and unloaded
    SceneManager.MoveGameObjectToScene(newZone.Root, SceneManager.GetActiveScene());
  }

  void QueueGenerateZone(string generatorType, Vector2Int inOverworldCoordinates)
  {
    generateQueue.Enqueue(new KeyValuePair<Vector2Int, string>(inOverworldCoordinates, generatorType));
    _running = true;
  }

  protected Zone GenerateZone(KeyValuePair<Vector2Int, string> generationInfo)
  {
    print(generationInfo);
    print(generators);
    if(!generators.ContainsKey(generationInfo.Value))
    {
      return null;
    }
    return generators[generationInfo.Value].GenerateZone(generationInfo.Key, Layer);
  }
}