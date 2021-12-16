using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPreviewer : MonoBehaviour
{
  // TODO: the code in ComputeCombinedTexture should be moved to a Generator manager class.
  // All this should do is recieve the texture from said manager, and draw it on an object.
  // For now this test code lives here
  public GeneratorManager generator;
  Texture2D outputTexture;
  Renderer rend;
  void Start()
  {
    rend = GetComponent<Renderer>();
    
    
    //ComputeCombinedTexture();
  }

  public void ComputeCombinedTexture()
  {
    outputTexture = new Texture2D(generator.tGen.mapWidth, generator.tGen.mapHeight);
    Color[] pix = new Color[outputTexture.width * outputTexture.height];
    int y = 0;
    Color waterColor = new Color(0.5f, 0.7f, 0.8f);
    while (y < outputTexture.height)
    {
      int x = 0;
      while (x < outputTexture.width)
      {
        // zone borders, for debug. TODO: move to terrain previewer
        if (x % 16 == 0 || (y % 16 == 0))
        {
          pix[y * outputTexture.width + x] = new Color(1.0f, 1.0f, 1.0f);
        }
        else if (generator.tGen.terrainTex.GetPixel(x, y) == Color.black)
        {
          pix[y * outputTexture.width + x] = waterColor;
        }
        else
        {
          pix[y * outputTexture.width + x] = Color.Lerp(new Color(0.1f, 0.5f, 0.15f),
          new Color(1.0f, 1.0f, 0.8f),
          generator.tGen.terrainTex.GetPixel(x, y).r - 0.2f);
        }
        
        x++;
      }
      y++;
    }
    outputTexture.SetPixels(pix);
    outputTexture.Apply();
    rend.material.mainTexture = outputTexture;
  }
}
