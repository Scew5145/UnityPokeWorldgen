using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPreviewer : MonoBehaviour
{
  // TODO: the code in ComputeCombinedTexture should be moved to a Generator manager class.
  // All this should do is recieve the texture from said manager, and draw it on an object.
  // For now this test code lives here
  public GeneratorManager generator;
  Texture2D gridPreviewTexture;
  Renderer rend;
  void Start()
  {
    rend = GetComponent<Renderer>();
    
    
    //ComputeCombinedTexture();
  }

  public void ComputeCombinedTexture()
  {
    gridPreviewTexture = new Texture2D(generator.tGen.mapWidth, generator.tGen.mapHeight);
    Color[] pix = new Color[gridPreviewTexture.width * gridPreviewTexture.height];
    int y = 0;
    while (y < gridPreviewTexture.height)
    {
      int x = 0;
      while (x < gridPreviewTexture.width)
      {
        // zone borders, for debug. TODO: move to terrain previewer
        if (x % generator.regionData.zoneDimensions.x == 0 || (y % generator.regionData.zoneDimensions.y == 0))
        {
          pix[y * gridPreviewTexture.width + x] = Color.white;
        }
        else
        {
          pix[y * gridPreviewTexture.width + x] = Color.black;
        }
        
        x++;
      }
      y++;
    }
    gridPreviewTexture.SetPixels(pix);
    gridPreviewTexture.Apply();
    rend.material.SetTexture("_Heightmap", generator.tGen.generatedTexture);
    rend.material.SetTexture("_Grid", gridPreviewTexture);
    rend.material.SetTexture("_Cities", generator.cGen.generatedTexture);
  }
}
