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
 *   This, obviously, is stupid. Instead, I've just animated Walking, Idling, etc. as scriptable objects.
 *   OverworldMoveAnimate by handles the 'four directions' part of it. Actual Movement of the character is handled in a separate script, See PlayerMovement.cs
*/


public class OverworldMoveAnimate : MonoBehaviour
{
  public enum EOverworldMoveState
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
  public SpriteAnimation WalkBase;
  public Texture2D spriteSheet;
  public SpriteRenderer sRenderer;
  public bool isSymmetric = false;
  protected EOverworldMoveState moveState = EOverworldMoveState.Idle;
  protected EOverworldMoveState lastMoveState = EOverworldMoveState.Idle;
  protected EFacingDirection direction = EFacingDirection.Up;
  protected EFacingDirection lastDirection = EFacingDirection.Up;
  protected int currentFrame = 0;
  protected float currentTime = 0.0f;
  protected float nextTime = 0.0f;
  protected Sprite[] walkSprites;

  public void SetMoveState(EOverworldMoveState inMoveState)
  {
    if (moveState != inMoveState)
    {
      currentFrame = 0;
      currentTime = 0.0f;
      nextTime = 0.0f;
      // Debug.Log(inMoveState);
      moveState = inMoveState;
    }
  }

  public void SetDirection(EFacingDirection inDirection)
  {
    if (direction != inDirection)
    {
      direction = inDirection;
    }
  }

  // I'd like to move this out of here and into a scriptable object or something similar, but for now this will do
  protected static Dictionary<int, KeyValuePair<int, bool>> SymmetricIndexMap = new Dictionary<int, KeyValuePair<int, bool>>
  {
    { 0, new KeyValuePair<int, bool>(0, false) },
    { 1, new KeyValuePair<int, bool>(1, false) },
    { 2, new KeyValuePair<int, bool>(1, true) },
    { 3, new KeyValuePair<int, bool>(2, false) },
    { 4, new KeyValuePair<int, bool>(3, false) },
    { 5, new KeyValuePair<int, bool>(3, true) },
    { 6, new KeyValuePair<int, bool>(4, false) },
    { 7, new KeyValuePair<int, bool>(5, false) },
    { 8, new KeyValuePair<int, bool>(6, false) },
    { 9, new KeyValuePair<int, bool>(4, true) },
    { 10, new KeyValuePair<int, bool>(5, true) },
    { 11, new KeyValuePair<int, bool>(6, true) }
  };

  protected void Start()
  {
    walkSprites = Resources.LoadAll<Sprite>(spriteSheet.name);
  }

  // Update is called once per frame
  void LateUpdate()
  {
    DoAnimation();
    lastDirection = direction;
    lastMoveState = moveState;
  }

  protected virtual void DoAnimation()
  {
    switch (moveState)
    {
      case EOverworldMoveState.Idle: // Idle state is simple, can be optimized as just having a single movement
        if (moveState == lastMoveState && lastDirection == direction)
        {
          break;
        }
        else
        {
          if(isSymmetric)
          {
            KeyValuePair<int, bool> spriteInfo = SymmetricIndexMap[3 * (int)direction];
            sRenderer.sprite = walkSprites[spriteInfo.Key];
            sRenderer.flipX = spriteInfo.Value;
          }
          else
          {
            sRenderer.sprite = walkSprites[3 * (int)direction];
          }
        }
        break;
      case EOverworldMoveState.Moving:
        int spIndex = GetSpriteIndex(WalkBase, direction);
        if (isSymmetric)
        {
          KeyValuePair<int, bool> spriteInfo = SymmetricIndexMap[spIndex];
          sRenderer.sprite = walkSprites[spriteInfo.Key];
          sRenderer.flipX = spriteInfo.Value;
        }
        else
        {
          sRenderer.sprite = walkSprites[spIndex];
        }
        break;
    }
  }

  protected int GetSpriteIndex(SpriteAnimation spriteAnim, EFacingDirection directionToGet)
  {
    if(spriteAnim.Times.Count == 0)
    {
      Debug.LogError("Sprite Animation 0 Keyframes! This is a bad time!");
      return -1;
    }
    if (currentTime >= nextTime)
    {
      
      currentFrame += 1;
      if(currentFrame >= spriteAnim.Times.Count)
      {
        nextTime = spriteAnim.length;
      }
      else
      {
        nextTime = spriteAnim.Times[currentFrame];
      }
      if (currentFrame > spriteAnim.Times.Count)
      {
        currentFrame = 0;
        currentTime = 0.0f;
        nextTime = 0.0f;
      }
      // print("advance" + spriteAnim.Keyframes[currentFrame].Value + " | " + currentTime);
    }
    currentTime += Time.deltaTime;
    int displayedFrame = currentFrame % spriteAnim.Times.Count;
    return (spriteAnim.SpriteFrames[displayedFrame]) + 3 * (int)direction;
  }
}
