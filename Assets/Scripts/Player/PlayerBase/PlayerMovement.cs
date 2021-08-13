using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  // Collision
  private BoxCollider playerCollider;
  const float collRayStartOffset = 0.49f;
  readonly static float collRayLength = 1.49f; // statically calc this to go fast 

  // Offset for all movement raycasts (from hitbox center). Both horizontal and vertical position. Starting a bit above the bottom of the 1x1 cube
  readonly static Vector3 raycastOffset = new Vector3(0.0f, 0.499f, 0.0f);
  public OverworldMoveAnimate movementAnimationComponent;

  // Movement
  private Vector2 lastMoveInput = new Vector2(1.0f, 0.0f);
  private Vector2 distanceMoved = new Vector2(0, 0);
  private float playerBaseSpeed = 3.0f;
  private float runMultiplier = 2.0f;

  //Bumped State
  private readonly float bumpedCooldown = 1.0f/3.0f;
  private float timeSinceLastBump = 0.0f;
  private Vector2 lastBumpDirection = new Vector2(0, 0);
  private bool movedSinceLastBump = true;
  enum EMovementState { Idle, Moving, Running, Bumped };
  EMovementState MoveState = EMovementState.Idle;
  EMovementState lastMoveState = EMovementState.Idle;
  private void Start()
  {
    playerCollider = gameObject.GetComponent<BoxCollider>();
    playerCollider.isTrigger = false;
  }

  void Update()
  {
    lastMoveState = MoveState;
    HandleMovement();
    if(lastMoveState != MoveState && MoveState != EMovementState.Idle)
    {
      movementAnimationComponent.SetMoveState(GetAnimMoveState());
    }
    else if(MoveState == EMovementState.Idle && lastMoveState == EMovementState.Idle)
    {
      // Wait an extra frame to return to idle, so that we don't tear while running
      movementAnimationComponent.SetMoveState(GetAnimMoveState());
    }
    if (lastMoveInput.x != 0)
    {
      movementAnimationComponent.SetDirection(lastMoveInput.x < 0.0f ? OverworldMoveAnimate.EFacingDirection.Left : OverworldMoveAnimate.EFacingDirection.Right);
    }
    else
    {
      movementAnimationComponent.SetDirection(lastMoveInput.y < 0.0f ? OverworldMoveAnimate.EFacingDirection.Down : OverworldMoveAnimate.EFacingDirection.Up);
    }
  }

  private OverworldMoveAnimate.EOverworldMoveState GetAnimMoveState()
  {
    switch(MoveState)
    {
      case EMovementState.Idle: return OverworldMoveAnimate.EOverworldMoveState.Idle;
      case EMovementState.Moving: return OverworldMoveAnimate.EOverworldMoveState.Moving;
      case EMovementState.Running: return OverworldMoveAnimate.EOverworldMoveState.Running;
      case EMovementState.Bumped: return OverworldMoveAnimate.EOverworldMoveState.Bumped;
    }
    return OverworldMoveAnimate.EOverworldMoveState.Idle;
  }

  void HandleMovement()
  {
    if(MoveState == EMovementState.Bumped)
    {
      //print("cooling" + timeSinceLastBump);
      timeSinceLastBump -= Time.deltaTime;
      if(timeSinceLastBump <= 0.0f)
      {
        MoveState = EMovementState.Idle;
      }
      return;
    }
    if (MoveState != EMovementState.Idle)
    {
      DoMove();
      return;
    }
    Vector2 wantedMovement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    EMovementState wantedState = wantedMovement == Vector2.zero ? EMovementState.Idle : EMovementState.Moving;
    if (wantedState == EMovementState.Idle)
    {
      movedSinceLastBump = true; // Player released the key, consider this "moving" for the sake of bumping
      return;
    }
    // If the B button is pressed, while wanting to move
    if (wantedState == EMovementState.Moving && Input.GetAxis("Deny") != 0.0f)
    {
      wantedState = EMovementState.Running;
    }
    Vector2 move = new Vector2(0.0f, 0.0f);
    if (wantedMovement.x != 0.0f)
    {
      move.x = wantedMovement.x > 0.0f ? 1.0f : -1.0f;
    }
    else
    {
      move.y = wantedMovement.y > 0.0f ? 1.0f : -1.0f;
    }

    Vector3 raycastDirection = new Vector3(move.x, 0.0f, move.y);
    Vector3 offsetCenter = playerCollider.center + playerCollider.transform.position + raycastOffset;
    Ray colliderRay = new Ray(offsetCenter, raycastDirection);
    // Debug.DrawRay(offsetCenter, raycastDirection.normalized * collRayLength, Color.green, 1.0f, true);

    if (Physics.Raycast(colliderRay, out RaycastHit hit, collRayLength))
    {
      if (movedSinceLastBump)
      {
        lastBumpDirection = move;
        wantedState = EMovementState.Bumped;
        timeSinceLastBump = bumpedCooldown;
        movedSinceLastBump = false;
      }
      else
      {
        wantedState = EMovementState.Idle;
      }
    }

    if(wantedState != EMovementState.Idle && wantedState != EMovementState.Bumped)
    {
      movedSinceLastBump = true;
    }
    lastMoveInput = move;
    MoveState = wantedState;
  }

  void DoMove()
  {
    Vector2 move = lastMoveInput * Time.deltaTime * playerBaseSpeed;
    if (MoveState == EMovementState.Moving && Input.GetAxis("Deny") != 0.0f)
    {
      MoveState = EMovementState.Running;
    }
    if (MoveState == EMovementState.Running)
    {
      move *= runMultiplier;
    }
    distanceMoved += move;
    // string debugMessage = distanceMoved.magnitude + " " + lastMoveInput.magnitude + " | " + lastMoveInput * playerBaseSpeed;
    if(distanceMoved.magnitude >= lastMoveInput.magnitude) 
    {
      // gone past target tile, correct to the tile for the next move, and reset to idle
      move = lastMoveInput - distanceMoved;
      distanceMoved = new Vector2(0, 0);
      MoveState = EMovementState.Idle;
      RoundPosition();
      return;
    }

    gameObject.transform.position += new Vector3(move.x, 0.0f, move.y);

    // Vertical movement raycast for ground snapping
    Vector3 raycastStart = playerCollider.center + playerCollider.transform.position;
    Vector3 raycastDirection = new Vector3(0.0f, -1.0f, 0.0f);
    Ray colliderRay = new Ray(raycastStart, raycastDirection);
    // Debug.DrawRay(raycastStart, raycastDirection, Color.red, 1.0f, true);
    if (Physics.Raycast(colliderRay, out RaycastHit hit, 1.5f))
    {
      // Debug.Log("hit ground " + hit.point.y);
      gameObject.transform.position = new Vector3(gameObject.transform.position.x, hit.point.y, gameObject.transform.position.z);
    }
    
  }

  void RoundPosition()
  {
    gameObject.transform.position = new Vector3(Mathf.Round(gameObject.transform.position.x),
                                                gameObject.transform.position.y,
                                                Mathf.Round(gameObject.transform.position.z));
  }
}
