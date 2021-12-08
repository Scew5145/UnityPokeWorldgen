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

  public TerrainGenerator(int _seed, int _mapWidth = 512, int _mapHeight = 512, float _scale = 10.0f, float _xOrg = 0.0f, float _yOrg = 0.0f)
  {
    seed = _seed;
    mapWidth = _mapWidth;
    mapHeight = _mapHeight;
    scale = _scale;
    xOrg = _xOrg + seed;
    yOrg = _yOrg + seed;
    terrainTex = new Texture2D(mapWidth, mapHeight);
    simplexNoise = new OpenSimplexNoise(seed);
    pix = new Color[terrainTex.width * terrainTex.height];
    Random.InitState(seed);
  }

  public void CalcTerrain()
  {
    // For each pixel in the texture...
    float y = 0.0f;
    float steps = 6.0f;
    float halfStep = steps / 2.0f;
    float sphereCenterX = 0.5f;//Random.Range(0.45f, 0.55f);
    float sphereCenterY = 0.5f;//Random.Range(0.45f, 0.55f);
    float maxDistanceFromCenter = 0.6f;

    while (y < terrainTex.height)
    {
      float x = 0.0f;
      while (x < terrainTex.width)
      {
        double xCoord = xOrg + x / terrainTex.width * scale;
        double yCoord =  yOrg + y / terrainTex.height * scale;
        float sample = (0.5f + ((float)simplexNoise.Evaluate(xCoord, yCoord) / 2.0f)); // between -1.0 and 1.0 after rescaling it
        //float sample = Mathf.PerlinNoise((float)xCoord, (float)yCoord);

        // A clamped 0.0 to 1.0 function that will make the borders more ocean, and all ocean at the very edge
        float distanceToCenter = (1.0f - (Mathf.Min(maxDistanceFromCenter, Vector2.Distance(new Vector2(sphereCenterX, sphereCenterY), new Vector2(x / terrainTex.width, y / terrainTex.height))) / maxDistanceFromCenter));
        // Rescale the distance scalar to have flatter coasts and mountaintops, with a sharper midsection
        float bezierBlend = distanceToCenter * distanceToCenter * (3.0f - 2.0f * distanceToCenter);
        float modifiedSample = (sample) * bezierBlend; 
        float unstepped = (modifiedSample);
        float stepped = ((Mathf.Ceil(steps * modifiedSample)) / steps);
        if(stepped < 0.2) // 0.2, for now, is sealevel. need to add this to the parameters
        {
          pix[(int)y * terrainTex.width + (int)x] = new Color(0, 0.2f, 0.4f);
        }
        else
        {
          pix[(int)y * terrainTex.width + (int)x] = new Color(stepped, stepped, stepped);
        }
        //pix[(int)y * terrainTex.width + (int)x] = new Color(unstepped, unstepped, unstepped);
        //Debug.Log(pix[(int)y * terrainTex.width + (int)x]);
        x++;
      }
      y++;
    }

    // Copy the pixel data to the texture and load it into the GPU.
    terrainTex.SetPixels(pix);
    terrainTex.Apply();
  }
  
}
