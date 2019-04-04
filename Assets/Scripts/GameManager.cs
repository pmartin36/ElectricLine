using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager> {

	private float _timeScale = 1f;
	public float TimeScale {
		get {
			return _timeScale;
		}
		set {
			Time.timeScale = value;
			_timeScale = value;
		}
	}

	public ContextManager ContextManager;
	public LevelManager LevelManager {
		get
		{
			return ContextManager as LevelManager;
		}
		set
		{
			ContextManager = value;
		}
	}
	//public MenuManager HouseManager {
	//	get
	//	{
	//		return ContextManager as MenuManager;
	//	}
	//	set
	//	{
	//		ContextManager = value;
	//	}
	//}

	private bool InProgressSceneSwitch = false;

	public void Awake() {
		
	}

	public void HandleInput(InputPackage p) {
		ContextManager.HandleInput(p);
	}

	public void ReloadLevel() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}


	public void ToggleSoundOn() {
		
	}

	private IEnumerator SwitchScreen(string sceneName, IEnumerator leavingSceneCoroutine) {
		InProgressSceneSwitch = true;
		var currentScene = SceneManager.GetActiveScene();
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

		asyncLoad.allowSceneActivation = false;
		yield return StartCoroutine(leavingSceneCoroutine);
		yield return new WaitUntil(() => asyncLoad.progress >= 0.9f); //when allowsceneactive is false, progress stops at .9f
		asyncLoad.allowSceneActivation = true;
	}
}