using UnityEngine;

public class HexMetrics {
	public float OuterRadius;
	public float InnerRadius;

	//    4_____5
	//    /     \
	//  3/       \0
	//   \       /
	//   2\_____/1
	//    
	// starting with rightmost point, going clockwise
	public Vector2[] Corners;

	public HexMetrics(int radius) {
		OuterRadius = radius;
		InnerRadius = OuterRadius * 0.866025404f;

		Corners = new Vector2[] {
			new Vector2(OuterRadius, 0),
			new Vector2(0.5f * OuterRadius, -InnerRadius),
			new Vector2(-0.5f * OuterRadius, -InnerRadius),
			new Vector2(-OuterRadius, 0),
			new Vector2(-0.5f * OuterRadius, InnerRadius),
			new Vector2(0.5f * OuterRadius, InnerRadius)
		};
	}

	public Vector3 RepresentationalCoordinatesToWorldCoordinates(int x, int y, int z = 0) {
		var absX = System.Math.Abs(x);
		return new Vector3(
			x * OuterRadius * .75f,
			(y + 0.5f * (absX & 1)) * InnerRadius,
			z);
	}

	public Vector3 RepresentationalCoordinatesToWorldCoordinates(Vector3Int rep) {
		return RepresentationalCoordinatesToWorldCoordinates(rep.x, rep.y);
	}

	public Vector3Int WorldCoordinatesToRepresentationalCoordinates(Vector3 world) {
		var repx = System.Math.Abs(Mathf.RoundToInt(world.x / (OuterRadius * .75f)));
		return new Vector3Int(
			repx,
			Mathf.RoundToInt((world.y / InnerRadius) - 0.5f * (repx & 1)),
			0);
	}
}

public enum HexMainDirection {
	X,
	Y
}
