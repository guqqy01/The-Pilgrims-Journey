using UnityEngine;
using System.Collections.Generic;
using Tiles;
using Equipment;


namespace Units
{
    public class UnitBuffableStat : MonoBehaviour
    {
        private BuffableStat buffableStat;

        public object Stat => buffableStat.Value;

        public void Init(UnitSO unitSO, UnitStatsEnum unitStat)
        {
            switch (unitStat)
            {
                case UnitStatsEnum.Health:
                    buffableStat = new BuffableStat();
                    break;
                case UnitStatsEnum.Shield:
                    buffableStat = new BuffableStat();
                    break;
                case UnitStatsEnum.ShieldRegeneration:
                    buffableStat = new BuffableStat();
                    break;
                case UnitStatsEnum.ShieldCooldown:
                    buffableStat = new BuffableStat();
                    break;
                case UnitStatsEnum.MovementSpeed:
                    buffableStat = new BuffableStat();
                    break;
                case UnitStatsEnum.SpawnRate:
                    buffableStat = new BuffableStat();
                    break;
            }
        }

        public void Buff(string buffName, Buff buff)
        {
            buffableStat.AddBuff(buffName, buff);
        }


    }

    public class Health : MonoBehaviour
    {
        private BuffableStat maxHealth;
        public int MaxHealth => (int)maxHealth.Value.Value;
        public int currentHealth { private set; get; } = 5;

        public delegate void OnDie();
        public OnDie onDie;

        public delegate void OnHealthChange(int currentHealth);
        public OnHealthChange onHealthChange;

        public void Init(UnitSO unitSO)
        {
            maxHealth = new BuffableStat(unitSO.Health);

            currentHealth = MaxHealth;
            onHealthChange?.Invoke(currentHealth);
        }

        public void Init(Tile tile)
        {
            maxHealth = new BuffableStat(tile.Unit.Health);

            Buff buff = UpgradeManager.Instance.GetBuff(UnitStatsEnum.Health);

            maxHealth.AddBuff("UpgradeManager", buff);

            if (tile != null)
            {
                maxHealth.AddBuff("Tile", tile.GetTileBuff(UnitStatsEnum.Health));
            }


            EquipmentSO[] equipmentSOs = tile.Equipment;

            if(equipmentSOs != null)
            {
                for (int i = 0; i < equipmentSOs.Length; i++)
                {
                    if (equipmentSOs[i] == null)
                        continue;

                    if(equipmentSOs[i].TryGetBuff(UnitStatsEnum.Health, out Buff outBuff))
                    {
                        maxHealth.AddBuff("Equipment" + (i + 1).ToString(), outBuff);
                    }
                }
            }
                
                
            currentHealth = MaxHealth;
            onHealthChange?.Invoke(currentHealth);
        }

        public void ModifyHealth(int amount)
        {
            currentHealth = Mathf.Clamp(amount + currentHealth, 0, MaxHealth);

            onHealthChange?.Invoke(currentHealth);

            if (currentHealth <= 0)
            {
                onDie?.Invoke();
                SpawningManager.Instance.SpawnedUnits.Remove(GetComponent<Unit>());
                Destroy(gameObject);
            }

        }
    }
}

