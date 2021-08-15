using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ZoneStreamer is a singleton class used for managing the world level streaming at runtime.
// Primarily, it's job is to hold information on which zones are streamed in and which ones aren't, As well as handle events
// When a zone is entered or exited. 
// The ZoneFactory class is responsible for registering any created scenes with this streamer, and for interfacing with ZoneGenerators.
// The ZoneGenerator class is responsible for populating each scene registered here.
public sealed class ZoneStreamer : ScriptableObject
{
  private static ZoneStreamer instance = null;
  private static readonly object padlock = new object();
  ZoneStreamer()
  {
  }
  public static ZoneStreamer Streamer
  {
    get
    {
      lock (padlock)
      {
        if (instance == null)
        {
          instance = new ZoneStreamer();
        }
        return instance;
      }
    }
  }

  
}

