using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRider : MonoBehaviour
{
	public bool Active { get; set; }
    public bool IsConnected { get; set; }
	public Line ConnectedLine { get; protected set; }

	protected int index;
	protected int direction;

	protected float MaxLineSpeed;
	protected float LineSpeed = 2f;
	protected bool canConnect;

	protected Vector3 LineVelocity;
	protected LayerMask LinesLayerMask;

	protected Vector2 velocity;

	protected virtual void Start() {
		LinesLayerMask = 1 << LayerMask.NameToLayer("Lines");
		canConnect = true;
	}

	public virtual void SelectLine(Tower t) {	
		if(t.Lines.Count == 0) {
			return;
		}
		else if(t.Lines.Count == 1) {
			ConnectedLine = t.Lines[0];
		}
		else {
			// select a new line
			int i = (t.Lines.IndexOf(ConnectedLine) + 1) % t.Lines.Count;
			ConnectedLine = t.Lines[i];
		}
		SetIndexAndDirection();
	}

	public void SetIndexAndDirection() {
		if(IsConnected) {
			// turn around on line
			(index, direction) = ConnectedLine.GetNearestIndexAndDirection(transform.position, true);
		}
		else {
			// go same direction as current velocity
			(index, direction) = ConnectedLine.GetNearestIndexAndDirection(transform.position, false, velocity);
		}
	}

	public virtual void FixedUpdate() {
		if(IsConnected) {
			if(ConnectedLine != null) {
				LineActions();
			}
		}
		else {
			NonLineActions();
		}
	}

	protected void NonLineActions() {
		if (canConnect) {
			RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.1f, velocity.normalized, velocity.magnitude * Time.deltaTime, LinesLayerMask);
			if(hit) {
				Connect(hit);
			}
		}
	}

	protected void LineActions() {
		float movement = LineSpeed * Time.fixedDeltaTime;

		// get towards center of line
		var next = ConnectedLine.Positions[index + direction];
		Vector3 diff = (next.Position - transform.position);
		var p = Vector3.Project(diff, next.Normal);	
		transform.position += Time.fixedDeltaTime * p * 5f;

		while (movement > 0) {
			diff = (ConnectedLine.Positions[index + direction].Position - transform.position);
			diff.Scale(new Vector3(1, 1, 0));
			if (movement >= diff.magnitude) {
				transform.position += diff;
				movement -= diff.magnitude;
				index += direction;
				if (index == 0 || index == ConnectedLine.Positions.Count - 1) {
					Tower tower = ConnectedLine.GetClosestTower(transform.position);
					SelectLine(tower);
				}
			}
			else {
				transform.position += diff.normalized * movement;
				movement = 0;
			}
		}

		LineVelocity = diff.normalized * LineSpeed;
	}

	protected virtual void Connect(RaycastHit2D hit) {
		ConnectedLine = hit.collider.GetComponent<Line>();
		transform.position = hit.centroid;
		SetIndexAndDirection();		
		IsConnected = true;	
	}

	protected virtual void Disconnect() {
		IsConnected = false;
		ConnectedLine = null;
	}
}
