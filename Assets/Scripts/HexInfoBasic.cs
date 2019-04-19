using System;
using UnityEngine;

[Serializable]
public class HexInfoBasic {
	public virtual bool Occupied { 
		get => Filled;
	}

	public bool Filled { get; protected set; }
	public bool Locked { get; protected set; } // whatever this point is assigned to originally cannot be replaced

	public Vector3 PhysicalCoordinates;
	public HexCoordinates Coordinates;

	public int NumTouchedWalls { get; set; }
	public bool Reachable { get; set; }

	public HexCell Cell;

	public HexInfoBasic(int x, int y, HexMetrics metrics): this(x, y, metrics, false, false, false) { }
	public HexInfoBasic(int x, int y, HexMetrics metrics, bool fill, bool locked, bool reachable) {
		PhysicalCoordinates = metrics.RepresentationalCoordinatesToWorldCoordinates(x, y);

		this.Coordinates = HexCoordinates.FromRepresentationalCoordinates(x, y);
		this.Filled = fill;
		this.Locked = locked;
		this.Reachable = reachable;
		this.NumTouchedWalls = -1;
	}
}

