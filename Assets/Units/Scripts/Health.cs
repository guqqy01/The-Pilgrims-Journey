using UnityEngine;
using System.Collections.Generic;

namespace Units
{
    internal class Health : MonoBehaviour
    {
        private BuffableIntStat maxHealth;
        public int MaxHealth => maxHealth.Value;
        public int currentHealth { private set; get; } = 5;

        public delegate void OnDie();
        public OnDie onDie;

        public delegate void OnHealthChange(int currentHealth);
        public OnHealthChange onHealthChange;

        public void Init(UnitSO unitSO, TileStats tileStats, List<EquipmentSO> equipmentSOs)
        {
            maxHealth = new BuffableIntStat(unitSO.baseHealth);
            maxHealth.AddBaseBuff("Tile", tileStats.additionalHealth);
            maxHealth.AddMultiplierBuff("Tile", tileStats.healthMultiplier);

            for (int i = 0; i < equipmentSOs.Count; i++)
            {
                if (equipmentSOs[i] == null)
                    continue;

                maxHealth.AddBaseBuff("Equipment" + (i + 1).ToString(), equipmentSOs[i].IntBaseValue(UnitIntStats.Health));
                maxHealth.AddMultiplierBuff("Equipment" + (i + 1).ToString(), equipmentSOs[i].IntMultiplierValue(UnitIntStats.Health));
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
                Destroy(gameObject);
            }

        }
    }
}

