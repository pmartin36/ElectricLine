using System;
using UnityEngine;

[Serializable]
public struct HexCoordinates {
	[SerializeField]
	private int x, y;
	public int X { get => x; set => x = value; }
	public int Y { get => y; set => y = value; }
	public int Z { get => -X - Y; }

	public HexCoordinates(int x, int y) {
		this.x = x;
		this.y = y;
	}

	public override string ToString() {
		return $"({X}, {Y}, {Z})";
	}

	public string ToStringOnSeparateLines() {
		return $"{X}\n{Y}\n{Z}";
	}

	public static HexCoordinates FromRepresentationalCoordinates(int x, int y) {
		return new HexCoordinates(x, y - x / 2);
	}

	public static HexCoordinates operator+(HexCoordinates i1, HexCoordinates i2) {
		return new HexCoordinates(i1.x + i2.x, i1.y + i2.y);
	}

	public static bool operator ==(HexCoordinates i1, HexCoordinates i2) {
		return i1.x == i2.x && i1.y == i2.y;
	}

	public static bool operator !=(HexCoordinates i1, HexCoordinates i2) {
		return !(i1 == i2);
	}

	public static int Distance(HexCoordinates i1, HexCoordinates i2) {
		var diffX = Mathf.Abs(i1.X - i2.X);
		var diffY = Mathf.Abs(i1.Y - i2.Y);
		var diffZ = Mathf.Abs(i1.Z - i2.Z);
		return Mathf.Max(diffX, diffY, diffZ);
	}

	public static bool IsAtLeastCellsAway(HexCoordinates i1, HexCoordinates i2, int cellsAway) {
		return Distance(i1,i2) > cellsAway;
	}

	public Vector3Int GetRepresentationalCoordinates() {
		return new Vector3Int(x, y + x / 2);
	}

	public HexCoordinates Rotate(int degree) {
		switch(degree) {
			
			case 60:
			case -300:
				return new HexCoordinates(-Y, -Z);
			case 120:
			case -240:
				return new HexCoordinates(Z, X);

			case -60:
			case 300:
				return new HexCoordinates(-Z, -X);
			case -120:
			case 240:
				return new HexCoordinates(Y, Z);


			case 0:
			case 360:
				return this;
			case 180:
			case -180:
				return new HexCoordinates(-X, -Y);
			default:
				throw new Exception("Invalid degree rotation");
		}
	}

	public HexCoordinates[] GetNeighbors() {
		return new HexCoordinates[] {
			new HexCoordinates(X - 1, Y + 1), // positive z
			new HexCoordinates(X + 1, Y - 1), // negtaive z
			new HexCoordinates(X,	  Y + 1), // positive x
			new HexCoordinates(X,     Y - 1), // negative x
			new HexCoordinates(X + 1, Y),	   // positive y
			new HexCoordinates(X - 1, Y)	   // negative y
		};
	}
}
