using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
	public HexCoordinates coords;
	public Color color { set => GetComponent<SpriteRenderer>().color = value; }

	void Start() {
		
    }

    void Update() {
        
    }
}
