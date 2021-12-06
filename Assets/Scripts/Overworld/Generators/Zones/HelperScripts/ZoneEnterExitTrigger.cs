using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneEnterExitTrigger : MonoBehaviour
{
  public GameEvent enterEvent;
  private void OnTriggerEnter(Collider other)
  {
    if(other.tag == "Player")
    {
      enterEvent.Raise(this);
    }
  }
}
