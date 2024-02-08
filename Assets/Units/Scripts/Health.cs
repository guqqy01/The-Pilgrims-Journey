using UnityEngine;

namespace Units
{
    internal class Health : MonoBehaviour
    {
        private int baseHealth;
        private int additionalHealth;
        private float multiplier = 1;
        public int MaxHealth => Mathf.CeilToInt((baseHealth + additionalHealth) * multiplier);
        public int currentHealth { private set; get; } = 5;

        public delegate void OnDie();
        public OnDie onDie;

        public delegate void OnHealthChange(int currentHealth);
        public OnHealthChange onHealthChange;

        public void Init(UnitSO unitSO, TileStats tileStats)
        {
            baseHealth = unitSO.baseHealth;
            additionalHealth = tileStats.additionalHealth;
            multiplier = tileStats.healthMultiplier;

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

