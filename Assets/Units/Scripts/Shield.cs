using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Units
{
    internal class Shield : MonoBehaviour
    {
        private BuffableIntStat shield;
        public int MaxShield => shield.Value;
        public int CurrentShield { private set; get; } = 5;


        private BuffableFloatStat regeneration;
        public float Regeneration => regeneration.Value;


        private BuffableFloatStat cooldown;
        public float Cooldown => cooldown.Value;


        private float timeToNextRegenPoint;


        public delegate void OnDestroy();
        public OnDestroy onDestroy;

        public delegate void OnShieldChange(int currentShield);
        public OnShieldChange onShieldChange;

        public void Init(UnitSO unitSO, TileStats tileStats)
        {
            shield = new BuffableIntStat(unitSO.Shield);
            shield.AddBaseBuff("Tile", tileStats.additionalShield);
            shield.AddPercentBuff("Tile", tileStats.shieldMultiplier);
            CurrentShield = MaxShield;

            regeneration = new BuffableFloatStat(unitSO.ShieldRegeneration);
            regeneration.AddPercentBuff("Tile", tileStats.shieldRegenerationMultiplier);

            cooldown = new BuffableFloatStat(unitSO.ShieldCooldown);
            cooldown.AddPercentBuff("Tile", tileStats.shieldCooldownMultiplier);

            onShieldChange?.Invoke(CurrentShield);
        }


        private void Update()
        {
            if (CurrentShield == MaxShield) return;

            if (Time.time >= timeToNextRegenPoint)
            {
                RegenerateShield(1);
                timeToNextRegenPoint += Regeneration;
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

            if (amount > 0) timeToNextRegenPoint = Time.time + Cooldown;

            if (CurrentShield == 0)
                onDestroy?.Invoke();

            return leftOverDmg;
        }

    }
}


