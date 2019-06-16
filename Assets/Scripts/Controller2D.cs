using UnityEngine;
using System.Collections;

public class Controller2D : RaycastController {

	public float maxSlopeAngle = 80;

	public CollisionInfo collisions;
	[HideInInspector]
	public Vector2 playerInput;

	public override void Start() {
		base.Start();
		collisions.faceDir = 1;

	}

	public void Move(Vector2 moveAmount, out Vector2 vDiff, bool standingOnPlatform) {
		Move(moveAmount, Vector2.zero, out vDiff, standingOnPlatform);
	}

	public void Move(Vector2 moveAmount, Vector2 input, out Vector2 vDiff, bool standingOnPlatform = false) {
		UpdateRaycastOrigins();

		collisions.Reset();
		collisions.moveAmountOld = moveAmount;
		playerInput = input;

		//if (moveAmount.y < 0) {
		//	DescendSlope(ref moveAmount);
		//}

		if (moveAmount.x != 0) {
			collisions.faceDir = (int)Mathf.Sign(moveAmount.x);
		}

		Vector2 moveAmountCopy = new Vector2(moveAmount.x, moveAmount.y);

		HorizontalCollisions(ref moveAmount);
		if (moveAmount.y != 0) {
			VerticalCollisions(ref moveAmount);
		}

		collisions.ridingSlopedWall = OverhangMovement(ref moveAmountCopy);
		if (collisions.ridingSlopedWall) {
			moveAmount = moveAmountCopy;
		}

		transform.Translate(moveAmount);

		if (standingOnPlatform) {
			collisions.below = true;
		}

		float vDiffx = 0;
		float vDiffy = collisions.climbingSlopeOld && !collisions.climbingSlope
			? Mathf.Abs(collisions.moveAmountOld.x) * Mathf.Cos(collisions.slopeAngleOld * Mathf.Deg2Rad)
			: 0;

		vDiff = new Vector2(vDiffx, vDiffy);
	}

	void HorizontalCollisions(ref Vector2 moveAmount) {
		float directionX = collisions.faceDir;
		float rayLength = Mathf.Abs(moveAmount.x) + skinWidth;

		if (Mathf.Abs(moveAmount.x) < skinWidth) {
			rayLength = 2 * skinWidth;
		}

		for (int i = horizontalRayCount - 1; i >= 0; i--) {
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

			if (hit) {

				if (hit.distance == 0 || hit.distance > Mathf.Abs(moveAmount.x)) {
					continue;
				}

				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (i == 0 && slopeAngle <= maxSlopeAngle) {
					if (collisions.descendingSlope) {
						collisions.descendingSlope = false;
						moveAmount = collisions.moveAmountOld;
					}
					float distanceToSlopeStart = 0;
					if (slopeAngle != collisions.slopeAngleOld) {
						distanceToSlopeStart = hit.distance - skinWidth;
						moveAmount.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
					moveAmount.x += distanceToSlopeStart * directionX;
				}

				if ((!collisions.climbingSlope || (slopeAngle > maxSlopeAngle))) {
					moveAmount.x = (hit.distance - skinWidth) * directionX;
					rayLength = hit.distance;

					if (collisions.climbingSlope) {
						moveAmount.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
					}

					collisions.left = directionX == -1;
					collisions.right = directionX == 1;
				}
			}
		}
	}

	void VerticalCollisions(ref Vector2 moveAmount) {
		float directionY = Mathf.Sign(moveAmount.y);
		float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;

		for (int i = 0; i < verticalRayCount; i++) {

			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

			if (hit) {
				if (hit.collider.tag == "Through") {
					if (directionY == 1 || hit.distance == 0) {
						continue;
					}
					if (collisions.fallingThroughPlatform) {
						continue;
					}
					if (playerInput.y == -1) {
						collisions.fallingThroughPlatform = true;
						Invoke("ResetFallingThroughPlatform", .5f);
						continue;
					}
				}

				moveAmount.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;

				if (collisions.climbingSlope) {
					moveAmount.x = moveAmount.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);
				}

				collisions.below = directionY == -1;
				collisions.above = directionY == 1;
			}
		}

		if (collisions.climbingSlope) {
			float directionX = Mathf.Sign(moveAmount.x);
			rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * moveAmount.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if (slopeAngle != collisions.slopeAngle) {
					moveAmount.x = (hit.distance - skinWidth) * directionX;
					collisions.slopeAngle = slopeAngle;
					collisions.slopeNormal = hit.normal;
				}
			}
		}
	}

	void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal) {
		float moveDistance = Mathf.Abs(moveAmount.x);
		float climbmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (moveAmount.y <= climbmoveAmountY) {
			moveAmount.y = climbmoveAmountY;
			moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
			collisions.slopeNormal = slopeNormal;
		}
	}

	void DescendSlope(ref Vector2 moveAmount) {
		RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, collisionMask);
		RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, collisionMask);
		if (maxSlopeHitLeft ^ maxSlopeHitRight) {
			SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
			SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);
		}

		if (!collisions.slidingDownMaxSlope) {
			float directionX = Mathf.Sign(moveAmount.x);
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle) {
					if (Mathf.Sign(hit.normal.x) == directionX) {
						if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x)) {
							float moveDistance = Mathf.Abs(moveAmount.x);
							float descendmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
							moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
							moveAmount.y -= descendmoveAmountY;

							collisions.slopeAngle = slopeAngle;
							collisions.descendingSlope = true;
							collisions.below = true;
							collisions.slopeNormal = hit.normal;
						}
					}
				}
			}
		}
	}

	void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount) {

		if (hit) {
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
			if (slopeAngle > maxSlopeAngle && slopeAngle < 90) {
				moveAmount.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs(moveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

				collisions.slopeAngle = slopeAngle;
				collisions.slidingDownMaxSlope = true;
				collisions.slopeNormal = hit.normal;
			}
		}

	}

	private bool OverhangMovement(ref Vector2 moveAmount) {
		float directionX = Mathf.Sign(moveAmount.x);

		float xMovement = Mathf.Abs(moveAmount.x);

		if(!collisions.below) {
			// fire ray in top right (going up), top left (going up), and top left/right (going left/right depending on direction)
			if(moveAmount.y > xMovement) {
				float yRayLength = Mathf.Abs(moveAmount.y) + skinWidth;
				RaycastHit2D topLeftUp = Physics2D.Raycast(raycastOrigins.topLeft, Vector2.up, yRayLength, collisionMask);
				RaycastHit2D topRightUp = Physics2D.Raycast(raycastOrigins.topRight, Vector2.up, yRayLength, collisionMask);

				if (topLeftUp.collider != null ^ topRightUp.collider != null) {
					RaycastHit2D hit;
					float xdir;
					if (topLeftUp.collider != null) {
						hit = topLeftUp;
						xdir = 1;
					}
					else {
						hit = topRightUp;
						xdir = -1;
					}
					float slopeAngle = Vector2.Angle(hit.normal, Vector2.down);

					float distanceToSlopeStart = hit.distance - skinWidth;
					float slopeMoveAmount = moveAmount.y - distanceToSlopeStart;

					float slopeMoveAmountY =  Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * slopeMoveAmount;
					float slopeMoveAmountX = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * xdir * slopeMoveAmount;

					moveAmount.y = distanceToSlopeStart + slopeMoveAmountY;
					if(slopeMoveAmountX * moveAmount.x > 0) {
						moveAmount.x = slopeMoveAmountX > 0 ? Mathf.Max(slopeMoveAmountX, moveAmount.x) : Mathf.Min(slopeMoveAmountX, moveAmount.x);
					}
					else {
						moveAmount.x = slopeMoveAmountX;
					}
					// Debug.Log($"Overhung Up {(topLeftUp == hit ? "Left" : "Right")} ({moveAmount.x}, {moveAmount.y})");
					return true;
				}
			}
			else {
				RaycastHit2D hit;
				RaycastHit2D bottomHit;
				if(directionX > 0) {
					hit = Physics2D.Raycast(raycastOrigins.topRight, Vector2.right, xMovement + skinWidth, collisionMask);
					bottomHit = Physics2D.Raycast(raycastOrigins.bottomRight, Vector2.right, xMovement + skinWidth, collisionMask);
				}
				else {
					hit = Physics2D.Raycast(raycastOrigins.topLeft, Vector2.left, xMovement + skinWidth, collisionMask);
					bottomHit = Physics2D.Raycast(raycastOrigins.bottomLeft, Vector2.left, xMovement + skinWidth, collisionMask);
				}

				if(hit.collider != null) {
					float slopeAngle = Vector2.Angle(hit.normal, Vector2.down);

					float d = hit.distance;
					if(bottomHit.collider != null) {
						xMovement = bottomHit.distance - skinWidth;
					}
					float distanceToSlopeStart = hit.distance - skinWidth;
					float slopeMoveAmount = xMovement - distanceToSlopeStart;

					float slopeMoveAmountY = -Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * slopeMoveAmount;
					float slopeMoveAmountX = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * slopeMoveAmount;

					moveAmount.x = (distanceToSlopeStart + slopeMoveAmountX) * directionX;
					moveAmount.y = Mathf.Min(slopeMoveAmountY, moveAmount.y);
					// Debug.Log($"Overhung {(directionX > 0 ? "Right" : "Left")} ({moveAmount.x}, {moveAmount.y})");
					return true;
				}
			}
		}
		return false;
	}

	void ResetFallingThroughPlatform() {
		collisions.fallingThroughPlatform = false;
	}

	public struct CollisionInfo {
		public bool above, below;
		public bool left, right;

		public bool climbingSlopeOld;
		public bool climbingSlope;
		public bool descendingSlope;
		public bool slidingDownMaxSlope;

		public bool ridingSlopedWall;

		public float slopeAngle, slopeAngleOld;
		public Vector2 slopeNormal;
		public Vector2 moveAmountOld;
		public int faceDir;
		public bool fallingThroughPlatform;

		public void Reset() {
			above = below = false;
			left = right = false;
			descendingSlope = false;
			slidingDownMaxSlope = false;
			slopeNormal = Vector2.zero;

			climbingSlopeOld = climbingSlope;
			climbingSlope = false;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}

}