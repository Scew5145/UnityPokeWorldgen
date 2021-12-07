using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRuntimeTable<TKey, TValue>
{
  TValue this[TKey key]
  {
    get;
    set;
  }
}

public abstract class RuntimeTable<TKey, TValue> : ScriptableObject, IRuntimeTable<TKey, TValue>
{
  protected Dictionary<TKey, TValue> KeyPair = new Dictionary<TKey, TValue>();
  public void Add(TKey key, TValue value)
  {
    if (!KeyPair.ContainsKey(key))
    {
      KeyPair.Add(key, value);
    }
  }

  public void Remove(TKey key)
  {
    if (KeyPair.ContainsKey(key))
    {
      KeyPair.Remove(key);
    }
  }

  public bool ContainsKey(TKey key)
  {
    return KeyPair.ContainsKey(key);
  }

  public TValue this[TKey key]
  {
    get
    {
      return KeyPair[key];
    }
    set
    {
      KeyPair[key] = value;
    }
  }

  public void Clear()
  {
    KeyPair.Clear();
  }
}
