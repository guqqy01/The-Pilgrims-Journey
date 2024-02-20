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

    public UnitSpawningInfo(Dictionary<Tile, int> tilesAndSize, UnitSpawningInfo previous, int position)
    {
        unitSpawnInfo = new List<UnitSpawnInfo>();

        foreach (KeyValuePair<Tile, int> kvp in tilesAndSize)
        {
            UnitSpawnInfo info = this[kvp.Key];

            if (info != null)
            {
                info.AddSpawnRate(kvp.Value);
            }
            else
            {
                unitSpawnInfo.Add(new UnitSpawnInfo(kvp.Key, previous != null && previous.ContainsTile(kvp.Key) ? previous[kvp.Key].nextSpawnTime : Time.time, kvp.Value));
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
