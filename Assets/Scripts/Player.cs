using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : LineRider, CameraFollowable
{
	private InputPackage lastInput;
	private Vector3 inputDirection;
	private SpriteRenderer spriteRenderer;

	// Physics
	private Controller2D controller;
	private float velocityXSmoothing;

	public float MaxJumpHeight;
	public float MinJumpHeight;
	public float TimeToJumpApex;
	private int availableJumpCount = 2;
	private bool jumpInProgress  = false;

	private bool dashAvailable = true;
	private bool dashing = false;
	private float dashStartTime = -1f;
	private Vector2 dashVelocity;

	private Vector2 lastTowerPassVelocity;
	private float lastTowerPassTime;

	private float gravity;
	private float maxJumpVelocity, minJumpVelocity;

	private float minSpeed = 8f;
	private float moveSpeed = 8f;
	private float maxSpeed = 30f;

	private Line disabledLine;

	// Camera Followable
	public Vector2 Velocity { get => velocity; }
	public Vector2 Position { get => transform.position; }

	protected override void Start() {
		base.Start();
		IsConnected = true;
		controller = GetComponent<Controller2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();

		gravity = -(2 * MaxJumpHeight) / Mathf.Pow(TimeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * TimeToJumpApex;
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * MinJumpHeight);
		gravity *= 1.1f;

		moveSpeed = minSpeed;
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

		if(p.Drop && !lastInput.Drop) {
			Disconnect(false);
			availableJumpCount--;
		}

		if(p.Dash && !lastInput.Dash) {
			StartDash();
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
		lastTowerPassTime = Time.time;
		lastTowerPassVelocity = LineVelocity;
		if (t.Lines.Count > 2 && inputDirection.sqrMagnitude > 0.1f) {
			// select the line that the player is pointing in the direction of
			foreach(Line l in t.Lines) {
				if(l != ConnectedLine && Mathf.Abs(Vector2.Dot(inputDirection, l.PhysicalCoordinatesDirection)) > 0.75f) {
					ConnectedLine = l;
					SetIndexAndDirection();
					return;
				}
			}
			base.SelectLine(t);
		}
		else {
			base.SelectLine(t);
		}
	}

	public override void FixedUpdate() {
		if(Active) {
			if(IsConnected) {
				if(ConnectedLine != null) {
					base.LineActions();
					if (LineSpeed > minSpeed) {
						LineSpeed -= LineSpeed * 0.1f * Time.fixedDeltaTime;
					}
				}
			}
			else {
				float speedLossModifier = 0.25f;

				// calculate velocity regardless, we want to use velocity to determine lineSpeed so this needs to constantly be updated with user direction
				CalculateVelocity(); 

				if (dashing) {
					controller.Move(dashVelocity * Time.fixedDeltaTime, inputDirection, out Vector2 vdiff);
					if (Time.time - dashStartTime > 0.2f) {
						dashing = false;
					}
					velocity.y = 0;
				}
				else {
					controller.Move(velocity * Time.fixedDeltaTime, inputDirection, out Vector2 vdiff);
					velocity += vdiff / Time.fixedDeltaTime;

					if (controller.collisions.below) {
						if (controller.collisions.slidingDownMaxSlope) {
							velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.fixedDeltaTime;
						}
						else {
							velocity.y = 0;
						}
						availableJumpCount = 2;
						dashAvailable = true;
						speedLossModifier = 1f;
					}
					else if (controller.collisions.above) {
						velocity.y = 0;
					}
				}
				
				if(moveSpeed > minSpeed) {
					moveSpeed -= moveSpeed * speedLossModifier * Time.fixedDeltaTime;
				}
				base.NonLineActions();
			}
		}
	}

	protected void Disconnect(bool up) {
		if(ConnectedLine != null) {
			Vector2 v = LineVelocity;
			// allow the player a brief amount of time (depending on velocity) after they've hit a tower
			// where they can jump with the direction of the last part of the line ( kind of like wile e coyote )
			if ((Time.time - lastTowerPassTime) * LineSpeed < 0.3f) {
				v = lastTowerPassVelocity;
			}
			velocity.x = v.magnitude * Vector3.Scale(v, new Vector3(2, 1, 0)).normalized.x;
			moveSpeed = Mathf.Clamp(Mathf.Abs(velocity.x), minSpeed, maxSpeed);
			if (up) {
				float scaledVy = v.y * 1.25f;
				velocity.y = scaledVy > maxJumpVelocity ? scaledVy : maxJumpVelocity;
			}
			else {
				velocity.y = -0.2f;
			}
			disabledLine = ConnectedLine;
			StartCoroutine(DisableConnectionForTime());
			Debug.Log($"Leaving line at {v} - movement velocity is {velocity}");
		}
		else if(up) {
			velocity.y = maxJumpVelocity;
		}
		base.Disconnect();
	}

	protected override void Connect(RaycastHit2D hit) {
		if(!lastInput.Drop && hit.collider.gameObject != disabledLine?.gameObject) {
			base.Connect(hit);
			var d = ConnectedLine.Positions[index].Direction;

			float dot = Vector2.Dot(velocity.normalized, d.normalized);
			float modifier = Mathf.Abs(dot) > 0.8f ? 1.25f : 1.0f;

			LineSpeed = Mathf.Clamp(Mathf.Abs(velocity.x) * modifier + Mathf.Abs(velocity.y) * 0.25f, minSpeed, maxSpeed);
			Debug.Log($"Joining line at {velocity} - line velocity is {LineSpeed} - dot was {dot}");

			availableJumpCount = 2;
			dashAvailable = true;
			dashing = false;
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
		if(availableJumpCount > 0) {
			if(IsConnected) {
				Disconnect(true);
			}
			else {
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
			availableJumpCount--;
			jumpInProgress = true;
		}
		
	}

	public void OnJumpInputUp() {
		if (jumpInProgress && velocity.y > minJumpVelocity) {
			velocity.y = minJumpVelocity;
		}
		jumpInProgress = false;
	}

	private void StartDash() {
		if(dashAvailable) {
			Disconnect(false);
			dashVelocity = Mathf.Max(velocity.x, minSpeed) * 2f * Mathf.Sign(inputDirection.x) * Vector2.right;
			dashAvailable = false;
			dashing = true;
			dashStartTime = Time.time;
			availableJumpCount = 1;
		}
	}

	void CalculateVelocity() {
		float targetVelocityX = inputDirection.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? .1f : .2f);
		velocity.y += gravity * Time.deltaTime;	
	}

	protected IEnumerator DisableConnectionForTime() {
		yield return new WaitForSeconds(0.5f);
		disabledLine = null;
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
}
