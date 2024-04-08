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

        public void Init(UnitSO unitSO)
        {
            maxHealth = new BuffableIntStat(unitSO.baseHealth);

            currentHealth = MaxHealth;
            onHealthChange?.Invoke(currentHealth);
        }

        public void Init(Tile tile)
        {
            maxHealth = new BuffableIntStat(tile.Unit.baseHealth);

            if (tile != null)
                maxHealth.AddMultiplierBuff("Tile", tile.healthMultiplier);

            List<EquipmentSO> equipmentSOs = tile.Equipment;

            if(equipmentSOs != null)
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

