using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
	private HexGrid Grid;
	private List<Transform> availableLinks;

	public float Alpha = 0.5f;

	[SerializeField]
	private Transform LinkContainer;
	[SerializeField]
	private Transform LinkPrefab;

	private HingeJoint2D lastLink;

	private Rigidbody2D sourceConnection;

	private int lastFrameNumUnits;

    void Awake() {
		
    }

    void Update() {
        
    }

	public void Init(HexGrid grid, Vector3 mousePosition) {
		Grid = grid;
		HexInfo h = Grid.TryGetCellInfoFromWorldPosition(mousePosition, out bool success); // don't need to check success, it's already done in the calling method
	
		sourceConnection = h.TowerHead.GetComponent<Rigidbody2D>();
		availableLinks = new List<Transform>();

		//float angle = Vector2.SignedAngle(transform.position - mousePosition, Vector2.right);
		//lastLink = Instantiate(LinkPrefab, mousePosition, Quaternion.Euler(0,0,angle), LinkContainer); // existing hinge joint links up to next link
		//lastLink.gameObject.AddComponent(typeof(HingeJoint2D)); // added hinge joint links up to mouse position
		//lastLink.connectedBody = sourceConnection;
		//lastLink.name = "Last Link";

		//availableLinks = new List<HingeJoint2D>() {
		//	lastLink
		//};
		//lastFrameNumUnits = 1;
	}

	public void Update(Vector3 position) {
		Vector3 dist = transform.position - position;
		float mag = dist.magnitude;
		float stepSize = 0.25f;
		if (mag > stepSize) {
			Vector3 v1 = (dist.x > 0 ? position : transform.position);
			float d = Mathf.Abs(dist.x);
			float h = Mathf.Abs(dist.y);
			float s = mag * 1.2f;
			float a = ApproximateAlpha(s, d, h, v1);

			float ln = Mathf.Log((s + h) / (s - h));		
			float x1 = 0.5f * (a * ln - d);
			float y1 = a * Utils.Cosh(x1 / a);
			float diffX = x1 - v1.x;
			float diffY = y1 - v1.y;
			
			List<Vector3> positions = new List<Vector3>() {};
			for(float i = 0; i < d; i+= stepSize) {
				float y = a * Utils.Cosh((i + x1) / a) - diffY;
				positions.Add(new Vector3(
					v1.x + i,
					y,
					-1));
			}

			for (int i = 0; i < positions.Count; i++) {
				if (availableLinks.Count > i) {
					availableLinks[i].transform.position = positions[i];
				}
				else {
					Transform newlink = Instantiate(LinkPrefab, positions[i], Quaternion.identity, LinkContainer);
					newlink.name = $"Link {i}";
					availableLinks.Add(newlink);
				}
			}
		}
	}

	public float ApproximateAlpha(float length, float xDist, float yDist, Vector3 v1) {
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

		int i = 0;
		float x_prev = 1;
		float x = 1 - c(1);
		while (Mathf.Abs(x - x_prev) > maxError && i < maxIterations) {
			x_prev = x;
			x -= c(x);
			i++;
		}

		return x;
	}

	public bool IsValidPlacement(Vector3 position) {
		HexInfo h = Grid.TryGetCellInfoFromWorldPosition(position, out bool success);
		if (!success) return false;
		
		return h.TowerHead != null && h.TowerHead.gameObject != sourceConnection.gameObject;
	}

	public void Place() {

	}
}
