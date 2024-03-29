using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using System.Text;

public class GeneratorManager : MonoBehaviour
{
  public TerrainGenerator tGen;
  public CityGenerator cGen;
  public BiomeGenerator bGen;

  public RegionGeneratorData regionData = new RegionGeneratorData();

  public GameEvent GenerationFinishedEvent;

  public enum GenerationStep
  {
    Idle,
    Started,
    Terrain,
    Biomes,
    Cities,
    Routes,
    Finished
  }

  GenerationStep currentStep = GenerationStep.Started;

  void Start()
  {
    // Important test seeds: 630058
    regionData.seed = (int)(Random.Range(0.0f, 1.0f) * 1000000);
    regionData.regionDimensions = new Vector2Int(40, 40);
    regionData.zoneDimensions = new Vector2Int(24, 24);
    Debug.Log("Seed: " + regionData.seed);
  }

  void Update()
  {
    float frameStartTime = Time.realtimeSinceStartup;
    if(currentStep != GenerationStep.Idle)
    {
      Debug.Log("Step:" + currentStep);
    }
    switch (currentStep)
    {
      case GenerationStep.Idle:
        break;
      case GenerationStep.Started:
        regionData.allZoneData = new ZoneGeneratorData[regionData.regionDimensions.x * regionData.regionDimensions.y];
        currentStep = GenerationStep.Terrain;
        break;
      case GenerationStep.Terrain:
        tGen = new TerrainGenerator(regionData); 
        tGen.Generate();
        currentStep = GenerationStep.Cities;
        break;
      case GenerationStep.Cities:
        cGen = new CityGenerator(regionData);
        cGen.Generate();
        currentStep = GenerationStep.Biomes;
        break;
      case GenerationStep.Biomes:
        bGen = new BiomeGenerator(regionData);
        bGen.Generate();
        currentStep = GenerationStep.Finished;
        break;
      case GenerationStep.Finished:
        GenerationFinishedEvent.Raise(this);
        currentStep = GenerationStep.Idle;
        RegionGeneratorData.SaveRegion("testRegionSave.region", regionData);
        Debug.Log(Application.persistentDataPath);
        break;
    }
    if (currentStep != GenerationStep.Idle)
    {
      Debug.Log("took " + (Time.realtimeSinceStartup - frameStartTime) + " to execute");
    }
  }
}

// Important terms that I'll try to be as consistent as possible with:
// if a class has "Data" in the name, that means it's instanced, for that specific runthrough of the game.
// if a class has "Info" in the name, that means it's static, and usually loaded from a json or is a game object, or both.

[DataContract(Name = "RegionGeneratorData", Namespace = "http://schemas.datacontract.org/2004/07/UnityEngine")]
public struct RegionGeneratorData
{
  /**<summary>
   * <c>RegionGeneratorData</c> is a struct created and controlled by the GeneratorManager. Most region-scope generators must be passed this struct on construction,
   * in order to perform their generation duties.
   * </summary>
   */
  [DataMember]
  public int seed;
  // represents the number of total zones on the overworld map
  [DataMember]
  public Vector2Int regionDimensions;
  // 24 is the number of tiles in a zone in DPPt (I think. Could be wrong, but it seems like a reasonable baseline)
  // By assuming that each pixel is one tile, we can do exact lookups to find a tile's base height which is helpful for zone generation
  [DataMember]
  public Vector2Int zoneDimensions;

  [DataMember]
  public ZoneGeneratorData[] allZoneData; // flattened 2d array because data contracts don't support 2d ones (x + y*regionDimensions.x)

  [DataMember]
  public Dictionary<string, SubBiomeGeneratorData> allBiomeData;

  public static bool SaveRegion(string fileName, RegionGeneratorData data)
  {
    // Create a new instance of a StreamWriter
    // to read and write the data.
    string path = Application.persistentDataPath + "/" + fileName;
    // Creating the directory like this will do nothing if it already exists
    string folder = Path.GetDirectoryName(path);
    Directory.CreateDirectory(folder);

    FileStream fs = new FileStream(path, FileMode.Create);
    XmlTextWriter writer = new XmlTextWriter(fs, Encoding.Unicode);
    writer.Formatting = Formatting.Indented;
    DataContractSerializer ser = new DataContractSerializer(data.GetType());
    ser.WriteObject(writer, data);
    writer.Close();
    fs.Close();
    return true;
  }
}

// TODO: construct these based on scriptable objects with things like type info, tileset info, etc.
// each item in the scriptable object should be static, and have its data accessible via the biomeName
[DataContract(Name = "SubBiomeGeneratorData", Namespace = "http://schemas.datacontract.org/2004/07/UnityEngine")]
public struct SubBiomeGeneratorData
{
  public string biomeName;

  //public TilesetExtensionInfo
}


[DataContract(Name = "SubBiomeZoneData", Namespace = "http://schemas.datacontract.org/2004/07/UnityEngine")]
public struct SubBiomeZoneData
{
  [DataMember]
  public Vector2Int biomeCenterOverworldCoordinates;

  [DataMember]
  public string biomeName;
  // TODO: encounter data needs to reference the subbiome, but should be stored in ZoneGeneratorData, probably via biomeName
}

[DataContract(Name = "ZoneGeneratorData", Namespace = "http://schemas.datacontract.org/2004/07/UnityEngine")]
public struct ZoneGeneratorData
{
  /**<summary>
   * <c>ZoneGeneratorData</c> is a struct created by the macro-generators (aka, the generators that handle construction of metadata like biome, map height, etc)
   * for the representation of a zone, without fully constructing it. <c>ZoneGenerators</c> should recieve it before GenerateZone() is called, if needed.
   * </summary>
   */
  [DataMember]
  public Vector2Int OverworldCoordinates;

  [DataMember]
  public float[] heightMap; // TODO: Data contracts don't support 2d arrays, so this needs to be flattened

  [DataMember]
  public string zoneType;
  [DataMember]
  public string layer;
  [DataMember]
  public HashSet<string> tags; // for zonewide metadata, E.G. "land" for zones that are primarily land

  // SubBiomes consist of generator data and a weight for this specific zone. Sorted Highest weight to lowest
  [DataMember]
  List<KeyValuePair<SubBiomeZoneData, float>> SubBiomes;
}