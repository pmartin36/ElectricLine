using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerData: ScriptableObject {
	public float MaxEnergy { get; set; }
	public float JumpHeight { get; set; }
	public float DashTime { get; set; }

	public PlayerData(float maxEnergy, float jumpHeight, float dashTime) {
		MaxEnergy = maxEnergy;
		JumpHeight = jumpHeight;
		DashTime = dashTime;
	}
}

