using System;
using UnityEngine;

[Serializable]
public class HexInfo {
	public bool Occupied { 
		get => Filled || TowerHead != null;
	}

	public bool Filled { get; private set; }
	public bool Locked { get; private set; } // whatever this point is assigned to originally cannot be replaced

	public Tower TowerHead { get; set; }

	public Vector3 PhysicalCoordinates;
	public HexCoordinates Coordinates;

	public int NumTouchedWalls { get; set; }
	public bool Reachable { get; set; }

	public HexCell Cell;

	public HexInfo(int x, int y, HexMetrics metrics): this(x, y, metrics, false, false, false) { }
	public HexInfo(int x, int y, HexMetrics metrics, bool fill, bool locked, bool reachable) {
		PhysicalCoordinates = metrics.RepresentationalCoordinatesToWorldCoordinates(x, y);

		this.Coordinates = HexCoordinates.FromRepresentationalCoordinates(x, y);
		this.Filled = fill;
		this.Locked = locked;
		this.Reachable = reachable;
		this.NumTouchedWalls = -1;
	}
}

