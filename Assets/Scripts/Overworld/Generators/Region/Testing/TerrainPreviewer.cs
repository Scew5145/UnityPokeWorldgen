using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPreviewer : MonoBehaviour
{
  // Start is called before the first frame update
  TerrainGenerator tGen;
  Renderer rend;
  void Start()
  {
    rend = GetComponent<Renderer>();
    int seed = (int)(Random.Range(0.0f, 1.0f) * 256);
    Debug.Log("Seed: " + seed);
    tGen = new TerrainGenerator(seed);
    tGen.CalcTerrain();
    rend.material.mainTexture = tGen.terrainTex;
  }
}
