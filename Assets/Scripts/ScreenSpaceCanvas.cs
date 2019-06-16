using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenSpaceCanvas : MonoBehaviour
{
	public Image Blacker;
	public Slider EnergySlider;

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

	public void InitEnergySlider(float max) {
		RectTransform rectTransform = EnergySlider.GetComponent<RectTransform>();
		float left = Mathf.Lerp(1000, 100, (max - 1) / 4);
		rectTransform.offsetMin = new Vector2(left, rectTransform.offsetMin.y);
		EnergySlider.maxValue = max;
		EnergySlider.value = max;
	}

	public void UpdateEnergySlider(float val) {
		EnergySlider.value = val;
	}
}
