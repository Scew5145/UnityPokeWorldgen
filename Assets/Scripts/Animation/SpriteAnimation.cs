using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*There's two main variants for spritesheets: Symmetric and Asymmetric.
 *  Asymmetric spritesheets are 12 total frames.
 *  Symmetric spritesheets are 7 total frames. to create the 5 "missing" frames, you mirror like so,
 *  where A is the asymmetric frame and S is the mirrored symmetric frame:
 *    A2 = mS1 | A5 = mS2 | A9 : mS6 | A10 : mS7 | A11 : mS8
*/

/*
 * Using this Class:
 * Whatever controls the animation should only make calls to SetMoveState. SpriteAnimation is public, 
 * so use it in other animator classes if you need non-moving animations. As far as I know, outside of the Player, 
 * there's very few objects that should have moving sprite animations extended beyond walk/idle.
 *  
 */

[CreateAssetMenu(fileName = "NewAnimation", menuName = "ScriptableObjects/Animations/Sprite Animation", order = 1)]
public class SpriteAnimation : ScriptableObject
{
  /*
   * I would prefer to store these as key-value pairs of times to frames, but there's a couple of reasons I don't:
   * 1. Unity Can't display or allow you to edit KeyValuePair objects in-editor for scriptable objects
   * 2. If we ever need to extend SpriteAnimation to contain more data, it's easier to add another List than rebuild one big coupled list
   */
  public List<float> Times;
  public List<int> SpriteFrames;
  public float length;

  public bool IsValidAnimation()
  {
    float previousTime = -1.0f;
    foreach(float Time in Times)
    {
      if(previousTime > Time)
      {
        // Error: mis-ordered keyframes, animation will not play properly
        Debug.LogWarning(name + " has mis-ordered keyframes");
        return false;
      }
      else if(previousTime == Time)
      {
        // Two keyframes have the same time, first one will be skipped during playback
        Debug.LogWarning(name + " has overlapping keyframes");
        return false;
      }
      previousTime = Time;
    }

    if(previousTime > length)
    {
      // The length of the animation is shorter than the last keyframe, some of the animation will be skipped
      Debug.LogWarning(name + " has unreachable frames due to an invalid length");
      return false;
    }
    return Times.Count == SpriteFrames.Count;
  }

  public void Awake()
  {
    bool isValid = IsValidAnimation();
    if(!isValid)
    {
      Debug.LogWarning("Animation " + name + " is invalidly constructed! Check the keyframes of the ScriptableObject.");
    }
  }
}
