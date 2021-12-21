using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeGenerator : RegionGenerator
{
  public BiomeGenerator(RegionGeneratorData _regionData) : base(_regionData)
  {
    Random.InitState(regionData.seed);
    generatedTexture = new Texture2D(regionData.regionDimensions.x * regionData.zoneDimensions.x, regionData.regionDimensions.y * regionData.zoneDimensions.y);
  }

  public void Generate()
  {
    // thoughts on this:
    // 'biome hotspots':
    //    * Every biome is represented by a hotspot zone, where the 'center' of the biome is located. Zones get a biome based on closeness to a biome
    //    * from there, each zone is given some biome data that's used during zone generation to decide which 'mons go where, what the terrain looks like, etc
    // Ways to determine biome hotspots:
    //    * full random (bad)
    //    * identify map features: lakes, mountains, closeness to poles, etc, and use that (good, but harder)
  }

}
