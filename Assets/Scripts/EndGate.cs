using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGate : MonoBehaviour
{
    public bool IsOpen { get; set; } = false;

	public void Open() {
		IsOpen = true;
		StartCoroutine(OpenGate());
	}

	IEnumerator OpenGate() {
		float offset = 0;
		while(offset < 1) {
			foreach(Transform t in transform) {
				t.localPosition = t.localRotation * Vector3.down * offset;
			}
			offset += Time.deltaTime;
			yield return null;
		}
	}
}
