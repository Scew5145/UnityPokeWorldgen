using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("Zone")]
[CreateAssetMenu(fileName = "NewZoneGenerator", menuName = "ScriptableObjects/ZoneGenerator/Zone", order = 1)]
public class ZoneGenerator : ScriptableObject
{
  /*
   * A ZoneGenerator is basically a constructor interface for a Zone.
   * To properly create a generator subclass, the inheritor must do the following:
   *    * Implement GenerateZone(),
   *    * Have the generated route pass the "continuity" and "traversable" criteria (more on this in the ZoneTester class)
   * Usually, for the sake of generation, ZoneGenerators should use the "InternalsVisibleTo" directive for the sake of allowing CreateAssetMenu to work correctly.
   *    * if CreateAssetMenu didn't require the filename to be the same as the class name, this would be a subclass instead.
   * DO NOT KEEP LONG-TERM REFERENCES TO ZONEGENERATORS. They will keep the zone references loaded forever, which is obviously bad. Only keep them around
   * when needed.
   */

  protected int seed; // ZoneGenerator #TODO for debug purposes - should be passed to the zone on generation and saved to gen files

  public ZoneGenerator() : base()
  {
  }
  [SerializeField]
  public List<GameObject> tileTypes = new List<GameObject>();
  public GameObject GetTilePrefab(int tileType)
  {
    if (tileType >= tileTypes.Count)
    {
      return null;
    }
    return tileTypes[tileType];
  }
  public virtual Zone GenerateZone(Vector2Int inOverworldCoordinates, string inLayer = "overworld")
  {
    GameObject newRoot = CreateSceneRoot();
    Zone newZone = CreateInstance<Zone>();
    newZone.SetSceneRoot(newRoot);
    return newZone;
  }

  public virtual GameObject CreateSceneRoot()
  {
    return new GameObject(GetRootName());
  }

  public virtual string GetRootName()
  {
    return "Default Zone";
  }
}
