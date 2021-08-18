using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "NewTable", menuName = "ScriptableObjects/Utilities/InitializableRuntimeTable", order = 1)]
public abstract class InitializableRuntimeTable<TKey, TValue> : RuntimeTable<TKey, TValue>
{
  // Dictionaries can't be serialized by unity as a property, so we store a list with our pairs, and then dispose at runtime after setting it up.
  public List<TKey> InsertionKeyset;
  public List<TValue> InsertionValueset;

  public void Init()
  {
    KeyPair.Clear();
    if(InsertionValueset.Count != InsertionKeyset.Count)
    {
      Debug.Log("Length of " + name + "Insertion values and Insertion keys do not match");
      return;
    }
    for(int i = 0; i < InsertionKeyset.Count; i++)
    {
      Add(InsertionKeyset[i], InsertionValueset[i]);
    }
  }
}
