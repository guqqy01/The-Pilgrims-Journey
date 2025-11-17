using UnityEngine;
using Units;

namespace Relics
{
    public class StartShieldRelicEffect : RelicEffect
    {
        [SerializeField]
        private int shieldAmountPerLevel;
        [SerializeField]
        private float unitTime;

        private float timeToStop;

        public override string Description
        {
            get
            {
                return $"Gives the units spawned in the first {unitTime} seconds a shield of {shieldAmountPerLevel * Relic.Level}";
            }
        }

        public override void OnPlayEffect()
        {
            timeToStop = Time.time + unitTime;

            UnitSpawningManager.Instance.onUnitSpawned += AddShieldToUnit;
        }

        public override void Reset()
        {
            UnitSpawningManager.Instance.onUnitSpawned -= AddShieldToUnit;
        }

        private void AddShieldToUnit(Unit unit)
        {
            if (Time.time >= timeToStop)
            {
                UnitSpawningManager.Instance.onUnitSpawned -= AddShieldToUnit;
                return;
            }

            Buff shieldBuff = new Buff(
                0f,
                new IntOrFloatObject(0),
                new IntOrFloatObject(shieldAmountPerLevel * Relic.Level)
            );


            unit.Shield.ModifyShield("StartShieldEffect" + GetHashCode(), shieldBuff);
        }
    }
}
