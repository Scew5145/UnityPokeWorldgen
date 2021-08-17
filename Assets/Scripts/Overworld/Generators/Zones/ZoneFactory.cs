using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneFactory : MonoBehaviour
{
  // Base class for manipulating ZoneGenerators, and registering Zones with the ZoneStreamer. 
  /* "Zones" are basically indvidual unity scenes with some important properties:
  * Zones are loaded and unloaded by the ZoneStreamer at runtime
  * Zones are an explicit size, determined by what type of zone they are.
  * Zones have links to their adjacent zones, with accessibility modifiers, preferably
  */
  /* ZoneFactory #TODO: 
   * This needs to get written. If I need to start with just a base that Makes a zone, generates it, and then asks for the Streamer to load it,
   * that's good enough for now.
   * Once that's done, I'll need to write variants of this class: RouteZoneFactory, DungeonZoneFactory, etc.
   * Then this class will be the one that decides which ZoneGenerator to call for which zone. It'll be sick.
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
