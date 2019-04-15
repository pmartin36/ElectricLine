using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
	private Camera main;
	private LayerMask groundMask;

	private void Start() {
		main = Camera.main;
	}

	private void Update() {
		InputPackage p = new InputPackage();
		p.MousePositionScreenSpace = Input.mousePosition;
		p.MousePositionWorldSpace = (Vector2)main.ScreenToWorldPoint(Input.mousePosition);

		p.MouseWheelDelta = Input.mouseScrollDelta.y;
		p.LeftMouse = Input.GetButton("LeftMouse");
		p.RightMouse = Input.GetButton("RightMouse");
		GameManager.Instance.ContextManager.HandleInput(p);
	}

}

public class InputPackage {
	public Vector3 MousePositionScreenSpace { get; set; }
	public Vector3 MousePositionWorldSpace { get; set; }
	public float MouseWheelDelta { get; set; }
	public bool LeftMouse { get; set; }
	public bool RightMouse { get; set; }
}
