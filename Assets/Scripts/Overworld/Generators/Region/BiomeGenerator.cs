using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Mathematics;
using UnityEngine;

public class BiomeGenerator : RegionGenerator
{
  public BiomeGenerator(RegionGeneratorData _regionData) : base(_regionData)
  {
  }

  // Mostly, we'll be tagging zones with "biome_" tags for use during generation.
  // TODO: Biome percentages for secondary biome data (for blending and other stuff)

  // All of the following List<HashSet<ZoneGeneratorData>> groups represent contiguous areas with a certain property (water has water, land has land, etc)
  // Each individual HashSet is referred to as a 'cluster'.
  // Each list is sorted by total area in pixels. It IS POSSIBLE for a zone to be in two separate clusters, and even expected in some cases,
  // e.g. shore is always in both land and water. It is also possible for a zone to be part of 2+ clusters from the same group (e.g. the zone has both a lake and ocean in it) 
  // No cluster is nessecarily convex.
  public List<HashSet<ZoneGeneratorData>> waterZoneClusters = new List<HashSet<ZoneGeneratorData>>(); // ocean, lake 1, lake 2, etc.
  public List<HashSet<ZoneGeneratorData>> landZoneClusters = new List<HashSet<ZoneGeneratorData>>(); // mainland, island 1, island 2, etc.
  public List<HashSet<ZoneGeneratorData>> mountainZoneClusters = new List<HashSet<ZoneGeneratorData>>(); // mountaintop 1, mountaintop 2, etc.

  // Each "typeCenter" represents the middle of a cluster of zones that has its biome modified by a secondary Pokemon type.
  // all of these are chosen as random weighted points, effectively. Some are centered on clusters (like peaks/lakes) or on cities.
  public HashSet<ZoneGeneratorData> typeCenters = new HashSet<ZoneGeneratorData>();

  public void Generate()
  {
    // thoughts on this:
    // 'biome hotspots':
    //    * Every biome is represented by a hotspot zone, where the 'center' of the biome is located. Zones get a biome based on closeness to a hotspot
    //    * from there, each zone is given some biome data that's used during zone generation to decide which 'mons go where, what the terrain looks like, etc
    // Ways to determine biome hotspots:
    //    * full random (bad)
    //    * identify map features: lakes, mountains, closeness to poles, etc, and use that (good, but harder)


    // Feature identification: Mountains, Lakes, Ocean, Islands
    float[,] newHeightMap = BiomeFeatureIdentifier.CreateHeightMap(regionData);

    // Every maximal location on the map, usually mountainous 
    //Debug.Log("Doing mountain Clusters");
    List<HashSet<Vector2Int>> maximumClusters = BiomeFeatureIdentifier.FindHeightClusters(newHeightMap, float.MinValue,
      (a, b) => { return a == b; },
      (a, b) => { return a > b; },
      (a, b) => { return a == b; });

    // Every minimal location on the map, aka the water -- this is SLOW. May want to work on getting this faster
    //Debug.Log("Doing water Clusters");
    List<HashSet<Vector2Int>> waterClusters = BiomeFeatureIdentifier.FindHeightClusters(newHeightMap, float.MaxValue,
      (a, b) => { return a == b; },
      (a, b) => { return a < b; },
      (a, b) => { return a == b; });

    //Debug.Log("Doing land Clusters");
    List<HashSet<Vector2Int>> landClusters = BiomeFeatureIdentifier.FindHeightClusters(newHeightMap, 0.0f,
      (a, b) => { return a > b; },
      (a, b) => { return false; },
      (a, b) => { return a > 0.0f; });

    waterClusters.Sort((x, y) => { return x.Count.CompareTo(y.Count); });

    // Label zones via clusters
    for (int clusterIndex = 0; clusterIndex < waterClusters.Count; clusterIndex++)
    {
      HashSet<Vector2Int> zonesTouched = HeightmapClusterToZoneCluster(waterClusters[clusterIndex]);
      String tagType = clusterIndex == (waterClusters.Count - 1) ? "biome_ocean" : "biome_lake";
      HashSet<ZoneGeneratorData> zoneCluster = new HashSet<ZoneGeneratorData>();
      foreach (Vector2Int overworldZone in zonesTouched)
      {
        ZoneGeneratorData zoneData = GetZoneData(overworldZone);
        zoneData.tags.Add(tagType);
        zoneCluster.Add(zoneData);
      }
      waterZoneClusters.Add(zoneCluster);
    }

    landClusters.Sort((x, y) => { return x.Count.CompareTo(y.Count); });
    for (int clusterIndex = 0; clusterIndex < landClusters.Count; clusterIndex++)
    {
      HashSet<Vector2Int> zonesTouched = HeightmapClusterToZoneCluster(landClusters[clusterIndex]);
      String tagType = clusterIndex == (landClusters.Count - 1) ? "biome_land" : "biome_island";
      HashSet<ZoneGeneratorData> zoneCluster = new HashSet<ZoneGeneratorData>();
      foreach (Vector2Int overworldZone in zonesTouched)
      {
        ZoneGeneratorData zoneData = GetZoneData(overworldZone);
        zoneData.tags.Add(tagType);
        zoneCluster.Add(zoneData);
      }
      landZoneClusters.Add(zoneCluster);
    }

    maximumClusters.Sort((x, y) => { return x.Count.CompareTo(y.Count); });
    for (int clusterIndex = 0; clusterIndex < maximumClusters.Count; clusterIndex++)
    {
      HashSet<Vector2Int> zonesTouched = HeightmapClusterToZoneCluster(maximumClusters[clusterIndex]);
      HashSet<ZoneGeneratorData> zoneCluster = new HashSet<ZoneGeneratorData>();
      foreach (Vector2Int overworldZone in zonesTouched)
      {
        ZoneGeneratorData zoneData = GetZoneData(overworldZone);
        zoneData.tags.Add("biome_mountain_peak");
        zoneCluster.Add(zoneData);
      }
      mountainZoneClusters.Add(zoneCluster);
    }

    // City tags - some will be tagged as urban, others won't. 
    // TODO: easy optimization here at the cost of coupling CityGenerator to BiomeGenerator more - recieve this list straight from CityGenerator
    List<ZoneGeneratorData> cities = GatherZonesOfTag("city");

    // At max, half the cities will be treated as urban
    // TODO: cities that take up more than one zone - this will need to change
    if(cities.Count > 0)
    {
      int numberOfUrbanCities = UnityEngine.Random.Range(1, cities.Count / 2);
      for (int cityIndex = 0; cityIndex < cities.Count; cityIndex++)
      {
        cities[cityIndex].tags.Add((cityIndex < numberOfUrbanCities) ? "biome_city_urban" : "biome_city_rural");
      }
    }

    // Another big part of generation here, separated for the sake of my sanity
    PlaceSubBiomes();

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
        /*bool foundCluster = false;
        for (int clusterIndex = 0; clusterIndex < waterClusters.Count; clusterIndex++)
        {
          if (waterClusters[clusterIndex].Contains(new Vector2Int(x, y)))
          {
            Color colorToUse = colorList[clusterIndex % colorList.Count];
            pix[x + (y * generatedTexture.width)] = colorToUse;
            foundCluster = true;
            break;
          }
        }*/
        Vector2Int zoneCoords = TilePositionToZone(new Vector2Int(x, y));
        ZoneGeneratorData zoneData = GetZoneData(zoneCoords);
        if(zoneData.tags.Contains("subbiome_center"))
        {
          pix[x + (y * generatedTexture.width)] = Color.red;
        }
        /*else if (zoneData.tags.Contains("biome_ocean"))
        {
          pix[x + (y * generatedTexture.width)] = Color.blue;
        }
        else if (zoneData.tags.Contains("biome_lake"))
        {
          pix[x + (y * generatedTexture.width)] = Color.cyan;
        }
        else if (zoneData.tags.Contains("biome_island"))
        {
          pix[x + (y * generatedTexture.width)] = Color.yellow;
        }*/
        else
        {
          pix[x + (y * generatedTexture.width)] = Color.white;
        }
      }
    }
    generatedTexture.SetPixels(pix);
    generatedTexture.Apply();
  }

  // Basic idea behind this function is pretty simple. We place a set number of 'sub-biome centers' 
  // That inform the surrounding zones with stuff like tileset information, pokemon encounters, gym types, etc.
  // The closer a zone is to the center of a sub-biome, the more weight that sub-biome will affect the encounters/tilesets/etc.
  // Using Possion Disc Sampling, similar to cities, so that they aren't too closer to each other.  
  protected void PlaceSubBiomes()
  {

    //TODO: more initialization stuff to move to an input struct for the generator
    float subBiomePlacementRadius = 12.0f; // units in 'zones'
    int numSubBiomes = 10;
    int maxPlacementAttempts = 10;

    List<ZoneGeneratorData> subBiomes = new List<ZoneGeneratorData>();
    List<ZoneGeneratorData> triedZones = new List<ZoneGeneratorData>();
    List<ZoneGeneratorData> validUntriedZones = new List<ZoneGeneratorData>();
    Vector2Int centerPoint = new Vector2Int(regionData.regionDimensions.x / 2, regionData.regionDimensions.y / 2);

    // This wonderful set of casts is just multiplying the region dimensions by the placement radius and rounding
    Vector2Int actualRadius = new Vector2Int((int)math.round(((float)regionData.regionDimensions.x) * subBiomePlacementRadius),
      (int)math.round(((float)regionData.regionDimensions.y) * subBiomePlacementRadius));
    Debug.Log(actualRadius);
    foreach (ZoneGeneratorData tempZone in regionData.allZoneData)
    {
      float distanceToCenter = Vector2Int.Distance(centerPoint, tempZone.OverworldCoordinates);
      if(distanceToCenter <= subBiomePlacementRadius)
      {
        validUntriedZones.Add(tempZone);
      }
    }
    float minDistanceBetweenSubBiomes = 4;
    // TODO: gather untried zones here based on closeness to center of the map

    for (int subBiomeIndex = 0; subBiomeIndex < numSubBiomes; subBiomeIndex++)
    {
      int attempts = 0;
      // Attempt poisson disk sampling until we run out of attempts.
      while (attempts <= maxPlacementAttempts)
      {
        if (validUntriedZones.Count == 0)
        {
          Debug.LogWarning("Ran out of space to place zones with good spacing! D:");
          break;
        }
        int index = UnityEngine.Random.Range(0, validUntriedZones.Count);
        // Debug.Log("Placing: " + validUntriedZones[index].OverworldCoordinates.x + " " + validUntriedZones[index].OverworldCoordinates.y);
        float distanceFromSubBiome = float.MaxValue;
        for (int placedIndex = 0; placedIndex < subBiomes.Count; placedIndex++)
        {
          ZoneGeneratorData tempPlacedBiome = subBiomes[placedIndex];
          float distanceToPlaced = Vector2Int.Distance(validUntriedZones[index].OverworldCoordinates, tempPlacedBiome.OverworldCoordinates);
          // Debug.Log("Checking: " + placedCity.OverworldCoordinates.x + " " + placedCity.OverworldCoordinates.y);
          // Debug.Log(distanceToPlaced);
          if (distanceToPlaced < distanceFromSubBiome)
          {
            distanceFromSubBiome = distanceToPlaced;
          }
        }
        // Debug.Log(distanceFromCity);
        if (distanceFromSubBiome <= minDistanceBetweenSubBiomes)
        {
          // Debug.LogWarning("Failed a placement");
        }
        else
        {
          // Found a valid cell, add it to the list of zones
          subBiomes.Add(validUntriedZones[index]);
          validUntriedZones.RemoveAt(index);
          break;
        }
        triedZones.Add(validUntriedZones[index]);
        validUntriedZones.RemoveAt(index);
        attempts += 1;
      }
      // If we fell all the way out of the attempt cycle without finding one, we'll add it afterwards.
      // The next city won't try the ones we've already attempted (because they're removed from validUntriedZones)
    }
    validUntriedZones.AddRange(triedZones); // Re-add the zones for future operations

    foreach(ZoneGeneratorData zone in subBiomes)
    {
      zone.tags.Add("subbiome_center");
    }

    foreach(ZoneGeneratorData zone in regionData.allZoneData)
    {
      // Goal here is to find the three closest biome centers for each zone, and then weight that zone's biome data accordingly
      // technically we only need to do this on actual player-accessible zones but doing it now is fine, just slower
      foreach(ZoneGeneratorData subBiomeCenter in subBiomes)
      {
        if(subBiomeCenter.Equals(zone))
        {
          continue;
        }
      }
    }
  }

  // Helper function for reduction of a set of points to a set of zones
  public  HashSet<Vector2Int> HeightmapClusterToZoneCluster(HashSet<Vector2Int> heightMapCluster)
  {
    HashSet<Vector2Int> zoneCluster = new HashSet<Vector2Int>();
    foreach(Vector2Int point in heightMapCluster)
    {
      Vector2Int zoneCoords = TilePositionToZone(new Vector2Int(point.x, point.y));
      zoneCluster.Add(zoneCoords);
    }
    return zoneCluster;
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
      //float frameStartTimePassing = Time.realtimeSinceStartup;
      List<Vector2Int> passingPixels = new List<Vector2Int>();
      for (int y = 0; y < heightMap.GetLength(1); y++)
      {
        for (int x = 0; x < heightMap.GetLength(0); x++)
        {
          if (comparison(heightMap[x, y], comparisonValue))
          {
            passingPixels.Add(new Vector2Int(x, y));
          }
          else if (iterateComparison(heightMap[x, y], comparisonValue))
          {
            passingPixels.Clear(); // new max/min/etc, empty out those we saved since they're now at a lower height
            comparisonValue = heightMap[x, y];
            //Debug.Log("New highest:" + comparisonValue);
            passingPixels.Add(new Vector2Int(x, y));
          }
        }
      }
      //Debug.Log("took " + (Time.realtimeSinceStartup - frameStartTimePassing) + " to find all passing pixels");
      HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
      List<HashSet<Vector2Int>> clusters = new List<HashSet<Vector2Int>>();
      foreach (Vector2Int point in passingPixels)
      {
        //float frameStartTime = Time.realtimeSinceStartup;
        if (visited.Contains(point))
        {
          continue;
        }
        HashSet<Vector2Int> newCluster = FindClusterScanFill(point, heightMap, sameClusterComparison);
        foreach (Vector2Int clusteredPoint in newCluster)
        {
          visited.Add(clusteredPoint);
        }
        clusters.Add(newCluster);
        //Debug.Log("took " + (Time.realtimeSinceStartup - frameStartTime) + " to find cluster " + clusters.Count);
      }


      return clusters;
    }

    // Gathers all adjacent points that pass "comparison" (good for finding lakes, mountain peaks, etc. once you id a pixel that's within a region)
    // Note: this method of Flood fill is what I can only refer to as SLOW AS BALLS. Do not use this function. Use FindClusterScanFill instead
    // I'm talking literaly minutes vs like 3 seconds for the same set of operations
    public static HashSet<Vector2Int> FindCluster(Vector2Int startPoint, float[,] heightMap, Func<float, float, bool> comparison)
    {
      HashSet<Vector2Int> cluster = new HashSet<Vector2Int>();
      Stack<Vector2Int> pointStack = new Stack<Vector2Int>();
      pointStack.Push(startPoint);
      int requeCount = 0;
      //TODO: using the height of the start point here isn't always what we want - right now it's hard coded in comparison but that's dumb
      float height = heightMap[startPoint.x, startPoint.y];
      while (pointStack.Count != 0)
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
        for (int i = -1; i < 2; i++)
        {
          for (int j = -1; j < 2; j++)
          {
            Vector2Int newPoint = new Vector2Int(currentPoint.x + i, currentPoint.y + j);
            if (cluster.Contains(newPoint))
            {
              requeCount++;
              continue;
            }
            pointStack.Push(newPoint);
          }
        }
        cluster.Add(currentPoint);
      }
      Debug.Log("Reque Count: " + requeCount);
      return cluster;

    }


    // Helper for the following function
    struct PointStackItem
    {
      public int x1, x2, y, dy;
      public PointStackItem(int inX1, int inX2, int inY, int inDy)
      {
        x1 = inX1;
        x2 = inX2;
        y = inY;
        dy = inDy;
      }
    };
    // Gathers all adjacent points that pass "comparison" (good for finding lakes, mountain peaks, etc. once you id a pixel that's within a region)
    public static HashSet<Vector2Int> FindClusterScanFill(Vector2Int startPoint, float[,] heightMap, Func<float, float, bool> comparison)
    {
      // A version of the above function based on the optimized scan fill from here: https://en.wikipedia.org/wiki/Flood_fill
      HashSet<Vector2Int> cluster = new HashSet<Vector2Int>();
      //TODO: using the height of the start point here isn't always what we want - right now it's hard coded in comparison() otherwise but that's dumb
      float height = heightMap[startPoint.x, startPoint.y];
      // Early out for if our starting point is bad - should never happen really but otherwise a decent check
      if (!comparison(height, height))
      {
        return cluster;
      }

      bool Inside(int x, int y)
      {
        // Check our bounds, check if the pixel passes comparison(), and check if we've already decided to use the pixel
        return (!(x < 0 || x >= heightMap.GetLength(0))) &&
          (!(y < 0 || y >= heightMap.GetLength(1))) &&
          comparison(heightMap[x, y], height) && 
          (!cluster.Contains(new Vector2Int(x,y)));
      }

      Stack<PointStackItem> pointStack = new Stack<PointStackItem>();
      pointStack.Push(new PointStackItem(startPoint.x, startPoint.x, startPoint.y, 1));
      pointStack.Push(new PointStackItem(startPoint.x, startPoint.x, startPoint.y, -1));
      int x;
      while(pointStack.Count != 0)
      {
        PointStackItem currentPointGroup = pointStack.Pop();
        x = currentPointGroup.x1;
        if (Inside(x, currentPointGroup.y))
        {
          while(Inside(x - 1, currentPointGroup.y))
          {
            cluster.Add(new Vector2Int(x - 1, currentPointGroup.y));
            x = x - 1;
          }
        }
        if(x < currentPointGroup.x1)
        {
          pointStack.Push(new PointStackItem(x, currentPointGroup.x1 - 1, currentPointGroup.y - currentPointGroup.dy, -currentPointGroup.dy));
        }
        while(currentPointGroup.x1 <= currentPointGroup.x2)
        {
          while(Inside(currentPointGroup.x1, currentPointGroup.y))
          {
            cluster.Add(new Vector2Int(currentPointGroup.x1, currentPointGroup.y));
            currentPointGroup.x1 = currentPointGroup.x1 + 1;
          }
          pointStack.Push(new PointStackItem(x, currentPointGroup.x1 - 1, currentPointGroup.y + currentPointGroup.dy, currentPointGroup.dy));
          if(currentPointGroup.x1 - 1 > currentPointGroup.x2)
          {
            pointStack.Push(new PointStackItem(currentPointGroup.x2 + 1, currentPointGroup.x1 - 1, currentPointGroup.y - currentPointGroup.dy, -currentPointGroup.dy));
          }
          currentPointGroup.x1 = currentPointGroup.x1 + 1;
          while (currentPointGroup.x1 < currentPointGroup.x2 && !Inside(currentPointGroup.x1, currentPointGroup.y))
          {
            currentPointGroup.x1 = currentPointGroup.x1 + 1;
          }
          x = currentPointGroup.x1;
        }
      }
      


      return cluster;
    }
  }
}