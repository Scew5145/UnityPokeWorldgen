using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * You might be wondering:
 * Why is this not using Unity's built in sprite animator?
 * Isn't this worse? Why this to animate stuff?
 * Short Answer: theirs sucks for sprite animation.
 * Longer Answer:
 *   Unity's animator, specifically for sprites, is not exposed to the user. For a single walking + idle animation set,
 *   It would require you to create 4 walking animations and 4 idle animations, with overrides for each direction, for EVERY CHARACTER.
 *   You can't make them dynamically on load, either, because that stuff isn't exposed to the user outside the editor module.
 *   This, obviously, is stupid. Instead, I've just animated Walking, Idling, etc. here, at 30fps.
*/

/*There's two main variants for spritesheets: Symmetric and Asymmetric.
 *  Asymmetric spritesheets are 12 total frames.
 *  Symmetric spritesheets are 7 total frames. to create the 5 "missing" frames, you mirror like so,
 *  where A is the asymmetric frame and S is the mirrored symmetric frame:
 *    A2 = mS1 | A5 = mS2 | A9 : mS6 | A10 : mS7 | A11 : mS8
*/

public struct SpriteAnimation
{
  public readonly List<KeyValuePair<float, int>> Keyframes;
  public readonly float length;
  public SpriteAnimation(List<KeyValuePair<float, int>> k, float l)
  {
    Keyframes = new List<KeyValuePair<float,int>>(k); 
    length = l;
  }

}


public class OverworldMoveAnimate : MonoBehaviour
{
  public enum OverworldMoveState
  {
    Idle,
    Moving,
    Running,
    Biking,
    Surfing,
    Bumped,
    Special
  }

  public enum EFacingDirection
  {
    Up,
    Down,
    Left,
    Right
  }
  
  public Texture2D spriteSheet;
  protected SpriteRenderer sRenderer;
  public bool isSymmetric = false;
  protected OverworldMoveState moveState = OverworldMoveState.Idle;
  protected OverworldMoveState lastMoveState = OverworldMoveState.Idle;
  public EFacingDirection direction = EFacingDirection.Up;
  protected EFacingDirection lastDirection = EFacingDirection.Up;
  protected int currentFrame = 0;
  protected float currentTime = 0.0f;
  protected float nextTime = 0.0f;
  protected Sprite[] sprites;

  public void SetMoveState(OverworldMoveState inMoveState)
  {
    if(moveState != inMoveState)
    {
      // Debug.Log(inMoveState);
      moveState = inMoveState;
    }
  }

  public void SetDirection(EFacingDirection inDirection)
  {
    if(direction != inDirection)
    {
      direction = inDirection;
    }
  }

  protected static SpriteAnimation WalkBase = new SpriteAnimation(new List<KeyValuePair<float, int>>
  {
    new KeyValuePair<float, int>(0.0f, 0),
    new KeyValuePair<float, int>((5.0f/30.0f), 1),
    new KeyValuePair<float, int>((10.0f/30.0f), 0),
    new KeyValuePair<float, int>((15.0f/30.0f), 2),
  }, 20.0f / 30.0f);

  private void Start()
  {
    sprites = Resources.LoadAll<Sprite>(spriteSheet.name);
    sRenderer = GetComponent<SpriteRenderer>();
  }

  // Update is called once per frame
  void Update()
  {
    if(moveState != lastMoveState)
    {
      currentFrame = 0;
      currentTime = 0.0f;
      nextTime = 0.0f;
    }
    switch(moveState)
    {
      case OverworldMoveState.Idle: // Idle state is simple, can be optimized as just having a single movement
        if (moveState == lastMoveState && lastDirection == direction)
        {
          break;
        }
        else
        {
          sRenderer.sprite = sprites[3 * (int)direction];
        }
        break;
      case OverworldMoveState.Moving:
        int spIndex = GetSpriteIndex(WalkBase, direction);
        sRenderer.sprite = sprites[spIndex];
        break;
    }
    lastDirection = direction;
    lastMoveState = moveState;
  }

  int GetSpriteIndex(SpriteAnimation spriteAnim, EFacingDirection directionToGet)
  {
    if(spriteAnim.Keyframes.Count == 0)
    {
      Debug.LogError("Sprite Animation 0 Keyframes! This is a bad time!");
      return -1;
    }
    if (currentTime >= nextTime)
    {
      
      currentFrame += 1;
      if(currentFrame >= spriteAnim.Keyframes.Count)
      {
        nextTime = spriteAnim.length;
      }
      else
      {
        nextTime = spriteAnim.Keyframes[currentFrame].Key;
      }
      if (currentFrame > spriteAnim.Keyframes.Count)
      {
        currentFrame = 0;
        currentTime = 0.0f;
        nextTime = 0.0f;
      }
      // print("advance" + spriteAnim.Keyframes[currentFrame].Value + " | " + currentTime);
    }
    currentTime += Time.deltaTime;
    int displayedFrame = currentFrame % spriteAnim.Keyframes.Count;
    return (spriteAnim.Keyframes[displayedFrame].Value) + 3 * (int)direction;
  }
}
