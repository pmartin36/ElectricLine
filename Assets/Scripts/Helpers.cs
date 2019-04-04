using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct TilePosition {
	public int X { get; set; }
	public int Y { get; set; }

	public TilePosition(int x, int y) {
		X = x;
		Y = y;
	}

	public override bool Equals(object obj) {
		if (!(obj is TilePosition))
			return false;

		TilePosition tp = (TilePosition)obj;
		return tp.X == this.X && tp.Y == this.Y;
	}

	public override int GetHashCode() {
		var hashCode = 1861411795;
		hashCode = hashCode * -1521134295 + X.GetHashCode();
		hashCode = hashCode * -1521134295 + Y.GetHashCode();
		return hashCode;
	}

	public static bool operator ==(TilePosition obj1, TilePosition obj2) {
		return obj1.X == obj2.X && obj1.Y == obj2.Y;
	}

	public static bool operator !=(TilePosition obj1, TilePosition obj2) {
		return !(obj1 == obj2);
	}

}