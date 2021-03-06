﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller2D : RaycastController {

	public enum State {stand, crouch, jumpstart, fall, wallsit, walljumpstart, run};
	public Color[] Statecolors = { Color.black, Color.yellow, Color.magenta, Color.blue, Color.green, Color.cyan, Color.red };
	public State state;

	float maxClimbAngle = 80.0f;
	float maxDescendAngle = 75.0f;

	public delegate void FuncHitHazard(Hazard hazard, Vector2 hitDir);
	public FuncHitHazard HitHazard;
	public FuncHitHazard HitHazardSevere;

	public bool vulnerable = true;
	public bool stunned = false;

	public CollisionInfo collisions;

	[HideInInspector]
	public Vector2 playerInput;

	public struct CollisionInfo{
		public bool above, below;
		public bool left, right;
		public bool climbingSlope, descendingSlope;
		public float slopeAngle, slopeAngleOld;
		public Vector2 moveAmountOld;
		public int faceDir;
		public bool fallingThroughPlatform;

		public void Reset(){
			above = below = false;
			left = right = false;
			climbingSlope = false;
			descendingSlope = false;
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}

	public override void Start(){
		base.Start ();
		collisions.faceDir = 1;
		state = State.stand;
	}

	public void Move(Vector2 moveAmount, bool standingOnPlatform){
		Move (moveAmount, Vector2.zero, standingOnPlatform);
	}

	public void Move(Vector2 moveAmount, Vector2 input, bool standingOnPlatform = false){
		UpdateRaycastOrigins ();
		collisions.Reset ();
		collisions.moveAmountOld = moveAmount;
		playerInput = input;

		if (moveAmount.x != 0) {
			collisions.faceDir = (int)Mathf.Sign (moveAmount.x);
		}

		if (moveAmount.y < 0) {
			DescendSlope (ref moveAmount);
		}

		HorizontalCollisions (ref moveAmount);

		if (moveAmount.y != 0) {
			VerticalCollisions (ref moveAmount);
		}

		transform.Translate (moveAmount);
		if (standingOnPlatform) {
			collisions.below = true;
		}
	}

	void HorizontalCollisions(ref Vector2 moveAmount){
		float directionX = collisions.faceDir;
		float rayLength = Mathf.Abs (moveAmount.x) + skinWidth;

		if (Mathf.Abs(moveAmount.x) < skinWidth) {
			rayLength = 2 * skinWidth;
		}

		for (int i = 0; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			Debug.DrawRay (rayOrigin, Vector2.right * directionX, Color.red);

			if (hit) {

				if (hit.distance == 0) {
					continue;//ignore if you a platform is moving through you
				}else if (hit.collider.tag == "Hazard" && vulnerable) {
					//TODO: give hazards a hazard component that can dictate damage/behaviour on contact
					//   would then pass along either that component or null
					HitHazard (hit.collider.GetComponent<Hazard>(), Vector2.right * directionX);
				} else if (hit.collider.tag == "HazardSevere" && vulnerable) {
					HitHazardSevere (hit.collider.GetComponent<Hazard>(), Vector2.right * directionX);
					return;
				}

				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
				if (i == 0 && slopeAngle <= maxClimbAngle) {
					if (collisions.descendingSlope) {
						collisions.descendingSlope = false;
						moveAmount = collisions.moveAmountOld;
					}
					float distanceToSlopeStart = 0;
					if (slopeAngle != collisions.slopeAngleOld) {
						//new slope
						distanceToSlopeStart = hit.distance - skinWidth;
						moveAmount.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope (ref moveAmount, slopeAngle);
					moveAmount.x += distanceToSlopeStart * directionX;
				}

				if (!collisions.climbingSlope || slopeAngle > maxClimbAngle) {
					moveAmount.x = (hit.distance - skinWidth) * directionX; // move to the collision point
					rayLength = hit.distance;//so that hitting multiple obstacles will only move you by the shortest

					if (collisions.climbingSlope) {
						moveAmount.y = Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (moveAmount.x);
					}

					collisions.left = directionX == -1;
					collisions.right = directionX == 1;
				}
			}
		}
	}

	void VerticalCollisions(ref Vector2 moveAmount){
		float directionY = Mathf.Sign (moveAmount.y);
		float rayLength = Mathf.Abs (moveAmount.y) + skinWidth;

		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

			Debug.DrawRay (rayOrigin, Vector2.up * directionY, Color.red);

			if (hit) {
				if (hit.collider.tag == "Through") {
					if (directionY == 1 || hit.distance == 0) {
						//moving up or stuck midway through the platform
						continue;
					}
					if (collisions.fallingThroughPlatform) {
						continue;
					}
					if (playerInput.y == -1) {
						collisions.fallingThroughPlatform = true;
						Invoke ("ResetFallingThroughPlatform", 0.5f);
						continue;
					}
				} else if (hit.collider.tag == "Hazard" && vulnerable) {
					//TODO: give hazards a hazard component that can dictate damage/behaviour on contact
					//   would then pass along either that component or null
					HitHazard (hit.collider.GetComponent<Hazard>(), Vector2.up * directionY);
				} else if (hit.collider.tag == "HazardSevere" && vulnerable) {
					HitHazardSevere (hit.collider.GetComponent<Hazard>(), Vector2.up * directionY);
					return;
				}

				moveAmount.y = (hit.distance - skinWidth) * directionY; // move to the collision point
				rayLength = hit.distance;//so that hitting multiple obstacles will only move you by the shortest

				if (collisions.climbingSlope) {
					moveAmount.x = moveAmount.y / Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad)
						* Mathf.Sign (moveAmount.x);
				}

				collisions.below = directionY == -1;
				collisions.above = directionY == 1;
			}
		}
		if (collisions.climbingSlope) {
			float directionX = Mathf.Sign (moveAmount.x);
			rayLength = Mathf.Abs (moveAmount.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight)
				+ Vector2.up * moveAmount.y;
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength,
				                   collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
				if (slopeAngle != collisions.slopeAngle) {
					moveAmount.x = (hit.distance - skinWidth) * directionX;
					collisions.slopeAngle = slopeAngle;
				}
			}
		}
	}

	void ClimbSlope(ref Vector2 moveAmount, float slopeAngle){
		float moveDistance = Mathf.Abs (moveAmount.x);
		float climbmoveAmountY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (moveAmount.y > climbmoveAmountY) {
			//print ("Jumping on slope");
		} else {
			moveAmount.y = climbmoveAmountY;
			moveAmount.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (moveAmount.x);
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
		}
	}

	void DescendSlope(ref Vector2 moveAmount){
		float directionX = Mathf.Sign (moveAmount.x);
		Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

		if (hit) {
			float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
			if (slopeAngle != 0 && slopeAngle <= maxDescendAngle) {
				if (Mathf.Sign (hit.normal.x) == directionX) {
					if (hit.distance - skinWidth <= Mathf.Tan (slopeAngle * Mathf.Deg2Rad) *
					   Mathf.Abs (moveAmount.x)) {
						float moveDistance = Mathf.Abs (moveAmount.x);
						float descendmoveAmountY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
						moveAmount.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (moveAmount.x);
						moveAmount.y -= descendmoveAmountY;

						collisions.slopeAngle = slopeAngle;
						collisions.descendingSlope = true;
						collisions.below = true;
					}
				}
			}
		}
	}

	void ResetFallingThroughPlatform(){
		collisions.fallingThroughPlatform = false;
	}
}
