using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent (typeof(LineRenderer))]
public class Line : MonoBehaviour
{
	private HexGrid Grid;
	public float StepSize = 0.25f;
	private LineRenderer line;
	private PolygonCollider2D polyCollider;

	public float LineWidth;

	public HexInfo SourceHex { get; private set; }
	public HexInfo DestHex { get; private set; }
	public HexCoordinates Direction {
		get => SourceHex.Coordinates - DestHex.Coordinates;
	}
	public Vector2 PhysicalCoordinatesDirection {
		get => Grid.RepresentationalCoordinatesToPhysicalCoordinates(Direction.GetRepresentationalCoordinates());
	}

	public List<LinePosition> Positions { get; set; }

    void Awake() {
		line = GetComponent<LineRenderer>();
		line.startWidth = LineWidth;
		line.endWidth = LineWidth;

		polyCollider = GetComponent<PolygonCollider2D>();
    }

    void Update() {
        
    }

	public void Init(HexGrid grid, Vector3 mousePosition) {
		Grid = grid;
		HexInfo h = Grid.TryGetCellInfoFromWorldPosition(mousePosition, out bool success); // don't need to check success, it's already done in the calling method
	
		SourceHex = h;
	}

	public void CreateFromData(LineData ld, HexGrid grid) {
		Grid = grid;
		SourceHex = grid[ld.SourceHex];
		transform.position = SourceHex.PhysicalCoordinates;
		Place(grid[ld.DestHex]);
	}

	public void Update(Vector3 position) {
		Vector3 dist = transform.position - position;
		float mag = dist.magnitude;

		HexInfo hex = Grid.TryGetCellInfoFromWorldPosition(position, out bool success);
		if (success && Mathf.Abs(hex.PhysicalCoordinates.x - SourceHex.PhysicalCoordinates.x) > 0.1f && mag > StepSize) {
			Vector3 v1, v2;
			float direction;
			if (transform.position.y < position.y) {
				v1 = transform.position;
				v2 = position;
				direction = Mathf.Sign(position.x - transform.position.x);
			}
			else {
				v1 = position;
				v2 = transform.position;
				direction = Mathf.Sign(transform.position.x - position.x);
			}
			Positions = new List<LinePosition>() { new LinePosition(new Vector3(v1.x, v1.y, -1)) };

			float d = Mathf.Abs(dist.x);
			float h = Mathf.Abs(dist.y);
			float s = mag * 1.05f;
			float a = TryApproximateAlpha(s, d, h, out success);
			if(success) {
				float ln = Mathf.Log((s + h) / (s - h));		
				float x1 = 0.5f * (a * ln - d);
				float y1 = a * Utils.Cosh(x1 / a);

				float v1y = Mathf.Min(transform.position.y, position.y);
				float diffY = y1 - v1y;
			
				
				for(float i = StepSize; i < d; i+= StepSize) {
					float y = a * Utils.Cosh((i + x1) / a) - diffY;
					Positions.Add(new LinePosition(new Vector3(
						v1.x + i * direction,
						y,
						-1)));
				}
			}
			Positions.Add(new LinePosition(new Vector3(v2.x, v2.y, -1)));		
		}
		else {
			Positions = new List<LinePosition>() {
				new LinePosition(new Vector3(transform.position.x, transform.position.y, -1)),
				new LinePosition(new Vector3(position.x, position.y, -1))
			};
		}

		Vector2[] ppoints = new Vector2[Positions.Count * 2];
		Vector3[] positions = new Vector3[Positions.Count];
		ppoints[0] = Vector2.zero;
		line.positionCount = Positions.Count;

		int length = ppoints.Length;
		Vector3 dir = Vector3.zero;
		Vector3 normal = Vector3.zero;
		for (int i = 0; i < Positions.Count; i++) {
			var pos = Positions[i];
			if(i != Positions.Count - 1 ) {
				dir = (pos.Position - Positions[i + 1].Position).normalized;
				normal = Quaternion.AngleAxis(90, Vector3.back) * dir;
			}
			var offset = normal * LineWidth / 4f;
			ppoints[i] = pos.Position + offset - transform.position;
			ppoints[length - i - 1] = pos.Position - offset - transform.position;
			positions[i] = Positions[i].Position;

			pos.Direction = dir;
			pos.Normal = normal;
		}
		polyCollider.points = ppoints;
		line.SetPositions(positions);
	}

	public float TryApproximateAlpha(float length, float xDist, float yDist, out bool success) {
		int maxIterations = 20;
		float maxError = 0.1f;
		float initial = 2f;

		// newton-raphson approximation
		// x1 = x0 - f(x0) / f'(x0)
		// compare x0 to x1, if diff is in acceptable tolerance, you've found x.
		System.Func<float, float> c = (a) => {
			float f = 2 * a * Utils.Sinh(xDist / (2 * a)) - Mathf.Sqrt(length * length - yDist * yDist);
			float fp = 2 * Utils.Sinh(xDist / (2 * a)) - xDist * Utils.Cosh(xDist / (2 * a)) / a;
			return f / fp;
		};

		// first guess for alpha is 1
		int i = 0;
		float al_prev = initial;
		float al = al_prev - c(al_prev);
		while (Mathf.Abs(al - al_prev) > maxError && i < maxIterations) {
			al_prev = al;
			al -= c(al);
			i++;
		}

		success = i < maxIterations && al > maxError;
		// Debug.Log($"{success}, {al}");
		return al;
	}

	public bool IsValidPlacement(HexInfo h) {
		bool isValidTowerHead = h.TowerHead != null && h.TowerHead.gameObject != SourceHex.TowerHead.gameObject;
		if(!isValidTowerHead) return false;

		var dir = SourceHex.Coordinates - h.Coordinates;
		var ndir = -dir;
		bool lineExists = SourceHex.TowerHead.Lines.Any(s => s.Direction == dir || s.Direction == ndir);
		return !lineExists;
	}

	public bool IsValidPlacement(Vector3 position) {
		HexInfo h = Grid.TryGetCellInfoFromWorldPosition(position, out bool success);	
		return success && IsValidPlacement(h);
	}

	public void Place(HexInfo h) {
		DestHex = h;
		Update(h.PhysicalCoordinates);

		SourceHex.TowerHead.Lines.Add(this);
		DestHex.TowerHead.Lines.Add(this);
	}

	public ValueTuple<int, int> GetNearestIndexAndDirection(Vector3 pos, bool edgeIndex, Vector3? velocity = null) {
		if(edgeIndex) {
			return (Positions[0].Position - pos).sqrMagnitude < (Positions[Positions.Count-1].Position - pos).sqrMagnitude 
				? new ValueTuple<int, int>(0, 1)
				: new ValueTuple<int, int>(Positions.Count-1, -1);
		}
		else {
			float closestDist = Mathf.Infinity;
			int closest = 0;
			for(int i = 0; i < Positions.Count; i++) {
				float sqrMag = (Positions[i].Position - pos).sqrMagnitude;
				if(sqrMag < closestDist) {
					closestDist = sqrMag;
					closest = i;
				}
			}

			Vector3 v = velocity ?? Vector3.zero;
			v.Scale(new Vector3(2, 1, 1)); // make velocity in the x direction weigh double

			int nextIndex = 0;
			int previousIndex = 0;
			if (closest == 0) {
				previousIndex = 0;
				nextIndex = 1;
			}
			else if (closest == Positions.Count - 1) {
				previousIndex = Positions.Count - 1;
				nextIndex = Positions.Count - 2;
			}
			else {	
				nextIndex = closest + 1;
				previousIndex = closest - 1;
			}

			Vector3 nextIndexPosition = Positions[nextIndex].Position - Positions[closest].Position;
			Vector3 previousIndexPosition = Positions[previousIndex].Position - Positions[closest].Position;

			if(Vector2.Dot(v, nextIndexPosition) > Vector2.Dot(v, previousIndexPosition)) {			
				if (Positions.Count == 2) {
					// if we're closer to next index, get the other point so that we can continue towards next index
					return new ValueTuple<int, int>(previousIndex, nextIndex - closest);
				}
				else {
					return new ValueTuple<int, int>(closest, nextIndex - closest);
				}
			}
			else {
				if (Positions.Count == 2) {
					return new ValueTuple<int, int>(nextIndex, closest - nextIndex);
				}
				else {
					return new ValueTuple<int, int>(closest, closest - nextIndex);
				}
			}
					
		}
	}

	public Tower GetClosestTower(Vector3 position) {
		var distToSource = (position - SourceHex.PhysicalCoordinates).sqrMagnitude;
		var distToDest = (position - DestHex.PhysicalCoordinates).sqrMagnitude;
		return distToSource < distToDest ? SourceHex.TowerHead : DestHex.TowerHead;
	}
}

public class LinePosition {
	public Vector3 Position { get; set; }
	public Vector3 Normal { get; set; }
	public Vector3 Direction { get; set; }

	public LinePosition(Vector3 p, Vector3 n) {
		Position = p;
		Normal = n;
	}

	public LinePosition(Vector3 position) {
		Position = position;
		Normal = Vector2.zero;
	}
}
