﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HexGrid : MonoBehaviour
{
	public int cellRadius;
	public int width;
	public int height;

	public HexMainDirection Direction = HexMainDirection.Y;

	private Vector3 HexSize;
	public Bounds PhysicalGridBounds { get; private set; }

	public Bounds PaddedGridDimensions {
		get {
			Vector3 outerxy = new Vector3(metrics.OuterRadius, metrics.InnerRadius, 0);
			return new Bounds(
				PhysicalGridBounds.center,
				PhysicalGridBounds.size + 2 * outerxy
			);
		}
	}

	public Bounds InversePaddedGridDimensions {
		get
		{
			Vector3 outerxy = new Vector3(metrics.OuterRadius, metrics.InnerRadius, 0);
			return new Bounds(
				PhysicalGridBounds.center,
				PhysicalGridBounds.size - 2 * outerxy
			);
		}
	}

	public GameObject startCellPrefabTEMP;
	public GameObject finishCellPrefabTEMP;
	public HexCell cellPrefab;

	private HexMetrics metrics;
	private Dictionary<HexCoordinates, HexInfo> cellInfo;

	public HexInfo StartingPoint;
	private HexInfo EndingPoint;

	void Awake() {
		metrics = new HexMetrics(cellRadius);

		Vector3 phys = metrics.RepresentationalCoordinatesToWorldCoordinates(width, height); ;
		HexSize = Direction == HexMainDirection.X ?
			new Vector3(metrics.InnerRadius, metrics.OuterRadius, 0) :
			new Vector3(metrics.OuterRadius, metrics.InnerRadius, 0);


		PhysicalGridBounds = new Bounds(
			(phys - HexSize) / 2f,
			phys
		);

		cellInfo = new Dictionary<HexCoordinates, HexInfo>();

		GenerateGrid();
	}

	private void Start() {
		GameManager.Instance.LevelManager.Grid = this;
	}

	private void GenerateGrid() {
		SetStartEnd();
		var activeList = new List<HexCoordinates>() { StartingPoint.Coordinates };
		var fullList = new List<HexCoordinates>() { StartingPoint.Coordinates };
		var smallRadiusSquared = 9;

		// generate center points for all the shapes to be generated
		while (activeList.Count > 0) {
			var active = activeList[UnityEngine.Random.Range(0, activeList.Count - 1)];
			var activeRepresentational = active.GetRepresentationalCoordinates();
			bool foundSpot = true;
			for (int i = 0; i < 20; i++) {
				foundSpot = true;
				float angle = 2 * Mathf.PI * UnityEngine.Random.value;
				float r = Mathf.Sqrt(UnityEngine.Random.value * 3 * smallRadiusSquared + smallRadiusSquared); // See: http://stackoverflow.com/questions/9048095/create-random-number-within-an-annulus/9048443#9048443
				Vector2 candidate = r * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
				int x = Mathf.FloorToInt(candidate.x + activeRepresentational.x);
				int y = Mathf.FloorToInt(candidate.y + activeRepresentational.y);
				HexCoordinates coord = HexCoordinates.FromRepresentationalCoordinates(x, y);
				if (WithinGridBounds(x,y) && !(cellInfo.ContainsKey(coord) && cellInfo[coord].Locked)) {
					foreach (HexCoordinates h in fullList) {
						if (!HexCoordinates.IsAtLeastCellsAway(coord, h, 3)) {
							foundSpot = false;
							break;
						}
					}
				}
				else {
					foundSpot = false;
				}

				if (foundSpot) {
					activeList.Add(coord);
					fullList.Add(coord);
					break;
				}
			}
			if (!foundSpot) {
				activeList.Remove(active);
			}
		}

		// create shapes
		fullList.Remove(StartingPoint.Coordinates);
		foreach (HexCoordinates h in fullList) {
			HexCoordinates[] shapeCoords = HexShapes.GetRotatedRandomShape();
			foreach (HexCoordinates coord in shapeCoords) {
				HexCoordinates cubePos = h + coord;
				if (!(cellInfo.ContainsKey(cubePos) && cellInfo[cubePos].Locked)) {
					var rc = cubePos.GetRepresentationalCoordinates();
					if (WithinGridBounds(rc)) {
						cellInfo[cubePos] = new HexInfo(rc.x, rc.y, metrics, true, false, false);
					}
				}
			}
		}

		// Determine points that are touching a wall
		GenerateNumWallsMap(StartingPoint.Coordinates);
		// if ending point isn't reachable - generation failed, generate again
		if (!EndingPoint.Reachable) {
			cellInfo.Clear();
			GenerateGrid();
			return;
		}

		// Create start/end cells
		GameObject startCell = Instantiate(startCellPrefabTEMP);
		startCell.transform.position = new Vector3(StartingPoint.PhysicalCoordinates.x, StartingPoint.PhysicalCoordinates.y, -0.5f);

		GameObject endCell = Instantiate(finishCellPrefabTEMP);
		endCell.transform.position = new Vector3(EndingPoint.PhysicalCoordinates.x, EndingPoint.PhysicalCoordinates.y, -0.5f);


		// fill in unreachable points --- TEMPORARY
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				var hexCoords = HexCoordinates.FromRepresentationalCoordinates(x, y);
				if (!cellInfo.ContainsKey(hexCoords)) {
					var hexInfo = new HexInfo(x, y, metrics, false, false, false);
					cellInfo[hexCoords] = hexInfo;
				}
			}
		}

		// Actually create cells
		foreach (HexInfo h in cellInfo.Values) {
			//if (h.Filled) {
			//	HexCell cell = Instantiate<HexCell>(cellPrefab);
			//	cell.transform.SetParent(transform, false);
			//	cell.transform.localPosition = h.PhysicalCoordinates;
			//	cell.color = new Color((float)h.PhysicalCoordinates.x / (width * cellRadius), (float)h.PhysicalCoordinates.y / (height * cellRadius), 0, 1);
			//	cell.coords = h.Coordinates;
			//	h.Cell = cell;
			//}

			var x = (float)h.PhysicalCoordinates.x / (width * cellRadius);
			var y = (float)h.PhysicalCoordinates.y / (height * cellRadius);
			if (h.Filled) {
				HexCell cell = Instantiate<HexCell>(cellPrefab);
				cell.transform.SetParent(transform, false);
				cell.transform.localPosition = h.PhysicalCoordinates;
				cell.coords = h.Coordinates;
				if (h.Locked) {
					cell.color = new Color(x, y, 1, h.Filled ? 1 : 0.5f);
				}
				else {
					cell.color = new Color(x, y, 1, h.Filled ? 1 : 0.5f);
				}
				h.Cell = cell;
			}
			else {
				GameObject c = Instantiate(startCellPrefabTEMP);
				if (!h.Reachable) {
					c.GetComponent<SpriteRenderer>().color = new Color(x, y, 1);
				}
				else {
					c.GetComponent<SpriteRenderer>().color = new Color(0.0f, 1f * h.NumTouchedWalls / 6f, 0.0f);
				}
				c.transform.SetParent(transform, false);
				c.transform.localPosition = new Vector3(h.PhysicalCoordinates.x, h.PhysicalCoordinates.y, 0.5f);
			}
		}
	}

	private void GenerateNumWallsMap(HexCoordinates origin) {
		HashSet<HexCoordinates> hexes = new HashSet<HexCoordinates>();
		hexes.Add(origin);

		bool addedNew = true;
		while(addedNew) {
			addedNew = false;
			var hexesList = hexes.ToList();
			foreach(HexCoordinates hex in hexesList) {
				int neighboringWalls = 0;
				Vector3Int hexRep = hex.GetRepresentationalCoordinates();
				HexCoordinates[] neighbors = hex.GetNeighbors();

				foreach(HexCoordinates n in neighbors) {
					if( cellInfo.ContainsKey(n) ) {
						if(cellInfo[n].Filled) {
							neighboringWalls++;
						}
						else {
							cellInfo[n].Reachable = true;
						}
					}
					else {
						Vector3Int rep = n.GetRepresentationalCoordinates();
						if (WithinGridBounds(rep)) {
							if (!hexes.Contains(n)) {
								addedNew = true;
								hexes.Add(n);
							}
							if (!cellInfo.ContainsKey(n)) {
								cellInfo[n] = new HexInfo(rep.x, rep.y, metrics, false, false, true);
							}
						}
						else {
							neighboringWalls++;
						}

						
					}
				}

				cellInfo[hex].NumTouchedWalls = neighboringWalls;
				hexes.Remove(hex);
			}
		}
	}

	private void SetStartEnd() {
		int xs, ys, xe, ye;
		xs = UnityEngine.Random.Range(0, width);
		ys = UnityEngine.Random.Range(height - 3, height-1);
		StartingPoint = new HexInfo(xs, ys, metrics, false, true, true);

		// create point below starting point
		HexCoordinates h = HexCoordinates.FromRepresentationalCoordinates(xs,ys);
		h.Y -= 1;
		Vector3Int rep = h.GetRepresentationalCoordinates();
		HexInfo belowStart = new HexInfo(rep.x, rep.y, metrics, true, true, false);

		xe = UnityEngine.Random.Range(0, width);
		ye = UnityEngine.Random.Range(0, 3);
		EndingPoint = new HexInfo(xe, ye, metrics, false, true, false);

		cellInfo.Add(StartingPoint.Coordinates, StartingPoint);
		cellInfo.Add(EndingPoint.Coordinates, EndingPoint);
		cellInfo.Add(belowStart.Coordinates, belowStart);
	}

	private bool WithinGridBounds(int x, int y) {
		return x >= 0 && x < width && y >= 0 && y < height;
	}
	private bool WithinGridBounds(Vector3Int point) {
		return WithinGridBounds(point.x, point.y);
	}

	public HexCoordinates TryGetHexCoordinateFromWorldPosition(Vector3 world, out bool success) {
		Vector3Int rep = metrics.WorldCoordinatesToRepresentationalCoordinates(world);
		success = WithinGridBounds(rep);
		HexCoordinates hex = HexCoordinates.FromRepresentationalCoordinates(rep.x, rep.y);	
		return hex;
	}

	public Vector3 TryGetTowerPlacement(Vector3 position, out bool success, float towerOffset = 0) {
		HexCoordinates hex = TryGetHexCoordinateFromWorldPosition(position, out success);
		if(!success) return Vector3.zero;

		HexInfo info = cellInfo[hex];
		success = !info.Filled && info.NumTouchedWalls > 0;
		if(!success) return Vector3.zero;

		HexInfo closestHex = info;
		float minDistance = 1001;
		foreach (HexInfo i in GetWallNeighbors(hex).Select( n => cellInfo[n])) {
			float dist = Vector2.Distance(i.PhysicalCoordinates, position);
			if (dist < minDistance) {
				minDistance = dist;
				closestHex = i;
			}
		}

		success = minDistance < metrics.InnerRadius * 0.9f;
		
		Vector3 diff = closestHex.PhysicalCoordinates - info.PhysicalCoordinates;
		Vector3 posRot = info.PhysicalCoordinates + (diff / 2f) - diff.normalized * towerOffset;
		posRot.z = Vector2.SignedAngle(Vector2.down, diff);
		return posRot;
	}

	private List<HexCoordinates> GetWallNeighbors(HexCoordinates hex) {
		HexCoordinates[] neighbors = hex.GetNeighbors();
		List<HexCoordinates> walledNeighbors = new List<HexCoordinates>();
		foreach(HexCoordinates h in neighbors) {
			if(cellInfo.ContainsKey(h) && cellInfo[h].Filled) {
				walledNeighbors.Add(h);
			}
		}
		return walledNeighbors;
	}

	//step through cells over many frames approach
	private IEnumerator GenerateNumWallsMap2(HexCoordinates origin) {
		HashSet<HexCoordinates> visited = new HashSet<HexCoordinates>(cellInfo.Keys.ToList());
		yield return null;
		HashSet<HexCoordinates> hexes = new HashSet<HexCoordinates>();
		hexes.Add(origin);

		bool addedNew = true;
		while (addedNew) {
			addedNew = false;
			var hexesList = hexes.ToList();
			foreach (HexCoordinates hex in hexesList) {
				int neighboringWalls = 0;
				Vector3Int hexRep = hex.GetRepresentationalCoordinates();
				HexCoordinates[] neighbors = new HexCoordinates[] {
					new HexCoordinates(hex.X - 1, hex.Y + 1), // positive z
					new HexCoordinates(hex.X + 1, hex.Y - 1), // negtaive z
					new HexCoordinates(hex.X,  hex.Y + 1), // positive x
					new HexCoordinates(hex.X,  hex.Y - 1), // negative x
					new HexCoordinates(hex.X + 1, hex.Y),	   // positive y
					new HexCoordinates(hex.X - 1, hex.Y)	   // negative y
				};

				foreach (HexCoordinates n in neighbors) {
					if (visited.Contains(n)) {
						if (cellInfo[n].Filled) {
							neighboringWalls++;
						}
					}
					else {
						Vector3Int rep = n.GetRepresentationalCoordinates();
						if (WithinGridBounds(rep)) {
							if (!hexes.Contains(n)) {
								addedNew = true;
								hexes.Add(n);
							}
							visited.Add(n);
						}
						else {
							neighboringWalls++;
						}
					}
				}

				cellInfo[hex].NumTouchedWalls = neighboringWalls;
				cellInfo[hex].Cell.color = new Color(0.0f, 1f * neighboringWalls / 6f, 0.0f);
				hexes.Remove(hex);
				yield return null;
			}
		}
	}
}