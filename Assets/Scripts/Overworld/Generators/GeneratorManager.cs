using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorManager : MonoBehaviour
{
  public ZoneGeneratorData[,] GeneratedZoneData;

  public TerrainGenerator tGen;
  public CityGenerator cGen;

  // represents the number of total zones on the overworld map
  public Vector2Int regionDimensions = new Vector2Int(32, 32);
  public int seed;

  public ZoneGeneratorData[,] zoneData;

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
    seed = (int)(Random.Range(0.0f, 1.0f) * 10000);
    Debug.Log("Seed: " + seed);
  }

  void Update()
  {
    switch (currentStep)
    {
      case GenerationStep.Idle:
        break;
      case GenerationStep.Started:
        zoneData = new ZoneGeneratorData[regionDimensions.x, regionDimensions.y];
        currentStep = GenerationStep.Terrain;
        break;
      case GenerationStep.Terrain:
        tGen = new TerrainGenerator(seed, zoneData, regionDimensions.x * 24, regionDimensions.y * 24); 
        // 24 is the number of tiles in a zone in DPPt
        // By assuming that each pixel is one tile, we can do exact lookups to find a tile's base height which is helpful for zone generation
        tGen.Generate();
        currentStep = GenerationStep.Cities;
        break;
      case GenerationStep.Cities:
        cGen = new CityGenerator(seed, zoneData);
        currentStep = GenerationStep.Finished;
        break;
      case GenerationStep.Finished:
        GenerationFinishedEvent.Raise(this);
        currentStep = GenerationStep.Idle;
        break;
    }
  }
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