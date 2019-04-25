using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HexGrid : HexGridBasic
{
	public static event System.EventHandler<HexGrid> GridGenerated;

	protected override void Start()
    {
		base.Start();
        GameManager.Instance.LevelManager.Grid = this;
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
}
