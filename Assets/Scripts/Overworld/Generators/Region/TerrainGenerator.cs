using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainGenerator
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

  public readonly int seed;

  public Texture2D terrainTex;
  private Color[] pix; // This should be a greyscale texture to go fast, but... L A Z Y
  public readonly ZoneGeneratorData[,] zoneGeneratorData;

  public TerrainGenerator(int _seed, ZoneGeneratorData[,] _zoneGeneratorData, int _mapWidth = 512, int _mapHeight = 512, float _scale = 8.0f, float _xOrg = 0.0f, float _yOrg = 0.0f)
  {
    seed = _seed;
    Random.InitState(seed);
    zoneGeneratorData = _zoneGeneratorData;
    mapWidth = _mapWidth;
    mapHeight = _mapHeight;
    scale = _scale;
    xOrg = _xOrg + Random.Range(-10000.0f, 10000.0f);
    yOrg = _yOrg + Random.Range(-10000.0f, 10000.0f);
    terrainTex = new Texture2D(mapWidth, mapHeight);
    simplexNoise = new OpenSimplexNoise(seed);
    pix = new Color[terrainTex.width * terrainTex.height];
  }

  public void Generate()
  {
    // For each pixel in the texture...
    float steps = 5.0f;
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
    while (y < terrainTex.height)
    {
      float x = 0.0f;
      while (x < terrainTex.width)
      {
        
        double xCoord = xOrg + x / terrainTex.width * scale;
        double yCoord = yOrg + y / terrainTex.height * scale;
        float sample = (0.5f + ((float)simplexNoise.Evaluate(xCoord, yCoord) / 2.0f)); // between -1.0 and 1.0 after rescaling it
        //float sample = Mathf.PerlinNoise((float)xCoord, (float)yCoord);

        Vector2 pixelPosition = new Vector2(x / terrainTex.width, y / terrainTex.height);
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
          pix[(int)y * terrainTex.width + (int)x] = new Color(0.0f, 0.0f, 0.0f);
        }
        else
        {
          // TODO: color code to terrain previewer
          //
          pix[(int)y * terrainTex.width + (int)x] = new Color(stepped, stepped, stepped);
        }
        //pix[(int)y * terrainTex.width + (int)x] = new Color(noiseMask, noiseMask, noiseMask);
        //Debug.Log(pix[(int)y * terrainTex.width + (int)x]);
        x++;
      }
      y++;
    }

    // Copy the pixel data to the texture and load it into the GPU.
    terrainTex.SetPixels(pix);
    terrainTex.Apply();

    int zoneWidth = mapWidth / zoneGeneratorData.GetLength(0);
    int zoneHeight = mapHeight / zoneGeneratorData.GetLength(0);

    // Copy the height data into the zones
    for (int i = 0; i < zoneGeneratorData.GetLength(0); i++)
    {
      for (int j = 0; j < zoneGeneratorData.GetLength(1); j++)
      {
        zoneGeneratorData[i, j].OverworldCoordinates = new Vector2Int(i, j);
        zoneGeneratorData[i, j].heightMap = new float[zoneWidth, zoneHeight];
        zoneGeneratorData[i, j].layer = "overworld";
        for (int posX = 0; posX < zoneWidth; posX++)
        {
          for(int posY = 0; posY < zoneHeight; posY++)
          {
            zoneGeneratorData[i, j].heightMap[posX, posY] = terrainTex.GetPixel(i * zoneWidth + posX,j * zoneHeight + posY).r;
          }
        }
      }
    }
  }

  // Helper functions for ray-based masks
  public Vector2 Normalized2D(Vector2 vector)
  {
    float mag = vector.magnitude;
    if(mag > .00001 )
    {
        return (vector / mag);
    }
    return Vector2.zero;
  }
 
  public Vector2 ClosestPointOnLine(Vector2 vA, Vector2 vB, Vector2 vPoint)
  {
    Vector2 vVector1 = vPoint - vA;
    Vector2 vVector2 = Normalized2D(vB - vA);

    float d = Vector2.Distance(vA, vB);
    float t = Vector2.Dot(vVector2, vVector1);

    if (t <= 0)
      return vA;

    if (t >= d)
      return vB;

    Vector2 vVector3 = vVector2 * t;

    Vector2 vClosestPoint = vA + vVector3;

    return vClosestPoint;
  }
}