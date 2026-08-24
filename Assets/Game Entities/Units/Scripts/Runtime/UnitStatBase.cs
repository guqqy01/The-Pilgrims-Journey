using UnityEngine;
using Tiles;
using Equipment;
using LevelingSystem;

namespace Units
{
    public static class UnitStatBase
    {
        internal static BuffableStat CreateBuffableStat(UnitTile tile, IntOrFloatObject value, UnitStatsEnum stat)
        {
            BuffableStat buffableStat = new BuffableStat(value);

            if (tile != null)
            {
                buffableStat.AddBuff("Tile", tile.GetTileBuff(stat));
            }

            buffableStat.AddBuff("OverallBuff", UnitBuffManager.Instance.GetBuff(stat));

            return buffableStat;
        }
    }
}

