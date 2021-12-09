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

  public TerrainGenerator(int _seed, int _mapWidth = 256, int _mapHeight = 256, float _scale = 8.0f, float _xOrg = 0.0f, float _yOrg = 0.0f)
  {
    seed = _seed;
    Random.InitState(seed);
    mapWidth = _mapWidth;
    mapHeight = _mapHeight;
    scale = _scale;
    xOrg = _xOrg + Random.Range(-10000.0f, 10000.0f);
    yOrg = _yOrg + Random.Range(-10000.0f, 10000.0f);
    terrainTex = new Texture2D(mapWidth, mapHeight);
    simplexNoise = new OpenSimplexNoise(seed);
    pix = new Color[terrainTex.width * terrainTex.height];
  }

  public void CalcTerrain()
  {
    // For each pixel in the texture...
    float y = 0.0f;
    float steps = 5.0f;
    float waterLevel = 1.0f / steps;
    //float sphereCenterX = 0.5f;//Random.Range(0.45f, 0.55f);
    //float sphereCenterY = 0.5f;//Random.Range(0.45f, 0.55f);
    int numberOfLines = 5;
    float maxDistanceFromCenter = 0.1f;
    float sphereRadius = 0.5f;
    float lineWeight = 0.5f;
    List<Vector2> points = new List<Vector2>();
    for(int i = 0; i < numberOfLines * 2; i++)
    {
      points.Add(new Vector2(Random.Range(0.15f, 0.85f), Random.Range(0.15f, 0.85f)));
    }
   

    while (y < terrainTex.height)
    {
      float x = 0.0f;
      while (x < terrainTex.width)
      {
        
        double xCoord = xOrg + x / terrainTex.width * scale;
        double yCoord = yOrg + y / terrainTex.height * scale;
        float sample = (0.5f + ((float)simplexNoise.Evaluate(xCoord, yCoord) / 2.0f)); // between -1.0 and 1.0 after rescaling it
        //float sample = Mathf.PerlinNoise((float)xCoord, (float)yCoord);

        // A clamped 0.0 to 1.0 function that will make the borders more ocean, and all ocean at the very edge
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

        float distanceToCenter = Mathf.Max(sphereWeight, distanceFromLines * lineWeight);
        // Rescale the distance scalar to have flatter coasts and mountaintops, with a sharper midsection
        //float rescaleHeight = Mathf.Clamp(1 - Mathf.Cos(Mathf.PI * distanceToCenter), 0.0f, 1.0f);
        float rescaleHeight = distanceToCenter * distanceToCenter * (3.0f - 2.0f * distanceToCenter);
        float modifiedSample = (sample) * rescaleHeight; 
        float unstepped = (modifiedSample);
        float stepped = ((Mathf.Ceil(steps * modifiedSample)) / steps);
        //if ((int)x % 8 == 0 || ((int)y % 8 == 0))
        //{
        //  pix[(int)y * terrainTex.width + (int)x] = new Color(1.0f, 1.0f, 1.0f);
        //}
        if (stepped <= waterLevel) // 0.2, for now, is sealevel. need to add this to the parameters
        {
          pix[(int)y * terrainTex.width + (int)x] = new Color(0, 0.1f, 0.35f);
        }
        else
        {
          Color lerpedColor = Color.Lerp(new Color(0.1f, 0.5f, 0.15f), new Color(1.0f, 1.0f, 0.8f), stepped);
          pix[(int)y * terrainTex.width + (int)x] = lerpedColor;
        }
        //pix[(int)y * terrainTex.width + (int)x] = new Color(rescaleHeight, rescaleHeight, rescaleHeight);
        //Debug.Log(pix[(int)y * terrainTex.width + (int)x]);
        x++;
      }
      y++;
    }

    // Copy the pixel data to the texture and load it into the GPU.
    terrainTex.SetPixels(pix);
    terrainTex.Apply();
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
