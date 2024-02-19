using System.Collections.Generic;
using UnityEngine;
using Units;

public class UnitSpawningInfo
{
    public List<UnitSpawnInfo> unitSpawnInfo = new List<UnitSpawnInfo>();

    public int Count => unitSpawnInfo.Count;

    public UnitSpawnInfo this[int i]
    {
        get
        {
            return unitSpawnInfo[i];
        }
    }

    public UnitSpawnInfo this[Tile tile]
    {
        get
        {
            for (int i = 0; i < unitSpawnInfo.Count; i++)
            {
                if (unitSpawnInfo[i].tile == tile)
                {
                    return unitSpawnInfo[i];
                }
            }

            return null;
        }
    }

    public UnitSpawningInfo(List<Tile> tiles, UnitSpawningInfo previous, Dictionary<Tile, List<Vector2Int>> tileAndSpaces, int position)
    {
        unitSpawnInfo = new List<UnitSpawnInfo>();

        foreach (Tile tile in tiles)
        {
            UnitSpawnInfo info = this[tile];

            float multiplier = 0;
            foreach (Vector2Int v2int in tileAndSpaces[tile])
            {
                if (v2int.x - position == 0)
                    multiplier++;
            }

            if (info != null)
            {
                info.AddSpawnRate(multiplier);
            }
            else
            {
                unitSpawnInfo.Add(new UnitSpawnInfo(tile, previous != null && previous.ContainsTile(tile) ? previous[tile].nextSpawnTime : Time.time, multiplier));
            }
        }
    }

    public bool ContainsTile(Tile tile)
    {
        foreach(UnitSpawnInfo info in unitSpawnInfo)
        {
            if (info.tile == tile)
                return true;
        }

        return false;
    }
}
