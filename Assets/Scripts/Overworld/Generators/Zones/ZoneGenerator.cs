using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using System.Text;
using UnityEngine;

[assembly: InternalsVisibleTo("Zone")]
[CreateAssetMenu(fileName = "NewZoneGenerator", menuName = "ScriptableObjects/ZoneGenerator/Zone", order = 1)]
public class ZoneGenerator : ScriptableObject
{
  /** <summary>
   * A <c>ZoneGenerator</c> is basically a constructor interface for a Zone.
   * To properly create a generator subclass, the inheritor must do the following:
   *    * Implement <c>GenerateZone()</c>,
   *    * Have the generated route pass the "continuity" and "traversable" criteria (more on this in the ZoneTester class, which... isn't written yet)
   * Usually, for the sake of generation, ZoneGenerators should use the "InternalsVisibleTo" directive for the sake of allowing CreateAssetMenu to work correctly.
   *    * if CreateAssetMenu didn't require the filename to be the same as the class name, this would be a subclass instead.
   * DO NOT SAVE LARGE AMOUNTS OF DATA IN ZONEGENERATORS.
   * Generally, a ZoneGenerator is always loaded if one of its zones are within the ZoneStreamer's active zones.
   * That means all of its tile prefabs will be loaded, which mean all of their textures will be loaded, etc. - saving large quantities of data is a bad idea.
   * </summary>
   */

  public int seed; // ZoneGenerator #TODO for debug purposes - should be passed to the zone on generation and saved to gen files
  public string rootName;

  [SerializeField]
  public GameEvent zoneEnterEvent;

  public ZoneGenerator() : base()
  {
  }
  // TODO: tileTypes as a list of game objects is a bad idea. They're always prefabs, so it makes more sense to load them only when needed.
  // Otherwise, if the generator is fully loaded with this, so are ALL OF ITS PREFABS. That's bad.
  // Instead, this tileTypes set should be a set of asset bundle paths that we load and unload as needed, handled with a function that does that separately.
  // We do need to have them loaded when we're constructing the zone, but after that we don't need them anymore.
  [SerializeField]
  public List<GameObject> tileTypes = new List<GameObject>();

  // #TODO: think about if I need this generator class separated from the base class of the zone itself.
  // isn't this just a constructor? What's the best way to handle this?
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

  public virtual Type GetZoneType()
  {
    return typeof(Zone);
  }

  /** <summary>
   * <c> GenerateZone </c> is a call once, save externally function. 
   * It's effectively static, but isn't explicitly, so that we can save ZoneGenerator variants in the unity editor, each with separate sets of generation params.
   * </summary> 
   */
  public virtual Zone GenerateZone(Vector2Int inOverworldCoordinates, string inLayer = "overworld")
  {    
    Zone newZone = new Zone();
    newZone.InitZone(inOverworldCoordinates, inLayer);
    newZone.ZoneType = "zone";
    return newZone;
  }

  /** <summary>
   * <c>BuildZone</c> is called when a level needs to be loaded fully into the scene. it is responsible for building the rootNode and attaching all children.
   * This is done separately from Generate Zone / Load Zone to allow multiple types of construction of a zone without duplicating scene creation logic.
   * </summary>
   */
  public virtual void BuildZone(in Zone zone)
  {
    GameObject newRoot = CreateSceneRoot();
    BoxCollider zoneEdgeDetect = newRoot.AddComponent<BoxCollider>();
    ZoneEnterExitTrigger zoneTrigger = newRoot.AddComponent<ZoneEnterExitTrigger>();
    zoneTrigger.enterEvent = zoneEnterEvent;
    newRoot.layer = 3;
    Vector3 zoneSize = zone.GetSize();
    zoneEdgeDetect.center = new Vector3((zoneSize.x / 2.0f) - 0.5f, zoneSize.y / 2.0f, (zoneSize.z / 2.0f) - 0.5f);
    zoneEdgeDetect.size = zoneSize;
    zoneEdgeDetect.isTrigger = true;
    zone.SetSceneRoot(newRoot);
    // Anything generated should be in the zoneSize.x * zoneSize.y grid, done as if the zone was being placed at 0,0 as its overworld tile
    zone.Root.transform.position = new Vector3(
      zone.OverworldCoordinates.x * zoneSize.x, 
      0, 
      zone.OverworldCoordinates.y * zoneSize.z);
    return;
  }

  public virtual Zone LoadZone(string fileName)
  {
    Type type = GetZoneType(); // Using GetType even in a base class function will still return the type of the derived class, because c# is cool
    string path = Application.persistentDataPath + fileName; // TODO: path as config scriptableobject
    FileStream fs = new FileStream(path, FileMode.Open);
    XmlTextReader reader = new XmlTextReader(fs);
    reader.Read();
    DataContractSerializer ser =
        new DataContractSerializer(type);

    Zone newZone = (Zone)ser.ReadObject(reader);
    fs.Close();
    return newZone;
  }

  public virtual bool SaveZone(string fileName, Zone zone)
  {
    // Create a new instance of a StreamWriter
    // to read and write the data.
    string path = Application.persistentDataPath + fileName; // TODO: path as config scriptableobject

    // Creating the directory like this will do nothing if it already exists
    string folder = Path.GetDirectoryName(path);
    Directory.CreateDirectory(folder);

    FileStream fs = new FileStream(path, FileMode.Create);
    XmlTextWriter writer = new XmlTextWriter(fs, Encoding.Unicode);
    writer.Formatting = Formatting.Indented;
    DataContractSerializer ser =
        new DataContractSerializer(zone.GetType());
    ser.WriteObject(writer, zone);
    writer.Close();
    fs.Close();
    return true;
  }

  public virtual GameObject CreateSceneRoot()
  {
    GameObject newRoot = new GameObject(GetRootName());
    newRoot.name = GetRootName();
    return newRoot;
  }

  public virtual string GetRootName()
  {
    return rootName;
  }
}
