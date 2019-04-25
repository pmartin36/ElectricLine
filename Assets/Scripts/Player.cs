using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : LineRider
{
	private Vector3 inputDirection;
	private SpriteRenderer spriteRenderer;

	protected override void Start() {
		Connected = true;
		spriteRenderer = GetComponent<SpriteRenderer>();
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
				base.LineMovement();
			}
			else {

			}
		}
	}
}
