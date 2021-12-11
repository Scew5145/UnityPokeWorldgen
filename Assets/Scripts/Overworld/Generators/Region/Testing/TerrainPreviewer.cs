using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPreviewer : MonoBehaviour
{
  // Start is called before the first frame update
  TerrainGenerator tGen;
  Texture2D outputTexture;
  Renderer rend;
  void Start()
  {
    rend = GetComponent<Renderer>();
    int seed = (int)(Random.Range(0.0f, 1.0f) * 10000);
    Debug.Log("Seed: " + seed);
    tGen = new TerrainGenerator(seed);
    ComputeCombinedTexture();
    rend.material.mainTexture = outputTexture;
  }

  void ComputeCombinedTexture()
  {
    tGen.CalcTerrain();

    outputTexture = new Texture2D(tGen.mapWidth, tGen.mapHeight);
    Color[] pix = new Color[outputTexture.width * outputTexture.height];
    int y = 0;
    Color waterColor = new Color(0.5f, 0.7f, 0.8f);
    while (y < outputTexture.height)
    {
      int x = 0;
      while (x < outputTexture.width)
      {
        if(tGen.terrainTex.GetPixel(x, y) == Color.black)
        {
          pix[(int)y * outputTexture.width + (int)x] = waterColor;
        }
        else
        {
          pix[(int)y * outputTexture.width + (int)x] = Color.Lerp(new Color(0.1f, 0.5f, 0.15f),
          new Color(1.0f, 1.0f, 0.8f),
          tGen.terrainTex.GetPixel(x, y).r - 0.2f);
        }
        
        x++;
      }
      y++;
    }
    outputTexture.SetPixels(pix);
    outputTexture.Apply();

  }
}
