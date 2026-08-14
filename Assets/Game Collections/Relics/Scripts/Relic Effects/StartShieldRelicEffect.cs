using UnityEngine;
using Units;
using System;
namespace Relics
{
    public class StartShieldRelicEffect : RelicEffect
    {
        [SerializeField]
        private int shieldAmountPerLevel;
        [SerializeField]
        private float unitTime;

        private float timeToStop;

        private int relicLevel;

        public override string Description(Relic relic)
        {
            return $"Gives the units spawned in the first {unitTime} seconds a shield of {shieldAmountPerLevel * relic.Level}";
        }

        public override string Description(RelicSO relicSO)
        {
            return $"Gives the units spawned in the first {unitTime} seconds a shield of ({shieldAmountPerLevel}) per level";
        }

        public override RelicEffect CreateInstance(Relic relic)
        {
            return new StartShieldRelicEffect(shieldAmountPerLevel, unitTime, updateUI);
        }

        public StartShieldRelicEffect() : base(default) { }

        public StartShieldRelicEffect(int shieldAmountPerLevel, float unitTime, Action updateUI = null) : base(updateUI)
        {
            this.shieldAmountPerLevel = shieldAmountPerLevel;
            this.unitTime = unitTime;
        }

        public override void OnPlayEffect(Relic relic)
        {
            timeToStop = Time.time + unitTime;
            relicLevel = relic.Level;
            UnitSpawningManager.Instance.onUnitSpawned += AddShieldToUnit;
        }

        public override void Reset(Relic relic)
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
                BuffType.Additional | BuffType.Multiplier | BuffType.Base,
                0f,
                new IntOrFloatObject(0),
                new IntOrFloatObject(shieldAmountPerLevel * relicLevel)
            );


            unit.Shield.ModifyShield("StartShieldEffect" + GetHashCode(), shieldBuff);
        }

        
    }
}
