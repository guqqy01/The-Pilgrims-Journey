using UnityEngine;
using Tiles;
using Equipment;
using LevelingSystem;

namespace Units
{
    public abstract class UnitStatBase : MonoBehaviour
    {
        protected BuffableStat CreateBuffableStat(Tile tile, IntOrFloatObject value, UnitStatsEnum stat)
        {
            BuffableStat buffableStat = new BuffableStat(value);

            Buff buff = UpgradeManager.Instance.GetBuff(stat);

            buffableStat.AddBuff("UpgradeManager", buff);

            if (tile != null)
            {
                buffableStat.AddBuff("Tile", tile.GetTileBuff(stat));
            }

            EquipmentSO[] equipmentSOs = tile.Equipment;

            if (equipmentSOs != null)
            {
                for (int i = 0; i < equipmentSOs.Length; i++)
                {
                    if (equipmentSOs[i] == null)
                        continue;

                    if (equipmentSOs[i].TryGetBuff(stat, out Buff outBuff))
                    {
                        buffableStat.AddBuff("Equipment" + (i + 1).ToString(), outBuff);
                    }
                }
            }

            buffableStat.AddBuff("OverallBuff", OverallBuffManager.Instance.GetBuff(stat));

            return buffableStat;
        }
    }
}

