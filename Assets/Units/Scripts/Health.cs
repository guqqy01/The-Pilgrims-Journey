using UnityEngine;
using Tiles;

namespace Units
{
    public class Health : MonoBehaviour
    {
        private BuffableStat maxHealth;
        public int MaxHealth
        {
            get
            {
                return maxHealth.Value.IntValue;
            }
        }

        public int CurrentHealth { private set; get; } = 5;

        public delegate void OnDie(Health health);
        public OnDie onDie;

        public delegate void OnHealthChange(int currentHealth);
        public OnHealthChange onHealthChange;


        public void Init(BuffableStat healthBuffableStat)
        {
            maxHealth = healthBuffableStat;

            CurrentHealth = MaxHealth;
            onHealthChange?.Invoke(CurrentHealth);
        }

        public void Init(int maxHealth)
        {
            maxHealth = new BuffableStat(maxHealth);

            CurrentHealth = MaxHealth;
            onHealthChange?.Invoke(CurrentHealth);
        }

        public void ModifyHealth(int amount)
        {
            CurrentHealth = Mathf.Clamp(amount + CurrentHealth, 0, MaxHealth);

            onHealthChange?.Invoke(CurrentHealth);

            if (CurrentHealth <= 0)
            {
                onDie?.Invoke(this);

                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            onDie = null;
            onHealthChange = null;
        }
    }
}

