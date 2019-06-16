using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : LineRider, CameraFollowable
{
	private bool acceptingInputs = true;
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
	private float dashTime;

	private Vector2 lastTowerPassVelocity;
	private float lastTowerPassTime;

	private float gravity;
	private float maxJumpVelocity, minJumpVelocity;

	private float minSpeed = 8f;
	private float moveSpeed = 8f;
	private float maxSpeed = 30f;

	//
	private float maxEnergy;
	private float energy;
	public bool HasEnergy { get; protected set; }

	private Line disabledLine;
	private Tower connectedTower;

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

	public void Init(Tower startTower, PlayerData data) {
		energy = maxEnergy = data.MaxEnergy;
		MaxJumpHeight = data.JumpHeight;
		dashTime = data.DashTime;
	}

	private void ResetDashAndJump() {
		dashAvailable = true;
		availableJumpCount = 2;
	}

	public void SetActive(Tower startTower) {
		Active = true;
		spriteRenderer.color = Color.yellow;
		SelectLine(startTower);
	}

	public void HandleInput(InputPackage p) {
		if(!acceptingInputs) {
			inputDirection = Vector2.zero;
			OnJumpInputUp();
			return;
		} 

		if(!HasEnergy) {
			// TODO: Player should continue falling in the same direction they were going when they had energy
			// and level should fail when they hit the ground
			OnJumpInputUp();
			return;
		}

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
		else if(t.Lines.Count == 0) {
			connectedTower = t;
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
				else if(connectedTower != null) {
					Vector2 dist = connectedTower.PhysicalHeadPosition - (Vector2)transform.position; // throw out z
					float ms = moveSpeed * Time.fixedDeltaTime;
					if(dist.magnitude > ms) {
						transform.position += (Vector3)dist.normalized * ms;
					}
					else {
						transform.position += (Vector3)dist;
					}
					if (moveSpeed > minSpeed) {
						moveSpeed -= ms;
					}
				}
				SetEnergy(Time.fixedDeltaTime * 5f);
			}
			else {
				float speedLossModifier = 0.25f;

				// calculate velocity regardless, we want to use velocity to determine lineSpeed so this needs to constantly be updated with user direction
				CalculateVelocity(); 

				if (dashing) {
					controller.Move(dashVelocity * Time.fixedDeltaTime, inputDirection, out Vector2 vdiff);
					if (Time.time - dashStartTime > dashTime) {
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
						ResetDashAndJump();
						speedLossModifier = 1f;
					}
					else if (controller.collisions.above && !controller.collisions.ridingSlopedWall) {
						velocity.y = 0;
					}
				}
				
				if(moveSpeed > minSpeed) {
					moveSpeed -= moveSpeed * speedLossModifier * Time.fixedDeltaTime;
				}

				SetEnergy(-Time.fixedDeltaTime / 2f);
				base.NonLineActions();
			}
		}
	}

	void Update() {
		GameManager.Instance.LevelManager.UpdateUIFromPlayer(energy);
	}

	private void SetEnergy(float delta) {
		energy = Mathf.Clamp(energy + delta, 0, maxEnergy);
		if(energy > 0) {
			HasEnergy = true;
		}
		else {
			HasEnergy = false;
			if(controller.collisions.below) {
				// TODO, this should actually be a loss but we're doing a switch level to fake death
				Active = false;
				inputDirection = Vector2.zero;
				GameManager.Instance.LevelManager.SwitchLevel(1);
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
			Invoke("RemoveDisabledLine", 0.5f);
			Debug.Log($"Leaving line at {v} - movement velocity is {velocity}");
		}
		else if(connectedTower != null) {
			velocity.x = lastInput.Horizontal * moveSpeed;
			velocity.y = up ? maxJumpVelocity : 0;			
			connectedTower = null;
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

			ResetDashAndJump();
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
				// TODO: Wile E Coyote
				availableJumpCount =  1;
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
				// TODO: Wile E Coyote
				availableJumpCount = controller.collisions.below ? 1 : 0;
			}
			
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
		}
	}

	void CalculateVelocity() {
		float targetVelocityX = inputDirection.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? .1f : .2f);
		velocity.y += gravity * Time.fixedDeltaTime;	
	}

	protected void RemoveDisabledLine() {
		disabledLine = null;
	}

	private void OnTriggerEnter2D(Collider2D collider) {
		if(collider.CompareTag("Tower")) {
			// the head of tower is a child of the actual tower
			Tower t = collider.transform?.parent.GetComponent<Tower>() ?? collider.transform.GetComponent<Tower>();
			if(t.Lines.Count == 0) {
				ResetDashAndJump();
				IsConnected = true;
				connectedTower = t;			
			}
			t.Touched();
		}
		else if(collider.CompareTag("Gate")) {
			EndGate g = collider.GetComponent<EndGate>();
			if(g.IsOpen) {
				acceptingInputs = false;

				// next level
				GameManager.Instance.LevelManager.SwitchLevel(1);
			}
		}
	}

	void OnDestroy() {
		Debug.Log("Destroyed");
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
