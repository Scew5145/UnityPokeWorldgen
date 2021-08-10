using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  // Collision
  private BoxCollider playerCollider;
  const float collRayStartOffset = 0.49f;
  const float collRayLength = 1.0f;
  
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
    RaycastHit hit;
    
    Vector3 raycastDirection = new Vector3(move.x, 0, move.y);
    Vector3 offsetCenter = playerCollider.center + playerCollider.transform.position;
    
    Ray colliderRay = new Ray(offsetCenter + raycastDirection * collRayStartOffset, raycastDirection);
    // Debug.DrawRay(offsetCenter + raycastDirection * collRayStartOffset, raycastDirection * collRayLength, Color.blue, 1.0f);

    if (Physics.Raycast(colliderRay, out hit, collRayLength))
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
    string debugMessage = distanceMoved.magnitude + " " + lastMoveInput.magnitude + " | " + lastMoveInput * playerBaseSpeed;
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
  }

  void RoundPosition()
  {
    gameObject.transform.position = new Vector3(Mathf.Round(gameObject.transform.position.x),
                                                Mathf.Round(gameObject.transform.position.y),
                                                Mathf.Round(gameObject.transform.position.z));
  }
}
