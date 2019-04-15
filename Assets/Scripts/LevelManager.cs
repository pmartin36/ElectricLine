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

	public Tower TowerPrefab;
	private Tower Tower;

	public Line LinePrefab;
	private Line Line;

	private Camera main;
	private InputPackage lastInput;

	private float maxCameraSize = 11f;
	private float minCameraSize = 5f;
	private float cameraSizeDiff { get => maxCameraSize - minCameraSize; }

	private bool dragging = false;
	private bool lineCreationInProgress = false;

	public override void Awake() {
		base.Awake();
		main = Camera.main;
	}

	public override void Start()
    {
        // player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
		Tower = GameObject.Instantiate<Tower>(TowerPrefab);
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
			main.orthographicSize = Mathf.Clamp(camSize, minCameraSize, maxCameraSize);

			// mouse pointer world space should not change when zooming
			Vector3 pointerPositionAfterCamResize = main.ScreenToWorldPoint(p.MousePositionScreenSpace);
			SetCameraPosition(main.transform.position + (p.MousePositionWorldSpace - pointerPositionAfterCamResize));
		}

		if(!lineCreationInProgress) {
			Grid.TryGetTowerLocation(p.MousePositionWorldSpace, Tower);
		}

		// mouseup
		if ((lastInput != null && lastInput.LeftMouse) && !p.LeftMouse) {
			// place
			if(!dragging && Tower.gameObject.activeInHierarchy) {
				Tower.Place();
				Grid.PlaceTower(Tower);

				Tower = GameObject.Instantiate<Tower>(TowerPrefab);
			}

			dragging = false;
		}
		// mousehold
		else if(lastInput != null && lastInput.LeftMouse && p.LeftMouse) {
			// dragging camera
			var diff = (p.MousePositionScreenSpace - lastInput.MousePositionScreenSpace) * Time.deltaTime * 
				Mathf.Lerp(0.5f, 1.4f, (main.orthographicSize - minCameraSize) / cameraSizeDiff) * Settings.ScrollSpeed;
			Vector3 newPosition = main.transform.position - diff;
			SetCameraPosition(newPosition);

			if(diff.sqrMagnitude > 0.01f) {
				dragging = true;
			}
		}

		if((lastInput == null || !lastInput.RightMouse) && p.RightMouse) {
			HexInfo h = Grid.TryGetCellInfoFromWorldPosition(p.MousePositionWorldSpace, out bool success);
			if (success) {
				if (h.TowerHead != null) {
					lineCreationInProgress = true;
					Tower.gameObject.SetActive(false);
					Line = Instantiate(LinePrefab, h.PhysicalCoordinates, Quaternion.identity);
					Line.gameObject.SetActive(true);
					Line.Init(Grid, p.MousePositionWorldSpace);
				}
			}		
		}
		else if(p.RightMouse && lineCreationInProgress) {
			// Update line with position
			Line.Update(p.MousePositionWorldSpace);
		}
		else if(!p.RightMouse && lineCreationInProgress) {
			// verify final spot is actually tower head
			// if so, create line
			bool success = Line.IsValidPlacement(p.MousePositionWorldSpace);
			if(success) {
				Line.Place();
			}
			else {
				Destroy(Line.gameObject);
			}
			lineCreationInProgress = false;
			Tower.gameObject.SetActive(true);
		}

		lastInput = p;
	}
}
