using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Switch : Tower {

	public static event System.EventHandler SwitchActivated;
	
	public bool Activated { get; set; }

	public static GameObject switchPrefab;
	public new static Switch CreateInstance() {
		switchPrefab = Resources.Load<GameObject>("Prefabs/Switch");
		return Instantiate(switchPrefab).GetComponentInChildren<Switch>();
	}

	public override void Touched() {
		Debug.Log("Touched up");
		if(!Activated) {
			Activated = true;
			StartCoroutine(Activate());
			SwitchActivated?.Invoke(this, null);
		}
	}

	public override void CreateFromData(TowerData td, HexGrid grid) {
		GridPosition = td.GridPosition;
		Place(grid);
	}

	public override void Place(HexGrid grid) {
		PhysicalHeadPosition = grid[GridPosition].PhysicalCoordinates;
		transform.parent.localScale = Vector3.one * grid.OuterRadius * transform.localScale.x / 2f;
		transform.parent.position = grid[GridPosition].PhysicalCoordinates + Vector3.back * 0.5f;
		grid.PlaceTower(this);
	}

	private IEnumerator Activate() {
		var sr = GetComponent<SpriteRenderer>();
		sr.color = Color.green;
		float time = 0;
		Vector3 start = Vector3.zero;
		Vector3 end = Vector3.forward * 180;
		while(transform.localEulerAngles.z < 180) {
			transform.localEulerAngles = Vector3.Lerp(start, end, time/0.5f);
			time += Time.deltaTime;
			yield return null;
		}
	}
}
