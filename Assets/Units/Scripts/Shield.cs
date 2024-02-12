using UnityEngine;

namespace Units
{
    internal class Shield : MonoBehaviour
    {
        private int baseShield;
        private int additionalShield;
        private float shieldMultiplier = 1;

        public int MaxShield => Mathf.CeilToInt((baseShield + additionalShield) * shieldMultiplier);
        public int CurrentShield { private set; get; } = 5;


        private float baseRegeneration;
        private float regenerationMultiplier = 1;

        public float Regeneration => baseRegeneration * regenerationMultiplier;

        private float baseCooldown;
        private float cooldownMultiplier = 1;

        public float Cooldown => baseCooldown * cooldownMultiplier;

        private float timeToNextRegenPoint;


        public delegate void OnDestroy();
        public OnDestroy onDestroy;

        public delegate void OnShieldChange(int currentShield);
        public OnShieldChange onShieldChange;

        public void Init(UnitSO unitSO, TileStats tileStats)
        {
            baseShield = unitSO.Shield;
            additionalShield = tileStats.additionalShield;
            shieldMultiplier = tileStats.shieldMultiplier;

            baseRegeneration = unitSO.ShieldRegeneration;
            regenerationMultiplier = tileStats.shieldRegenerationMultiplier;

            baseCooldown = unitSO.ShieldCooldown;
            cooldownMultiplier = tileStats.shieldCooldownMultiplier;

            CurrentShield = MaxShield;
            onShieldChange?.Invoke(CurrentShield);
        }


        private void Update()
        {
            if (CurrentShield == MaxShield) return;

            if (Time.time >= timeToNextRegenPoint)
            {
                RegenerateShield(1);
                timeToNextRegenPoint += baseRegeneration;
            }
        }

        public void RegenerateShield(int amount)
        {
            CurrentShield = Mathf.Clamp(CurrentShield + amount, 0, MaxShield);

            onShieldChange?.Invoke(CurrentShield);
        }

        public int DamageShield(int amount)
        {
            int leftOverDmg = Mathf.Max(amount - CurrentShield, 0);

            CurrentShield = Mathf.Clamp(CurrentShield - amount, 0, MaxShield);

            onShieldChange?.Invoke(CurrentShield);

            if (amount > 0) timeToNextRegenPoint = Time.time + baseCooldown;

            if (CurrentShield == 0)
                onDestroy?.Invoke();

            return leftOverDmg;
        }

    }
}


