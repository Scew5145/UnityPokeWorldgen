using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class for region-wide generator constructs. Generally in charge of generating a single feature of the biome (terrain, biome, city locations, etc)
public class RegionGenerator
{
  public Texture2D generatedTexture;
  public readonly RegionGeneratorData regionData;

  public RegionGenerator(RegionGeneratorData _regionData)
  {
    regionData = _regionData;
    Random.InitState(regionData.seed);
    generatedTexture = new Texture2D(regionData.regionDimensions.x * regionData.zoneDimensions.x, regionData.regionDimensions.y * regionData.zoneDimensions.y);
  }

  public Vector2Int TilePositionToZone(Vector2Int TileCoordinates)
  {
    Vector2Int OutputCoordinates = new Vector2Int();
    OutputCoordinates.x = TileCoordinates.x / regionData.zoneDimensions.x;
    OutputCoordinates.y = TileCoordinates.y / regionData.zoneDimensions.y;

    return OutputCoordinates;
  }
}