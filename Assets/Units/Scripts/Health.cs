using UnityEngine;
using System.Collections.Generic;
using Tiles;
using Equipment;


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

    public class Health : UnitStatBase
    {
        private BuffableStat maxHealth;
        public int MaxHealth
        {
            get
            {
                var stat = maxHealth.Value;

                // If this stat is int-based, use IntValue
                if (stat.IsInt)
                    return stat.IntValue;

                // If it’s float-based (because of buffs), round it
                return Mathf.RoundToInt(stat.FloatValue);
            }
        }

        public int CurrentHealth { private set; get; } = 5;

        public delegate void OnDie();
        public OnDie onDie;

        public delegate void OnHealthChange(int currentHealth);
        public OnHealthChange onHealthChange;

        public void Init(UnitSO unitSO)
        {
            maxHealth = new BuffableStat(unitSO.Health);

            CurrentHealth = MaxHealth;
            onHealthChange?.Invoke(CurrentHealth);
        }

        public void Init(Tile tile)
        {
            maxHealth = CreateBuffableStat(tile, tile.UnitSO.Health, UnitStatsEnum.Health);

            CurrentHealth = MaxHealth;
            onHealthChange?.Invoke(CurrentHealth);
        }

        public void ModifyHealth(int amount)
        {
            CurrentHealth = Mathf.Clamp(amount + CurrentHealth, 0, MaxHealth);

            onHealthChange?.Invoke(CurrentHealth);

            if (CurrentHealth <= 0)
            {
                onDie?.Invoke();
                SpawningManager.Instance.SpawnedUnits.Remove(GetComponent<Unit>());
                Destroy(gameObject);
            }

        }
    }
}

