using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BiomeGenerator : RegionGenerator
{
  public BiomeGenerator(RegionGeneratorData _regionData) : base(_regionData)
  {
  }

  public void Generate()
  {
    // thoughts on this:
    // 'biome hotspots':
    //    * Every biome is represented by a hotspot zone, where the 'center' of the biome is located. Zones get a biome based on closeness to a hotspot
    //    * from there, each zone is given some biome data that's used during zone generation to decide which 'mons go where, what the terrain looks like, etc
    // Ways to determine biome hotspots:
    //    * full random (bad)
    //    * identify map features: lakes, mountains, closeness to poles, etc, and use that (good, but harder)

    // Feature identification: Mountains
    float[,] newHeightMap = BiomeFeatureIdentifier.CreateHeightMap(regionData);

    // Every maximal location on the map, usually mountainous 
    List<HashSet<Vector2Int>> maximumClusters = BiomeFeatureIdentifier.FindHeightClusters(newHeightMap, float.MinValue,
      (a, b) => { return a == b; },
      (a, b) => { return a > b; },
      (a, b) => { return a == b; });
    //Debug.Log("Maximums Cluster count:" + maximumClusters.Count);

    // Every minimal location on the map, aka the water -- this is SLOW. May want to work on getting this faster
    List<HashSet<Vector2Int>> waterClusters = BiomeFeatureIdentifier.FindHeightClusters(newHeightMap, float.MaxValue,
      (a, b) => { return a == b; },
      (a, b) => { return a < b; },
      (a, b) => { return a == b; });

    List<HashSet<Vector2Int>> landClusters = BiomeFeatureIdentifier.FindHeightClusters(newHeightMap, 0.0f,
      (a, b) => { return a > b; },
      (a, b) => { return false; },
      (a, b) => { return a > 0.0f; });

    waterClusters.Sort((x, y) => { return x.Count.CompareTo(y.Count); });

    // Label zones via clusters
    for (int clusterIndex = 0; clusterIndex < waterClusters.Count; clusterIndex++)
    {
      HashSet<Vector2Int> zonesTouched = new HashSet<Vector2Int>();
      // Debug.Log("Cluster " + clusterIndex + " pixel count: " + waterClusters[clusterIndex].Count);
      foreach(Vector2Int coordinate in waterClusters[clusterIndex])
      {
        // Debug.Log(coordinate);
        zonesTouched.Add(TilePositionToZone(coordinate));
      }
      Debug.Log(zonesTouched.Count);
      String tagType = clusterIndex == (waterClusters.Count - 1) ? "water_ocean" : "water_lake";
      foreach (Vector2Int overworldZone in zonesTouched)
      {
        regionData.allZoneData[overworldZone.x + overworldZone.y * regionData.regionDimensions.x].tags.Add(tagType);
        Debug.Log("ZoneCoords:" + overworldZone);
      }
    }

    landClusters.Sort((x, y) => { return x.Count.CompareTo(y.Count); });
    for (int clusterIndex = 0; clusterIndex < landClusters.Count; clusterIndex++)
    {
      HashSet<Vector2Int> zonesTouched = new HashSet<Vector2Int>();
      // Debug.Log("Cluster " + clusterIndex + " pixel count: " + landClusters[clusterIndex].Count);
      foreach (Vector2Int coordinate in landClusters[clusterIndex])
      {
        // Debug.Log(coordinate);
        zonesTouched.Add(TilePositionToZone(coordinate));
      }
      Debug.Log(zonesTouched.Count);
      String tagType = clusterIndex == (landClusters.Count - 1) ? "land_main" : "land_island";
      foreach (Vector2Int overworldZone in zonesTouched)
      {
        regionData.allZoneData[overworldZone.x + overworldZone.y * regionData.regionDimensions.x].tags.Add(tagType);
        Debug.Log("ZoneCoords:" + overworldZone);
      }
    }

    maximumClusters.Sort((x, y) => { return x.Count.CompareTo(y.Count); });
    for (int clusterIndex = 0; clusterIndex < maximumClusters.Count; clusterIndex++)
    {
      HashSet<Vector2Int> zonesTouched = new HashSet<Vector2Int>();
      foreach (Vector2Int coordinate in maximumClusters[clusterIndex])
      {
        // Debug.Log(coordinate);
        zonesTouched.Add(TilePositionToZone(coordinate));
      }
      Debug.Log(zonesTouched.Count);
      String tagType = clusterIndex == (maximumClusters.Count - 1) ? "land_main" : "land_island";
      foreach (Vector2Int overworldZone in zonesTouched)
      {
        regionData.allZoneData[overworldZone.x + overworldZone.y * regionData.regionDimensions.x].tags.Add(tagType);
        Debug.Log("ZoneCoords:" + overworldZone);
      }
    }

    // Debug preview texture, just change which set of clusters are being looked at to debug them
    List<Color> colorList = new List<Color>();
    colorList.Add(Color.red);
    colorList.Add(Color.blue);
    colorList.Add(Color.green);
    colorList.Add(Color.yellow);
    colorList.Add(Color.cyan);
    colorList.Add(Color.magenta);
    Color[] pix = new Color[generatedTexture.width * generatedTexture.height];
    for (int y = 0; y < newHeightMap.GetLength(1); y++)
    {
      for (int x = 0; x < newHeightMap.GetLength(0); x++)
      {
        bool foundCluster = false;
        for (int clusterIndex = 0; clusterIndex < waterClusters.Count; clusterIndex++)
        {
          if (waterClusters[clusterIndex].Contains(new Vector2Int(x, y)))
          {
            Color colorToUse = colorList[clusterIndex % colorList.Count];
            pix[x + (y * generatedTexture.width)] = colorToUse;
            foundCluster = true;
            break;
          }
        }
        if(!foundCluster)
        {
          pix[x + (y * generatedTexture.width)] = Color.black;
        }
      }
    }
    generatedTexture.SetPixels(pix);
    generatedTexture.Apply();
  }

  public class BiomeFeatureIdentifier
  {
    // Technically, all of this could be done just using it in the zone format, but frankly, it's easier to think about
    // when its all packed in a 2d float array like this. 
    public static float[,] CreateHeightMap(RegionGeneratorData dataToParse)
    {
      float[,] newHeightMap = new float[dataToParse.regionDimensions.x * dataToParse.zoneDimensions.x, dataToParse.regionDimensions.y * dataToParse.zoneDimensions.y];
      for (int overworldY = 0; overworldY < dataToParse.regionDimensions.y; overworldY++)
      {
        for (int overworldX = 0; overworldX < dataToParse.regionDimensions.x; overworldX++)
        {
          for (int zoneY = 0; zoneY < dataToParse.zoneDimensions.y; zoneY++)
          {
            for (int zoneX = 0; zoneX < dataToParse.zoneDimensions.x; zoneX++)
            {
              newHeightMap[overworldX * dataToParse.zoneDimensions.x + zoneX, overworldY * dataToParse.zoneDimensions.y + zoneY] = dataToParse.allZoneData[overworldX + overworldY * dataToParse.regionDimensions.x]
                .heightMap[zoneX + zoneY * dataToParse.zoneDimensions.x];
            }
          }
        }
      }
      return newHeightMap;
    }

    // Function for finding an area of the map that satisfies the comparison function, separated into clusters
    // the function parameter names are a little confusing, but what they do is straightforward:
    // "comparison" is the primary check that's being made to add something to the list of points the function spits out, e.g. { return a == b }.
    // "iterateComparison" is the check that's made if the first fails, to see if we need to change the comparison value for future pixel checks,
    // e.g. { return a > b }. The two functions combined would be equivalent to "find all pixels with the highest value on the map."
    // If you just need the first single comparison, just set iterateComparison to a function that just returns false.
    public static List<HashSet<Vector2Int>> FindHeightClusters(float[,] heightMap, 
      float comparisonValue, // our starting value to determine which pixels should be considered using 'comparison'
      Func<float, float, bool> comparison, // How to determine which set of pixels will be allowed in the comparison
      Func<float, float, bool> iterateComparison, // iterates the previous comparison, e.g. if we find a set of pixels that are higher height then our previous
      Func<float, float, bool> sameClusterComparison) // how to determine which adjacent pixels are considered to be part of the same cluster (usually a == b)
    {
      List<Vector2Int> passingPixels = new List<Vector2Int>();
      for (int y = 0; y < heightMap.GetLength(1); y++)
      {
        for (int x = 0; x < heightMap.GetLength(0); x++)
        {
          if(comparison(heightMap[x, y], comparisonValue))
          {
            passingPixels.Add(new Vector2Int(x, y));
          }
          else if(iterateComparison(heightMap[x,y], comparisonValue))
          {
            passingPixels.Clear(); // new max/min/etc, empty out those we saved since they're now at a lower height
            comparisonValue = heightMap[x, y];
            //Debug.Log("New highest:" + comparisonValue);
            passingPixels.Add(new Vector2Int(x, y));
          }
        }
      }
      HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
      List<HashSet<Vector2Int>> clusters = new List<HashSet<Vector2Int>>();
      foreach(Vector2Int point in passingPixels)
      {
        if(visited.Contains(point))
        {
          continue;
        }
        HashSet<Vector2Int> newCluster = FindCluster(point, heightMap, sameClusterComparison);
        foreach(Vector2Int clusteredPoint in newCluster)
        {
          visited.Add(clusteredPoint);
        }
        clusters.Add(newCluster);
      }


      return clusters;
    }

    // Gathers all adjacent points (good for finding lakes, mountain peaks, etc. once you id a pixel that's within a region)
    public static HashSet<Vector2Int> FindCluster(Vector2Int startPoint, float[,] heightMap, Func<float, float, bool> comparison)
    {
      HashSet<Vector2Int> cluster = new HashSet<Vector2Int>();
      Stack<Vector2Int> pointStack = new Stack<Vector2Int>();
      pointStack.Push(startPoint);
      float height = heightMap[startPoint.x, startPoint.y];
      while(pointStack.Count != 0)
      {
        Vector2Int currentPoint = pointStack.Pop();
        // Bounds checks
        if (currentPoint.x < 0 || currentPoint.x >= heightMap.GetLength(0))
        {
          continue;
        }
        if (currentPoint.y < 0 || currentPoint.y >= heightMap.GetLength(1))
        {
          continue;
        }

        if (!(comparison(heightMap[currentPoint.x, currentPoint.y], height))) // if it doesn't pass our same cluster comparison, we can ignore it
        {
          continue;
        }
        // Add all adjacent pixels to the stack, assuming we haven't visited them yet.
        // Including diagonals for the rare case of a body of water tapering at exactly 45 degrees, leading to incorrectly separated bodies of water (seed: 630058)
        // This is MUCH slower, though. Oppurtunity for optimization here - recombine step on minimal regions?
        for(int i = -1; i < 2; i++)
        {
          for(int j = -1; j < 2; j++)
          {
            Vector2Int newPoint = new Vector2Int(currentPoint.x + i, currentPoint.y + j);
            if (cluster.Contains(newPoint))
            {
              continue;
            }
            pointStack.Push(newPoint);
          }
        }
        cluster.Add(currentPoint);
      }
      return cluster;
      
    }
  }
}