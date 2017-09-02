using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent (typeof(Controller2D))]
public class Player : MonoBehaviour {

	Controller2D controller;

	public float moveSpeed = 6.0f;
	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = 0.4f;

	float accelerationTimeAirborne = 0.06f;
	float accelerationTimeGrounded = 0.04f;

	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallLeap;
	public float wallSlideSpeedMax;
	public float wallStickTime = 0.25f;
	float timeToUnstick;

	float gravity;
	float maxDoubleJumpVelocity;
	float maxJumpVelocity;
	float minJumpVelocity;
	float velocityXSmoothing;
	Vector3 velocity;

	Vector2 directionalInput;
	bool wallSliding;
	int wallDirX;

	bool facingRight = true;
	public bool canDoubleJump = false;

	// Use this for initialization
	void Start () {
		controller = GetComponent<Controller2D> ();
		gravity = -2 * maxJumpHeight / (timeToJumpApex * timeToJumpApex);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		maxDoubleJumpVelocity = maxJumpVelocity * 0.75f;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);
	}

	// Update is called once per frame
	void Update () {
		CalculateVelocity ();
		HandleWallSliding ();

		controller.Move (velocity * Time.deltaTime, directionalInput);

		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}
		if (controller.collisions.below) {
			canDoubleJump = true;
		}
	}

	public void SetDirectionalInput(Vector2 input){
		directionalInput = input;
	}

	public void OnJumpInputDown(){
		if (wallSliding) {
			if (wallDirX == directionalInput.x) {
				//climb jump
				velocity.x = -wallDirX * wallJumpClimb.x;
				velocity.y = wallJumpClimb.y;
			} else if (directionalInput.x == 0) {
				//hop off
				velocity.x = -wallDirX * wallJumpOff.x;
				velocity.y = wallJumpOff.y;
			} else {
				//leap away
				velocity.x = -wallDirX * wallLeap.x;
				velocity.y = wallLeap.y;
			}
		} else if (controller.collisions.below) {
			velocity.y = maxJumpVelocity;
		} else if (canDoubleJump) {
			velocity.y = maxDoubleJumpVelocity;
			canDoubleJump = false;
		}
	}

	public void OnJumpInputUp(){
		if (velocity.y > minJumpVelocity) {
			velocity.y = minJumpVelocity;
		}
	}

	public void OnRollInputDown(){
		//could have predicate variables that can be swapped out
		//to check if an action is allowed or not
		if (!controller.collisions.below) {

		}
	}

	void CalculateVelocity(){
		float targetVelocityX = directionalInput.x * moveSpeed;

		if (facingRight && targetVelocityX < 0) {
			facingRight = false;
		} else if (!facingRight && targetVelocityX > 0) {
			facingRight = true;
		}

		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing,
			(controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);
		velocity.y += gravity * Time.deltaTime;
	}

	void HandleWallSliding(){
		wallDirX = (controller.collisions.left) ? -1 : 1;
		wallSliding = false;
		if ((controller.collisions.left || controller.collisions.right) &&
			!controller.collisions.below && velocity.y < 0) {
			wallSliding = true;
			canDoubleJump = true;

			if (velocity.y < -wallSlideSpeedMax) {
				//TODO - add delay where you latch onto the wall for a moment before sliding down
				velocity.y = -wallSlideSpeedMax;
			}
			if (timeToUnstick > 0) {
				velocityXSmoothing = 0;
				velocity.x = 0;
				if (directionalInput.x != wallDirX && directionalInput.x != 0) {
					timeToUnstick -= Time.deltaTime;
				} else {
					timeToUnstick = wallStickTime;
				}
			} else {
				timeToUnstick = wallStickTime;
			}

		}
	}

	struct State {
		public bool canMove;
		public bool canJump;
		public bool canWallSlide;
		public bool canWallJump;
		public bool canDoubleJump;
		public bool canRoll;

	}
}
