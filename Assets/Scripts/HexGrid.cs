using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HexGrid : HexGridBasic
{
	public static event System.EventHandler<HexGrid> GridGenerated;

	public int GetNumReachableEmptyCells() {
		return cellInfo.Values.Count(c => c.Cell is EmptyCell && c.Reachable);
	}

	public void Init(LevelData data = null, Line linePrefab = null) {
		if(data) {
			InitGrid(data?.Seed);
			//towers
			foreach(TowerData td in data.Towers) {
				if (td.IsSwitch) {
					Switch.CreateInstance().CreateFromData(td, this);
				}
				else {
					Tower.CreateInstance().CreateFromData(td, this);
				}
			}
			//lines
			foreach(LineData ld in data.Lines) {
				Line l = Instantiate(linePrefab);
				l.CreateFromData(ld, this);
			}
		}
		else {
			InitGrid(null);
		}

		GridGenerated?.Invoke(this, this);
	}

	public void TryGetTowerLocation(Vector3 position, Tower tower)
    {
        HexCoordinates hex = TryGetHexCoordinateFromWorldPosition(position, out bool success);
        if (success)
        {
            HexInfo info = cellInfo[hex];
            success = !info.Occupied && info.Reachable;
            if (success)
            {
                HexInfo closestHex = info;

                List<HexInfo> available =
                    hex.GetStraightLinesOfLength(2).Where(h => cellInfo.ContainsKey(h) && cellInfo[h].Filled).Select(n => cellInfo[n]).ToList();

                int minHexDistance = 100;
                float minPhysicalDistance = 100;
                foreach (HexInfo i in available)
                {
                    int dist = HexCoordinates.Distance(hex, i.Coordinates);
                    if (dist < minHexDistance)
                    {
                        float pDist = Vector2.Distance(i.PhysicalCoordinates, position);
                        minHexDistance = dist;
                        minPhysicalDistance = pDist;
                        closestHex = i;
                    }
                    else if (dist == minHexDistance)
                    {
                        float pDist = Vector2.Distance(i.PhysicalCoordinates, position);
                        if (pDist < minPhysicalDistance)
                        {
                            minPhysicalDistance = pDist;
                            closestHex = i;
                        }
                    }
                }

                success = minHexDistance < 100;
                if (success)
                {
                    Vector3 diff = closestHex.PhysicalCoordinates - info.PhysicalCoordinates;
                    Vector3 posRot = info.PhysicalCoordinates + (diff / 2f) + (0.5f * (minHexDistance - 1) * InnerRadius) * diff.normalized;
                    posRot.z = Vector2.SignedAngle(Vector2.down, diff);

                    tower.Height = minHexDistance;
                    tower.gameObject.SetActive(true);
                    tower.transform.position = new Vector3(posRot.x, posRot.y, -0.5f);
                    tower.transform.rotation = Quaternion.Euler(0, 0, posRot.z);
                    tower.GridPosition = info.Coordinates;
                }
            }
        }

        if (!success)
        {
            tower.gameObject.SetActive(false);
        }
    }

    public void PlaceTower(Tower tower)
    {
        if (!cellInfo.ContainsKey(tower.GridPosition))
        {
            Vector3Int xy = tower.GridPosition.GetRepresentationalCoordinates();
            cellInfo[tower.GridPosition] = new HexInfo(xy.x, xy.y, metrics);
        }
        cellInfo[tower.GridPosition].TowerHead = tower;
    }

    public Vector3 TryGetTowerPlacement(Vector3 position, out bool success, out int distance)
    {
        distance = -1; // just so out doesn't comlpain about not defining direction
        HexCoordinates hex = TryGetHexCoordinateFromWorldPosition(position, out success);
        if (!success) return Vector3.zero;
        HexInfo info = cellInfo[hex];
        success = !info.Filled;
        if (!success) return Vector3.zero;

        HexInfo closestHex = info;
        float minDistance = 1001;

        List<HexInfo> available =
            hex.GetStraightLinesOfLength(2).Where(h => cellInfo.ContainsKey(h) && cellInfo[h].Filled).Select(n => cellInfo[n]).ToList();

        foreach (HexInfo i in available)
        {
            float dist = Vector2.Distance(i.PhysicalCoordinates, position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestHex = i;
            }
        }

        success = minDistance < 1000;

        distance = HexCoordinates.Distance(hex, closestHex.Coordinates);
        Vector3 diff = closestHex.PhysicalCoordinates - info.PhysicalCoordinates;
        Vector3 posRot = info.PhysicalCoordinates + (diff / 2f) + (0.5f * (distance - 1) * InnerRadius) * diff.normalized;
        posRot.z = Vector2.SignedAngle(Vector2.down, diff);
        return posRot;
    }

	public void CreateSwitches(int numSwitches = 4) {
		// divide entire grid up into 9 zones in a 3x3 formation,
		// remove the zones with the start and end point
		// select `numSwitches` zones to put switches in

		int leftZoneBoundaryX = width / 3;
		int rightZoneBoundaryX = 2 * width / 3;
		int topZoneBoundaryY = 2 * height / 3;
		int bottomZoneBoundaryY = height / 3;

		int getZone(Vector3Int v) {
			int z = 0;
			if(v.x > rightZoneBoundaryX) {
				z += 2;
			} else if ( v.x >  leftZoneBoundaryX) {
				z += 1;
			}
			if(v.y > topZoneBoundaryY) {
				z += 6; // 3 * 2
			}
			else if(v.y > bottomZoneBoundaryY) {
				z += 3;
			}
			return z;
		};

		int startZone = getZone(StartingPoint.Coordinates.GetRepresentationalCoordinates());
		int endZone = getZone(EndingPoint.Coordinates.GetRepresentationalCoordinates());
		List<int> availableZones = Enumerable.Range(0, 9).Where(e => e != startZone && e != endZone).ToList();

		List<int> switchZones = new List<int>();
		for(int i = 0; i < numSwitches; i++) {
			int num = UnityEngine.Random.Range(0, availableZones.Count);
			switchZones.Add(availableZones[num]);
			availableZones.RemoveAt(num);
		}

		int border = System.Math.Min(height, width) / 10;

		HexCoordinates getRandomHexInZone(int zone) {
			int xZone = zone % 3;
			int yZone = zone / 3;

			int xGridPos, yGridPos;
			switch(xZone) {
				default:
				case 0:
					xGridPos = UnityEngine.Random.Range(0, leftZoneBoundaryX - border);
					break;
				case 1:
					xGridPos = UnityEngine.Random.Range(leftZoneBoundaryX + border, rightZoneBoundaryX - border);
					break;
				case 2:
					xGridPos = UnityEngine.Random.Range(rightZoneBoundaryX + border, width);
					break;
			}
			switch (yZone) {
				default:
				case 0:
					yGridPos = UnityEngine.Random.Range(0, bottomZoneBoundaryY - border);
					break;
				case 1:
					yGridPos = UnityEngine.Random.Range(bottomZoneBoundaryY + border, topZoneBoundaryY - border);
					break;
				case 2:
					yGridPos = UnityEngine.Random.Range(topZoneBoundaryY + border, height);
					break;
			}

			return HexCoordinates.FromRepresentationalCoordinates(xGridPos, yGridPos);
		}

		for(int i = 0; i < switchZones.Count; i++) {
			bool foundPosition = false;
			int attempts = 0;
			int zone = switchZones[i];
			while(!foundPosition) {
				attempts = 0;
				do {
					HexCoordinates h = getRandomHexInZone(zone);
					if(cellInfo[h].Reachable && cellInfo[h].NumTouchedWalls > 0) {
						Switch s = Switch.CreateInstance();
						s.GridPosition = h;
						s.Place(this);
						foundPosition = true;
						Debug.Log($"Found in zone {zone} in {attempts} attempts");
					}
					attempts++;
				} while (!foundPosition && attempts < 10);

				// this zone can't find a place, search another zone for position
				if(!foundPosition) {
					Debug.Log($"Failed to find in zone {zone}");
					int num = UnityEngine.Random.Range(0, availableZones.Count);
					zone = availableZones[num];
					availableZones.RemoveAt(num);
				}
			}
		}
	}
}
