using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent (typeof(InputManager))]
public class LevelManager : ContextManager
{
	private HexGrid _hexGrid;
	public HexGrid Grid {
		get => _hexGrid;
		set {
			_hexGrid = value;
		}
	}

	public LevelData LevelData;

	public bool InPlacementMode = true;
	public event EventHandler<bool> PlacementModeChange;

	// For Placement Mode
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

	// For Play Mode
	public Player playerPrefab;
	public Player Player { get; set; }
	public Tower StartTower { get => Grid.StartingPoint.TowerHead; }

	public override void Awake() {
		base.Awake();
		main = Camera.main;
		HexGrid.GridGenerated += GridGenerated;

		Grid = GameObject.FindObjectOfType<HexGrid>();
		if(LevelData != null) {
			Grid.Init(LevelData, TowerPrefab, LinePrefab);
		}
		else {
			Grid.Init();
		}
		
		SetCameraPosition(Grid.StartingPoint.PhysicalCoordinates);
	}

	public override void Start()
    {
		Tower = GameObject.Instantiate<Tower>(TowerPrefab);
    }

	public void GridGenerated(object sender, HexGrid grid) {
		this.Grid = grid;
		// start the game
		Player = Instantiate(playerPrefab, grid.StartingPoint.PhysicalCoordinates, Quaternion.identity);
		// Player.Init(grid.StartingPoint.TowerHead);
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

	private void LeavePlacementMode() {
		InPlacementMode = false;
		GameObject.Destroy(Tower.gameObject);
		PlacementModeChange?.Invoke(this, InPlacementMode);
		Player.SetActive(Grid.StartingPoint.TowerHead);
	}

	public override void HandleInput(InputPackage p) {
		// Common Input

		if(InPlacementMode) {
			if (p.Enter) {
				LeavePlacementMode();
			}
			else {
				PlacementModeInput(p);
			}
		}
		else {
			PlayModeInput(p);
		}	
	}

	public void PlayModeInput(InputPackage p) {
		Player.HandleInput(p);
	}

	public void PlacementModeInput(InputPackage p) {
		if (Mathf.Abs(p.MouseWheelDelta) > 0.2f) {
			float camSize = main.orthographicSize - p.MouseWheelDelta;
			float prevCamSize = main.orthographicSize;
			main.orthographicSize = Mathf.Clamp(camSize, minCameraSize, maxCameraSize);

			// mouse pointer world space should not change when zooming
			Vector3 pointerPositionAfterCamResize = main.ScreenToWorldPoint(p.MousePositionScreenSpace);
			SetCameraPosition(main.transform.position + (p.MousePositionWorldSpace - pointerPositionAfterCamResize));
		}

		Vector2 vpSpace = main.WorldToViewportPoint(p.MousePositionWorldSpace);
		bool mouseOnScreen = vpSpace.x < 1 && vpSpace.x >= 0 && vpSpace.y < 1 && vpSpace.y >= 0;

		if (!lineCreationInProgress) {
			Grid.TryGetTowerLocation(p.MousePositionWorldSpace, Tower);
		}

		// mouseup
		if ((lastInput != null && lastInput.LeftMouse) && !p.LeftMouse) {
			// place
			if (!dragging && mouseOnScreen && Tower.gameObject.activeInHierarchy) {
				Tower.Place();
				Grid.PlaceTower(Tower);

				Tower = GameObject.Instantiate<Tower>(TowerPrefab);
			}

			dragging = false;
		}
		// mousehold
		else if (lastInput != null && lastInput.LeftMouse && p.LeftMouse) {
			// dragging camera
			var diff = (p.MousePositionScreenSpace - lastInput.MousePositionScreenSpace) * Time.deltaTime *
				Mathf.Lerp(0.5f, 1.4f, (main.orthographicSize - minCameraSize) / cameraSizeDiff) * Settings.ScrollSpeed;
			Vector3 newPosition = main.transform.position - diff;
			SetCameraPosition(newPosition);

			if (diff.sqrMagnitude > 0.01f) {
				dragging = true;
			}
		}

		if ((lastInput == null || !lastInput.RightMouse) && p.RightMouse) {
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
		else if (p.RightMouse && lineCreationInProgress) {
			// Update line with position
			Line.Update(p.MousePositionWorldSpace);
		}
		else if (!p.RightMouse && lineCreationInProgress) {
			// verify final spot is actually tower head
			// if so, create line
			HexInfo h = Grid.TryGetCellInfoFromWorldPosition(p.MousePositionWorldSpace, out bool success);
			success = success && Line.IsValidPlacement(h);
			if (success) {
				Line.Place(h);
				Debug.Log("Line placed");
			}
			else {
				Destroy(Line.gameObject);
				Debug.Log("Line not placed");
			}
			lineCreationInProgress = false;
			Tower.gameObject.SetActive(true);
		}

		lastInput = p;
	}
}
