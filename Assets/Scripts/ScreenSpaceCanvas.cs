using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenSpaceCanvas : MonoBehaviour
{
	public Image Blacker;

	void Start() {
		
    }

    void Update() {
        
    }

	public void BlackScreen(System.Action onComplete) {
		StartCoroutine(ShowBlacker(onComplete));
	}

	IEnumerator ShowBlacker(System.Action onComplete) {
		while(Blacker.color.a < 1) {
			Blacker.color = new Color(0,0,0, Blacker.color.a + Time.deltaTime);
			yield return null;
		}
		onComplete();
	}
}
