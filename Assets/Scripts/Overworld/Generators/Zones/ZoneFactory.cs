using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneFactory : MonoBehaviour
{
  // Base class for manipulating ZoneGenerators, and registering Zones with the ZoneStreamer. 
  /* "Zones" are basically indvidual unity scenes with some important properties:
  * Zones are loaded and unloaded by the ZoneStreamer at runtime
  * Zones are an explicit size, determined by what type of zone they are.
  * Zones have links to their adjacent zones, with accessibility modifiers
  */

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

  }
}
