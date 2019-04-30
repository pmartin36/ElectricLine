using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : LineRider, CameraFollowable
{
	private InputPackage lastInput;
	private Vector3 inputDirection;
	private SpriteRenderer spriteRenderer;

	public bool ConnectionDisabled { get; set; }
	Coroutine DisableConnectionForTime;

	// Physics
	private Controller2D controller;
	private Vector3 velocity;
	private float velocityXSmoothing;

	public float MaxJumpHeight = 1.5f;
	public float MinJumpHeight = 0.5f;
	public float TimeToJumpApex = 0.4f;

	private float gravity;
	private float maxJumpVelocity, minJumpVelocity;
	private float moveSpeed = 6f; // static for now

	// Camera Followable
	public Vector2 Velocity { get => velocity; }
	public Vector2 Position { get => transform.position; }

	protected override void Start() {
		Connected = true;
		controller = GetComponent<Controller2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();

		gravity = -(2 * MaxJumpHeight) / Mathf.Pow(TimeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * TimeToJumpApex;
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * MinJumpHeight);

		lastInput = new InputPackage();
	}

	public void Init(Tower startTower) {
		
	}

	public void SetActive(Tower startTower) {
		Active = true;
		spriteRenderer.color = Color.yellow;
		SelectLine(startTower);
	}

	public void HandleInput(InputPackage p) {
		inputDirection = new Vector3(p.Horizontal, p.Vertical);

		if(p.Drop) {
			ConnectionDisabled = true;
			if(Connected) {
				Connected = false;
				velocity = new Vector3(LineVelocity.x, 0);
			}
		}

		if (p.Jump && !lastInput.Jump) {
			OnJumpInputDown();
		}
		else if (!p.Jump && lastInput.Jump) {
			OnJumpInputUp();
		}

		lastInput = p;
	}

	public override void SelectLine(Tower t) {
		if (t.Lines.Count > 1 && inputDirection.sqrMagnitude > 0.1f) {
			// select the line that the player is pointing in the direction of

			SetIndexAndDirection();
		}
		else {
			base.SelectLine(t);
		}
	}

	public override void FixedUpdate() {
		if(Active) {
			if(Connected) {
				if(CurrentLine != null) {
					base.LineMovement();
				}
			}
			else {
				CalculateVelocity();
				controller.Move(velocity * Time.deltaTime, inputDirection);
				if (controller.collisions.above || controller.collisions.below) {
					if (controller.collisions.slidingDownMaxSlope) {
						velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.fixedDeltaTime;
					}
					else {
						velocity.y = 0;
					}
				}
			}
		}
	}

	public void OnJumpInputDown() {
		//if (wallSliding) {
		//	if (wallDirX == inputDirection.x) {
		//		velocity.x = -wallDirX * wallJumpClimb.x;
		//		velocity.y = wallJumpClimb.y;
		//	}
		//	else if (inputDirection.x == 0) {
		//		velocity.x = -wallDirX * wallJumpOff.x;
		//		velocity.y = wallJumpOff.y;
		//	}
		//	else {
		//		velocity.x = -wallDirX * wallLeap.x;
		//		velocity.y = wallLeap.y;
		//	}
		//}
		if (controller.collisions.below) {
			if (controller.collisions.slidingDownMaxSlope) {
				if (inputDirection.x != -Mathf.Sign(controller.collisions.slopeNormal.x)) { // not jumping against max slope
					velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
					velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
				}
			}
			else {
				velocity.y = maxJumpVelocity;
			}
		}
	}

	public void OnJumpInputUp() {
		if (velocity.y > minJumpVelocity) {
			velocity.y = minJumpVelocity;
		}
	}

	//void HandleWallSliding() {
	//	wallDirX = (controller.collisions.left) ? -1 : 1;
	//	wallSliding = false;
	//	if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
	//		wallSliding = true;

	//		if (velocity.y < -wallSlideSpeedMax) {
	//			velocity.y = -wallSlideSpeedMax;
	//		}

	//		if (timeToWallUnstick > 0) {
	//			velocityXSmoothing = 0;
	//			velocity.x = 0;

	//			if (directionalInput.x != wallDirX && directionalInput.x != 0) {
	//				timeToWallUnstick -= Time.deltaTime;
	//			}
	//			else {
	//				timeToWallUnstick = wallStickTime;
	//			}
	//		}
	//		else {
	//			timeToWallUnstick = wallStickTime;
	//		}

	//	}

	//}

	void CalculateVelocity() {
		float targetVelocityX = inputDirection.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? .1f : .2f);
		velocity.y += gravity * Time.deltaTime;
	}
}
