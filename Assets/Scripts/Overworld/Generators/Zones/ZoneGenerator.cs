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
   * DO NOT SAVE LARGE AMOUNTS OF DATA IN ZONEGENERATORS.
   * Generally, a ZoneGenerator is always loaded if one of its zones are within the ZoneStreamer's active zones.
   * That means all of its tile prefabs will be loaded, which mean all of their textures will be loaded, etc.
   */

  protected int seed; // ZoneGenerator #TODO for debug purposes - should be passed to the zone on generation and saved to gen files

  public ZoneGenerator() : base()
  {
  }
  // TODO: tileTypes as a list of game objects is a bad idea. They're always prefabs, so it makes more sense to load them only when needed.
  // Otherwise, if the generator is full loaded with this, so are ALL OF ITS PREFABS. That's bad.
  // Instead, this tileTypes set should be a set of asset bundle paths that we load and unload as needed, handled with a function that does that separately.
  // We do need to have them loaded when we're constructing the zone, but after that we don't need them anymore.
  [SerializeField]
  public List<GameObject> tileTypes = new List<GameObject>();

  // #TODO: think about if I need this separated from the base class of the zone itself. isn't this just a constructor? What's the best way to handle this?
  // as of right now, it makes sense - tileTypes isn't static, which means I can have multiple instances of generators with different set params or tilesets.
  // That part is nice. it might be reason enough to keep doing it this way.
  // On the other hand, I could probably do the exact same thing just by making subclasses of the base generator type... We'll stick with this for now.
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
    // #TODO: this needs to be separated into two steps.
    // * the first step is generation. Generation just creates any nessecary data for scene recreation (e.g. tiles w/ tiletypes, trainers, etc)
    // * the second step is ACTUAL loading: create each of the GameObjects, set the scene root, etc.
    
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
