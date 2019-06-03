using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Data/Level Data")]
public class LevelData : ScriptableObject {
	public int Seed;
	public TowerData[] Towers;
	public LineData[] Lines;
}

