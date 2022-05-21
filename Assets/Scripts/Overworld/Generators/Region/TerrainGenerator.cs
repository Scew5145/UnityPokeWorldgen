using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainGenerator : RegionGenerator // TODO: all region generators should inherit from the same class
{
  // Base class for building out terrain to a 2D texture that can be sampled as a heightmap.
  // Most generation-dependent values are marked as readonly because they describe the map being built.
  // We don't want to change them after generation, because it'll 1. do nothing and 2. give inaccurate information to anything that wants to know 

  public readonly int mapWidth;
  public readonly int mapHeight;

  private OpenSimplexNoise simplexNoise;

  // Sampling of the noise texture is the same as the example from https://docs.unity3d.com/ScriptReference/Mathf.PerlinNoise.html
  // Biggest difference is that we're not using the perlin noise function and instead using OpenSimplex as a basis
  // The origin of the sampled area in the plane.
  public readonly float xOrg;
  public readonly float yOrg;

  // The number of cycles of the basic noise pattern that are repeated
  // over the width and height of the texture.
  public float scale;


  // TODO: TerrainGenerator constructor that accepts RegionGeneratorData and uses its values instead of internal ones to remove duplicate values
  public TerrainGenerator(RegionGeneratorData _regionData, float _scale = 8.0f, float _xOrg = 0.0f, float _yOrg = 0.0f) : base(_regionData)
  {
    mapWidth = regionData.regionDimensions.x * regionData.zoneDimensions.x;
    mapHeight = regionData.regionDimensions.y * regionData.zoneDimensions.y;
    scale = _scale;
    xOrg = _xOrg + Random.Range(-10000.0f, 10000.0f);
    yOrg = _yOrg + Random.Range(-10000.0f, 10000.0f);
    simplexNoise = new OpenSimplexNoise(regionData.seed);
  }

  public void Generate()
  {
    // pix array is local so we can discard it after the terrain texture is generated, if needed
    Color[] pix = new Color[generatedTexture.width * generatedTexture.height]; // This should be a greyscale texture to go fast, but... L A Z Y
    float steps = 6.0f;
    float waterLevel = 1.0f / steps;
    // The vast majority of these parameters should be moved into a initialization struct param for TerrainGenerator.
    // Preferably, a ScriptableObject so we can set-and-forget them and quickly switch between configs as needed
    int numberOfLines = 5;
    float maxDistanceFromCenter = 0.15f;
    float sphereRadius = 0.5f;
    float lineWeight = 0.6f;
    List<Vector2> points = new List<Vector2>();
    for(int i = 0; i < numberOfLines * 2; i++)
    {
      points.Add(new Vector2(Random.Range(0.15f, 0.85f), Random.Range(0.15f, 0.85f)));
    }

    float y = 0.0f;
    while (y < generatedTexture.height)
    {
      float x = 0.0f;
      while (x < generatedTexture.width)
      {
        
        double xCoord = xOrg + x / generatedTexture.width * scale;
        double yCoord = yOrg + y / generatedTexture.height * scale;
        float sample = (0.5f + ((float)simplexNoise.Evaluate(xCoord, yCoord) / 2.0f)); // between -1.0 and 1.0 after rescaling it
        //float sample = Mathf.PerlinNoise((float)xCoord, (float)yCoord);

        Vector2 pixelPosition = new Vector2(x / generatedTexture.width, y / generatedTexture.height);
        float minDistance = float.MaxValue;
        for(int line = 0; line < numberOfLines; line++)
        {
          Vector2 posClosestToLine = ClosestPointOnLine(points[line * 2], points[line * 2 + 1], pixelPosition);
          minDistance = Mathf.Min(minDistance, Vector2.Distance(pixelPosition, posClosestToLine));
        }

        float sphereWeight = (1.0f - (Mathf.Min(Vector2.Distance(new Vector2(0.5f, 0.5f), pixelPosition), sphereRadius) 
          / sphereRadius));

        float distanceFromLines = (1.0f - (Mathf.Min(maxDistanceFromCenter, minDistance)
          / maxDistanceFromCenter));

        float noiseMask = Mathf.Max(sphereWeight, distanceFromLines * lineWeight);
        // Rescale the distance scalar to have flatter coasts and mountaintops, with a sharper midsection, lookup "Bezier Curves"
        float rescaleHeight = noiseMask * noiseMask * (3.0f - 2.0f * noiseMask);
        float modifiedSample = (sample) * rescaleHeight; 
        float unstepped = (modifiedSample);
        float stepped = ((Mathf.Ceil(steps * modifiedSample)) / steps);
        
        if (stepped <= waterLevel)
        {
          pix[(int)y * generatedTexture.width + (int)x] = new Color(0.0f, 0.0f, 0.0f);
        }
        else
        {
          // TODO: color code to terrain previewer
          //
          pix[(int)y * generatedTexture.width + (int)x] = new Color(stepped, stepped, stepped);
        }
        //pix[(int)y * terrainTex.width + (int)x] = new Color(noiseMask, noiseMask, noiseMask);
        //Debug.Log(pix[(int)y * terrainTex.width + (int)x]);
        x++;
      }
      y++;
    }

    // Copy the pixel data to the texture and load it into the GPU.
    generatedTexture.SetPixels(pix);
    generatedTexture.Apply();

    int zoneWidth = regionData.zoneDimensions.x;
    int zoneHeight = regionData.zoneDimensions.y;
    int tilesInZone = zoneWidth * zoneHeight;
    // Copy the height data into the zones
    for (int i = 0; i < regionData.regionDimensions.x; i++)
    {
      for (int j = 0; j < regionData.regionDimensions.y; j++)
      {
        regionData.allZoneData[i + j*regionData.regionDimensions.x].OverworldCoordinates = new Vector2Int(i, j);
        regionData.allZoneData[i + j*regionData.regionDimensions.x].heightMap = new float[zoneWidth * zoneHeight];
        regionData.allZoneData[i + j*regionData.regionDimensions.x].layer = "overworld";
        int tilesOfLandInZone = 0;
        for (int posX = 0; posX < zoneWidth; posX++)
        {
          for(int posY = 0; posY < zoneHeight; posY++)
          {
            regionData.allZoneData[i + j*regionData.regionDimensions.x].heightMap[posX + posY * regionData.zoneDimensions.x] = generatedTexture.GetPixel(i * zoneWidth + posX,j * zoneHeight + posY).r;
            if(regionData.allZoneData[i + j*regionData.regionDimensions.x].heightMap[posX + posY * regionData.zoneDimensions.x] != 0.0f)
            {
              tilesOfLandInZone += 1;
            }
          }
        }
        if(regionData.allZoneData[i + j*regionData.regionDimensions.x].tags == null)
        {
          regionData.allZoneData[i + j*regionData.regionDimensions.x].tags = new List<string>();
        }
        if(tilesInZone/2 <= tilesOfLandInZone)
        {
          regionData.allZoneData[i + j*regionData.regionDimensions.x].tags.Add("land");
        }
        else
        {
          regionData.allZoneData[i + j*regionData.regionDimensions.x].tags.Add("water");
        }
      }
    }
  }
}