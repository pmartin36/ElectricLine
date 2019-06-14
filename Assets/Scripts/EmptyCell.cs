using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyCell : HexCell
{
	public static event System.EventHandler TileFlipped;
	public bool HasFlipped { get; set; } = false;

    void Start() {
		
    }

    void Update() {
        
    }

	public void FlipTile() {
		if(!HasFlipped) {
			HasFlipped = true;
			TileFlipped?.Invoke(this, null);
		}
	}
}
