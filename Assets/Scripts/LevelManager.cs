using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent (typeof(InputManager))]
public class LevelManager : ContextManager
{
	public Player player;

	private HexGrid _hexGrid;
	public HexGrid Grid {
		get => _hexGrid;
		set {
			_hexGrid = value;
			SetCameraPosition(Grid.StartingPoint.PhysicalCoordinates);
		}
	}

	public GameObject TowerPrefab;
	private GameObject Tower;

	private Camera main;
	private InputPackage lastInput;

	private float maxCameraSize = 15f;
	private float minCameraSize = 5f;
	private float cameraSizeDiff { get => maxCameraSize - minCameraSize; }

	public override void Awake() {
		base.Awake();
		main = Camera.main;
	}

	public override void Start()
    {
        // player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
		Tower = GameObject.Instantiate(TowerPrefab);
    }

	private void SetCameraPosition(Vector3 position) {
		Bounds clamp = Grid.PaddedGridDimensions;

		float x = position.x;
		float y = position.y;
		// left side of camera further right than clamp
		if(clamp.max.x < x) {
			x = clamp.max.x ;
		}
		else if(clamp.min.x > x) {
			x = clamp.min.x ;
		}

		// bottom side of camera further up than clamp
		if (clamp.max.y < y) {
			y = clamp.max.y ;
		}
		else if(clamp.min.y > y) {
			y = clamp.min.y ;
		}

		main.transform.position = new Vector3(x, y, -10);
	}

	public override void HandleInput(InputPackage p) {
		if(Mathf.Abs(p.MouseWheelDelta) > 0.2f) {
			float camSize = main.orthographicSize - p.MouseWheelDelta;
			float prevCamSize = main.orthographicSize;
			main.orthographicSize = Mathf.Clamp(camSize, 5f, 15f);

			// mouse pointer world space should not change when zooming
			Vector3 pointerPositionAfterCamResize = main.ScreenToWorldPoint(p.MousePositionScreenSpace);
			SetCameraPosition(main.transform.position + (p.MousePositionWorldSpace - pointerPositionAfterCamResize));
		}

		// x,y is position
		// z is rotation
		Vector3 posRot = Grid.TryGetTowerPlacement(p.MousePositionWorldSpace, out bool validTowerPosition, Tower.transform.lossyScale.y/4f);
		if(validTowerPosition) {
			Tower.SetActive(true);
			Tower.transform.position = new Vector3(posRot.x, posRot.y, -0.5f);	
			Tower.transform.rotation = Quaternion.Euler(0,0,posRot.z);
		}
		else {
			Tower.SetActive(false);
		}

		if((lastInput == null || !lastInput.LeftMouse) && p.LeftMouse) {
			// place
		}
		else if(lastInput != null && lastInput.LeftMouse && p.LeftMouse) {
			// dragging camera
			var diff = (p.MousePositionScreenSpace - lastInput.MousePositionScreenSpace) * Time.deltaTime * 
				Mathf.Lerp(0.5f, 1.4f, (main.orthographicSize - minCameraSize) / cameraSizeDiff) * Settings.ScrollSpeed;
			Vector3 newPosition = main.transform.position - diff;
			SetCameraPosition(newPosition);
		}

		if((lastInput == null || !lastInput.RightMouse) && p.RightMouse) {
			// switch tower type

		}

		lastInput = p;
	}
}
