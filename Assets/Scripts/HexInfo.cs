using System;
using UnityEngine;

[Serializable]
public class HexInfo : HexInfoBasic {
	public override bool Occupied { 
		get => Filled || TowerHead != null;
	}
	public Tower TowerHead {
		get => HexGameObject as Tower;
		set => HexGameObject = value; 
	}

	public MonoBehaviour HexGameObject { get; set; }

    public HexInfo(int x, int y, HexMetrics metrics) : base(x, y, metrics, false, false, false) { }
    public HexInfo(int x, int y, HexMetrics metrics, bool fill, bool locked, bool reachable)
        : base(x, y, metrics, fill, locked, reachable){ }
}

