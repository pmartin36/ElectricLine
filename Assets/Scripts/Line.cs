using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(LineRenderer))]
public class Line : MonoBehaviour
{
	private HexGrid Grid;
	public float StepSize = 0.25f;
	private LineRenderer line;

	private HexInfo sourceHex;

    void Awake() {
		line = GetComponent<LineRenderer>();
    }

    void Update() {
        
    }

	public void Init(HexGrid grid, Vector3 mousePosition) {
		Grid = grid;
		HexInfo h = Grid.TryGetCellInfoFromWorldPosition(mousePosition, out bool success); // don't need to check success, it's already done in the calling method
	
		sourceHex = h;
	}

	public void Update(Vector3 position) {
		Vector3 dist = transform.position - position;
		float mag = dist.magnitude;

		HexInfo hex = Grid.TryGetCellInfoFromWorldPosition(position, out bool success);
		if (success && Mathf.Abs(hex.PhysicalCoordinates.x - sourceHex.PhysicalCoordinates.x) > 0.1f && mag > StepSize) {	
			float d = Mathf.Abs(dist.x);
			float h = Mathf.Abs(dist.y);
			float s = mag * 1.1f;
			float a = ApproximateAlpha(s, d, h);

			float ln = Mathf.Log((s + h) / (s - h));		
			float x1 = 0.5f * (a * ln - d);
			float y1 = a * Utils.Cosh(x1 / a);

			float v1y = Mathf.Min(transform.position.y, position.y);
			float diffY = y1 - v1y;

			Vector3 v1, v2;
			float direction;
			if(transform.position.y < position.y) {
				v1 = transform.position;
				v2 = position;
				direction = Mathf.Sign(position.x - transform.position.x);
			}
			else {
				v1 = position;
				v2 = transform.position;
				direction = Mathf.Sign(transform.position.x - position.x);
			}
			
			List<Vector3> positions = new List<Vector3>() { new Vector3(v1.x, v1.y, -1) };
			for(float i = StepSize; i < d; i+= StepSize) {
				float y = a * Utils.Cosh((i + x1) / a) - diffY;
				positions.Add(new Vector3(
					v1.x + i * direction,
					y,
					-1));
			}
			positions.Add(new Vector3(v2.x, v2.y, -1));

			line.positionCount = positions.Count;
			for (int i = 0; i < positions.Count; i++) {			
				line.SetPositions(positions.ToArray());
			}
		}
		else {
			line.positionCount = 2;
			line.SetPositions(new Vector3[] {
				transform.position,
				position
			});
		}
	}

	public float ApproximateAlpha(float length, float xDist, float yDist) {
		int maxIterations = 10;
		float maxError = 0.1f;

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
		float al_prev = 1;
		float al = 1 - c(1);
		while (Mathf.Abs(al - al_prev) > maxError && i < maxIterations) {
			al_prev = al;
			al -= c(al);
			i++;
		}

		return al;
	}

	public bool IsValidPlacement(Vector3 position) {
		HexInfo h = Grid.TryGetCellInfoFromWorldPosition(position, out bool success);
		if (!success) return false;
		
		return h.TowerHead != null && h.TowerHead.gameObject != sourceHex.TowerHead.gameObject;
	}

	public void Place() {

	}
}
