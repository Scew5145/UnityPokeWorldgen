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

  // Movement
  private Vector2 lastMoveInput = new Vector2(0, 0);
  private Vector2 distanceMoved = new Vector2(0, 0);
  private float playerBaseSpeed = 4.0f;
  private float runMultiplier = 2.0f;

  enum MovementState { Idle, Moving, Running };
  MovementState MoveState = MovementState.Idle;
  private void Start()
  {
    playerCollider = gameObject.GetComponent<BoxCollider>();
    playerCollider.isTrigger = false;
  }

  void Update()
  {
    
    if (MoveState != MovementState.Idle)
    {
      DoMove();
      return;
    }
    Vector2 wantedMovement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    MovementState wantedState = wantedMovement == Vector2.zero ? MovementState.Idle : MovementState.Moving;
    if(wantedState == MovementState.Idle)
    {
      return;
    }
    // If the B button is pressed, while wanting to move
    else if (wantedState == MovementState.Moving && Input.GetAxis("Deny") != 0.0f) 
    {
      wantedState = MovementState.Running;
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
      // TODO: BUMP STATE HERE
      return;
    }


    lastMoveInput = move;
    MoveState = wantedState;
  }

  void DoMove()
  {
    Vector2 move = lastMoveInput * Time.deltaTime * playerBaseSpeed;
    if(MoveState == MovementState.Running)
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
      MoveState = MovementState.Idle;
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
