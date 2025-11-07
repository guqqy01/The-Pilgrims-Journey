using UnityEngine;
using Tiles;

namespace Units
{

    public class Health : UnitStatBase
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

