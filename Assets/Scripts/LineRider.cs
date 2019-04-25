using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRider : MonoBehaviour
{
	public bool Active { get; set; }
    public bool Connected { get; set; }
	public Line CurrentLine { get; private set; }

	protected int index;
	protected int direction;

	protected float MaxLineSpeed;
	protected float LineSpeed = 2f;

	protected virtual void Start() {

	}

	public virtual void SelectLine(Tower t) {	
		if(t.Lines.Count == 0) {
			return;
		}
		else if(t.Lines.Count == 1) {
			CurrentLine = t.Lines[0];
		}
		else {
			// select a new line
			int i = (t.Lines.IndexOf(CurrentLine) + 1) % t.Lines.Count;
			CurrentLine = t.Lines[i];
		}
		SetIndexAndDirection();
	}

	public void SetIndexAndDirection() {
		if(CurrentLine) {
			// turn around on line
			(index, direction) = CurrentLine.GetNearestIndexAndDirection(transform.position, true);
		}
		else {
			// go same direction as current velocity
			// (index, direction) = CurrentLine.GetNearestIndexAndDirection(transform.position, false, velocity);
		}
	}

	public virtual void FixedUpdate() {
		if(Connected) {
			if(CurrentLine != null) {
				LineMovement();
			}
		}
		else {

		}
	}

	protected void LineMovement() {
		float movement = LineSpeed * Time.fixedDeltaTime;
		while (movement > 0) {
			Vector3 diff = (CurrentLine.Positions[index + direction] - transform.position);
			diff.Scale(new Vector3(1, 1, 0));
			if (movement >= diff.magnitude) {
				transform.position += diff;
				movement -= diff.magnitude;
				index += direction;
				if (index == 0 || index == CurrentLine.Positions.Count - 1) {
					Tower tower = CurrentLine.GetClosestTower(transform.position);
					SelectLine(tower);
				}
			}
			else {
				transform.position += diff.normalized * movement;
				movement = 0;
			}
		}
	}
}
