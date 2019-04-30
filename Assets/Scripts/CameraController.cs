using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	private CameraFollowable Following = null;
	private Vector3 Position { set => transform.position = new Vector3(value.x, value.y, -10); }

	private Camera cam;

    void Start() {
		GameManager.Instance.LevelManager.PlacementModeChange += PlacementModeChange;
		cam = GetComponent<Camera>();
    }

    void Update() {
        if(Following != null) {
			Position = Following.Position;
		}
    }

	private void PlacementModeChange(object sender, bool active) {
		LevelManager lm = sender as LevelManager;
		this.Following = lm.Player.GetComponent<CameraFollowable>();
		cam.orthographicSize = 5f;
		Position = Following.Position;
	}
}

public interface CameraFollowable {
	Vector2 Position { get; }
	Vector2 Velocity { get; }
}
