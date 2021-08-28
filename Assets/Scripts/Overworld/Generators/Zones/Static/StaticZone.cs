using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;

[DataContract(Name = "TestZone", Namespace = "http://schemas.datacontract.org/2004/07/UnityEngine")]
public class StaticZone : Zone
{
  // No data to store here: not storing any random values of any sort, just the base Zone data.
  // It may make sense to store rotation data here at some point, but we need it on creation too, so its probably better handled by the 
  // Generator class itself instead
}
