using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
	private int height;
	public int Height {
		get => height;
		set {
			height = value;
			header.transform.localPosition =  GetPolePosition(value - 1);
			int i = 0;
			for(i = 0; i < value - 1; i++) {
				if(poles.Count > i) {
					poles[i].SetActive(true);
				}
				else { 
					GameObject o = Instantiate(PolePrefab, this.transform);
					o.transform.localPosition = GetPolePosition(i);
					o.transform.localRotation = Quaternion.identity;
					o.SetActive(true);
					poles.Add(o);
				}
			}

			for(;i < poles.Count; i++) {
				poles[i].SetActive(false);
			}
		}
	}

	public HexCoordinates GridPosition;

	public GameObject PolePrefab;
	private List<GameObject> poles = new List<GameObject>();
	private SpriteRenderer header;

	public List<Line> Lines;

	private void Awake() {
		Lines = new List<Line>();
		header = GetComponentInChildren<SpriteRenderer>();
	}

	void Start() {
		
    }

	private Vector3 GetPolePosition(int i) {
		return new Vector3(0, i * 2f, 0);
	}

	public void Place() {
		header.color = Color.white;
		for(int i = 0; i < Height - 1; i++) {
			poles[i].GetComponent<SpriteRenderer>().color = Color.white;
		}
	}

	public void CreateFromData(TowerData td, HexGrid grid) {
		GridPosition = td.GridPosition;	
		
		transform.eulerAngles = new Vector3(0,0,td.Rotation);

		//dir is relative to Vector3.right, so we have to rotation -90 to make it correspond to Vector3.down
		//but then we want to reverse the direction (so that we are going from HEAD to BASE (to find position) so we add 180
		Vector3 dir = Utils.AngleToVector(td.Rotation + 90);
		float h = 0.5f - td.Height;
		transform.position = grid[GridPosition].PhysicalCoordinates + dir * h * grid.InnerRadius + Vector3.back * 0.5f;

		Height = td.Height;
		Place();
		grid.PlaceTower(this);
	}
}