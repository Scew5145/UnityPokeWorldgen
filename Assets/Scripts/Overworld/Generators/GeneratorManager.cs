using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorManager : MonoBehaviour
{
  public TerrainGenerator tGen;
  public CityGenerator cGen;

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
    regionData.seed = (int)(Random.Range(0.0f, 1.0f) * 10000);
    regionData.regionDimensions = new Vector2Int(48, 48);
    regionData.zoneDimensions = new Vector2Int(24, 24);
    Debug.Log("Seed: " + regionData.seed);
  }

  void Update()
  {
    switch (currentStep)
    {
      case GenerationStep.Idle:
        break;
      case GenerationStep.Started:
        regionData.allZoneData = new ZoneGeneratorData[regionData.regionDimensions.x, regionData.regionDimensions.y];
        currentStep = GenerationStep.Terrain;
        break;
      case GenerationStep.Terrain:
        tGen = new TerrainGenerator(regionData.seed, regionData.allZoneData, regionData.regionDimensions.x * regionData.zoneDimensions.x, regionData.regionDimensions.y * regionData.zoneDimensions.y); 
        tGen.Generate();
        currentStep = GenerationStep.Cities;
        break;
      case GenerationStep.Cities:
        cGen = new CityGenerator(regionData);
        cGen.Generate();
        currentStep = GenerationStep.Finished;
        break;
      case GenerationStep.Finished:
        GenerationFinishedEvent.Raise(this);
        currentStep = GenerationStep.Idle;
        break;
    }
  }
}

public struct RegionGeneratorData
{
  /**<summary>
   * <c>RegionGeneratorData</c> is a struct created and controlled by the GeneratorManager. Most region-scope generators must be passed this struct on construction,
   * in order to perform their generation duties.
   * </summary>
   */
  public int seed;
  // represents the number of total zones on the overworld map
  public Vector2Int regionDimensions;
  // 24 is the number of tiles in a zone in DPPt
  // By assuming that each pixel is one tile, we can do exact lookups to find a tile's base height which is helpful for zone generation
  public Vector2Int zoneDimensions; 
  public ZoneGeneratorData[,] allZoneData;
}

public struct ZoneGeneratorData
{
  /**<summary>
   * <c>ZoneGeneratorData</c> is a struct created by the macro-generators (aka, the generators that handle construction of metadata like biome, map height, etc)
   * for the representation of a zone, without fully constructing it. <c>ZoneGenerators</c> should recieve it before GenerateZone() is called, if needed.
   * </summary>
   */

  public Vector2Int OverworldCoordinates;

  public float[,] heightMap;

  public string zoneType;
  public string layer;
  public List<string> tags; // for zonewide metadata, E.G. "land" for zones that are primarily land

}