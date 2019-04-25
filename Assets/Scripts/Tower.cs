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
			header.transform.localPosition =  GetPosition(value - 1);
			int i = 0;
			for(i = 0; i < value - 1; i++) {
				if(poles.Count > i) {
					poles[i].SetActive(true);
				}
				else { 
					GameObject o = Instantiate(PolePrefab, this.transform);
					o.transform.localPosition = GetPosition(i);
					o.transform.localRotation = Quaternion.identity;
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

    void Start() {
		Lines = new List<Line>();
		header = GetComponentInChildren<SpriteRenderer>();
    }

	private Vector3 GetPosition(int i) {
		return new Vector3(0, i * 2f, 0);
	}

	public void Place() {
		header.color = Color.white;
		for(int i = 0; i < Height - 1; i++) {
			poles[i].GetComponent<SpriteRenderer>().color = Color.white;
		}
	}
}
