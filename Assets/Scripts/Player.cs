using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent (typeof(Controller2D))]
public class Player : MonoBehaviour {

	public Controller2D controller;

	public int maxHealth;
	public int health;

	float respawnPosTime = 0;
	public float respawnPosInterval;
	public Vector3 respawnPos;
	public float respawnDuration = 1.0f;
	public float iframeTime = 0.2f;
	float iframes;
	public Material iframeMat;
	public Material normalMat;

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
	public Vector3 externalVelocity;
	public float stunTime;//maybe seperate to have something for externals

	Vector2 directionalInput;
	bool wallSliding;
	int wallDirX;

	bool facingRight = true;
	public bool canDoubleJump = false;


	// Use this for initialization
	void Start () {
		respawnPos = this.transform.position;
		health = maxHealth;
		controller = GetComponent<Controller2D> ();
		controller.HitHazard = HitHazard;
		controller.HitHazardSevere = HitHazardSevere;
		gravity = -2 * maxJumpHeight / (timeToJumpApex * timeToJumpApex);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		maxDoubleJumpVelocity = maxJumpVelocity * 0.75f;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);
	}

	// Update is called once per frame
	void Update () {
		SetRespawnPos ();
		iFrameTick ();
		stunTick ();
		CalculateVelocity ();
		HandleWallSliding ();

		controller.Move (velocity * Time.deltaTime, directionalInput);

		if (controller.collisions.above && velocity.y > 0){
			velocity.y = 0;
		}
		if (controller.collisions.below) {
			canDoubleJump = true;
			if (velocity.y < 0) {
				velocity.y = 0;
			}
		}
	}

	void SetRespawnPos(){
		respawnPosTime -= Time.deltaTime;

		if (respawnPosTime < 0 && controller.collisions.below) {
			//need to fix for moving platforms
			respawnPos = this.transform.position;
			respawnPosTime = respawnPosInterval;
		}
	}

	void HitHazard(Hazard hazard, Vector2 hitDir){
		controller.vulnerable = false;
		health -= hazard.damage;

		GetComponent<MeshRenderer> ().material = iframeMat;
		iframes = iframeTime;

		Vector2 knockbackDir = hitDir;
		knockbackDir.Normalize ();
		knockbackDir *= -1;
		controller.stunned = true;
		externalVelocity = knockbackDir * hazard.knockbackDist;
		externalVelocity.x *= 2;//increase horizontal knockback
		if (externalVelocity.y < 0)
			externalVelocity.y /= 4;//decrease pushdown if jumping up into hazard
		stunTime = hazard.knockbackTime;
		velocity = externalVelocity;
	}

	void HitHazardSevere(Hazard hazard, Vector2 hitDir){
		Debug.Log ("Hit Hazard Severe" + hazard);
		controller.vulnerable = false;
		health -= hazard.damage;

		GetComponent<MeshRenderer> ().material = iframeMat;
		iframes = respawnDuration;
		stunTime = respawnDuration;
		controller.stunned = true;
		respawnPos.y += 1.0f;  //This is in here cause it was respawning into the ground for some reason
		this.transform.position = respawnPos;
		controller.collisions.below = true;
		velocity = Vector3.zero;
	}

	public void SetDirectionalInput(Vector2 input){
		//probably have some stunned thing here
		directionalInput = input;
	}

	void stunTick(){
		if (!controller.stunned) {
			return;
		}

		stunTime-=Time.deltaTime;
		if (stunTime < 0) {
			stunTime = 0;
			controller.stunned = false;
			externalVelocity = Vector3.zero;
			//velocity = Vector3.zero;
		}
	}

	void iFrameTick(){
		if (controller.vulnerable) {
			return;
		}
		iframes-=Time.deltaTime;
		if (iframes < 0) {
			iframes = 0;
			controller.vulnerable = true;
			GetComponent<MeshRenderer> ().material = normalMat;
		}
	}

	public void OnJumpInputDown(){
		if (controller.stunned)
			return;
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
		//probably going to have to add knockback effects here

		if (facingRight && targetVelocityX < 0) {
			facingRight = false;
		} else if (!facingRight && targetVelocityX > 0) {
			facingRight = true;
		}

		//maybe have a list of external velocities and their durations
		targetVelocityX += externalVelocity.x;


		float curAcceleration = controller.collisions.below?accelerationTimeGrounded:accelerationTimeAirborne;
		if (controller.stunned) {
			targetVelocityX = 0;
		}

		//add another accelerator option for if you are being knocked by something
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing,
			curAcceleration);
		velocity.y += gravity * Time.deltaTime;
	}

	void HandleWallSliding(){
		wallDirX = (controller.collisions.left) ? -1 : 1;
		wallSliding = false;
		if ((controller.collisions.left || controller.collisions.right) &&
			!controller.collisions.below && velocity.y < 0 && !controller.stunned) {
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

	void OnDrawGizmos(){
		Gizmos.color = Color.red;
		float size = 0.3f;

		Gizmos.DrawLine (respawnPos - Vector3.left * size, respawnPos + Vector3.left * size);

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
