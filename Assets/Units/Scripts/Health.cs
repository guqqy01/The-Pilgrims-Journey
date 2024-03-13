using UnityEngine;

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

        public void Init(UnitSO unitSO, TileStats tileStats)
        {
            maxHealth = new BuffableIntStat(unitSO.baseHealth);
            maxHealth.AddBaseBuff("Tile", tileStats.additionalHealth);
            maxHealth.AddPercentBuff("Tile", tileStats.healthMultiplier);

            currentHealth = MaxHealth;
            onHealthChange?.Invoke(currentHealth);
        }

        public void ModifyHealth(int amount)
        {
            currentHealth = Mathf.Clamp(amount + currentHealth, 0, MaxHealth);

            onHealthChange?.Invoke(currentHealth);

            if (currentHealth == 0)
            {
                onDie?.Invoke();
                Destroy(gameObject);
            }

        }
    }
}

