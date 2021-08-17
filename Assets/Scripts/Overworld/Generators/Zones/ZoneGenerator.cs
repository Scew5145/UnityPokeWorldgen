using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ZoneGenerator : ScriptableObject
{
  // Zones:
  /*
   * To properly create a generator subclass, the inheritor must do the following:
   *    * Implement the ZoneCreator interface
   *    * Implement GenerateScene, calling this base class first
   *    * Have the generated route pass the "continuity" and "traversable" criteria (more on this in the ZoneTester class)
   * Usually, for the sake of generation, ZoneGenerators should be an inner class of the zone they're associated with.
   * DO NOT KEEP LONG-TERM REFERENCES TO ZONEGENERATORS. They will keep the zone loaded forever, which is obviously bad. Only keep them around
   * when needed.
   */

  private Zone _zone;
  public Zone Zone => _zone;
  protected readonly int seed; // to be used with all random calls. Base constructor sets this up, make sure to call it in child classes

  public ZoneGenerator(int inSeed)
  {
    seed = inSeed;
    Random.InitState(seed);
  }

  protected void SetZone(Zone newZone)
  {
    _zone = newZone;
  }

  public virtual Zone GenerateZone(Vector2Int inOverworldCoordinates, string inLayer = "overworld")
  {
    SetZone(new Zone(inOverworldCoordinates, inLayer));
    GetRootObject();
    return Zone;
  }

  public GameObject GetRootObject()
  {
    if(Zone.GetSceneRoot() == null)
    {
      GameObject newRoot = new GameObject("ZONEROOT");
      Zone.SetSceneRoot(newRoot);
    }
    return Zone.GetSceneRoot();
  }
}
