using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Utils {
	public static Vector2 RandomVectorInBox(Bounds b, float padding = 1) {
		return new Vector2(
			UnityEngine.Random.Range(b.min.x+padding, b.max.x-padding),
			UnityEngine.Random.Range(b.min.y+padding, b.max.y-padding)
		);
	}

	public static Vector3 AngleToVector(float angle) {
		return new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle)).normalized;
	}

	public static float xyToAngle(float x, float y) {
		return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
	}

	public static float VectorToAngle(Vector2 vector) {
		return xyToAngle(vector.x, vector.y);
	}

	public static Vector2 QuadraticBezier(Vector2 start, Vector2 end, Vector2 ctrl, float t) {
		return (1-t)*(1-t)*start + 2*t*(1-t)*ctrl + t*t*end;
	}

	public static float NegativeMod(float a, float b) {
		return a - b * Mathf.Floor(Mathf.Abs(a / b));
	}

	public static float AngleDiff(float a, float b) {
		return 180 - Mathf.Abs(Mathf.Abs(a - b) - 180);
	}

	public static float Cosh(float d) {
		return (Mathf.Exp(d) + Mathf.Exp(-d))/2f;
	}

	public static float Sinh(float d) {
		return (Mathf.Exp(d) - Mathf.Exp(-d)) / 2f;
	}
}


public struct Vector3Int {
	public int x { get; set; }
	public int y { get; set; }
	public int z { get; set; }

	public Vector3Int(int x, int y) : this() {
		this.x = x;
		this.y = y;
	}

	public Vector3Int(int x, int y, int z) : this() {
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public override string ToString() {
		return $"({x}, {y}, {z})";
	}
}

public static class HexShapes {

	private enum Shapes {
		Star,
		Line,
		J,
		Triangle3
	}

	/// <summary>
	///         *
	///         *
	///         *
	///       *   *
	///     *       *
	/// </summary>
	public static readonly HexCoordinates[] Star = {
		new HexCoordinates(0, 2),
		new HexCoordinates(0, 1),
		new HexCoordinates(0, 0),
		new HexCoordinates(-1, 0),   new HexCoordinates(+1, -1),
		new HexCoordinates(-2, 0),   new HexCoordinates(+2, -2),
	};

	/// <summary>
	/// *    *     *
	///	  *     *
	/// </summary>
	public static readonly HexCoordinates[] Line = {
		new HexCoordinates(-2, 1),
		new HexCoordinates(-1, 1),
		new HexCoordinates(0, 0),
		new HexCoordinates(1, 0),
		new HexCoordinates(2, -1),
	};

	/// <summary>
	///         *
	///           * 
	///       *  *       
	/// </summary>
	public static readonly HexCoordinates[] J = {
		new HexCoordinates(1, 0),
		new HexCoordinates(1, -1),
		new HexCoordinates(0, -1),
		new HexCoordinates(-1, 0),
	};

	/// <summary>
	///         *
	///        * *  
	///       * * *       
	/// </summary>
	public static readonly HexCoordinates[] Triangle3 = {
		new HexCoordinates(0, 0),
		new HexCoordinates(1, 0),
		new HexCoordinates(2, 0),
		new HexCoordinates(0, 1),
		new HexCoordinates(0, 2),
		new HexCoordinates(1, 1)
	};

	public static HexCoordinates[] GetRandomShape(int rotation = 0) {
		Array values = Enum.GetValues(typeof(Shapes));
		Shapes s = (Shapes)UnityEngine.Random.Range(0, values.Length);
		HexCoordinates[] r = new HexCoordinates[0];
		switch (s) {
			case Shapes.Star:
				r = Star;
				break;
			case Shapes.Line:
				r = Line;
				break;
			case Shapes.J:
				r = J;
				break;
			case Shapes.Triangle3:
				r = Triangle3;
				break;
		}
		return r.Select(c => c.Rotate(rotation)).ToArray();
	}

	public static HexCoordinates[] GetRotatedRandomShape() {
		var rotation = Mathf.RoundToInt(UnityEngine.Random.value * 6) * 60;
		return GetRandomShape(rotation);
	}
}