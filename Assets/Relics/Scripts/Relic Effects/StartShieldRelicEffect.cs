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

        public override string Description
        {
            get
            {
                if (Relic == null)
                    return "";

                return $"Gives the units spawned in the first {unitTime} seconds a shield of {shieldAmountPerLevel * Relic.Level}";
            }
        }

        public override RelicEffect CreateInstance()
        {
            return new StartShieldRelicEffect(Relic, shieldAmountPerLevel, unitTime, updateUI);
        }

        public StartShieldRelicEffect(Relic relic, int shieldAmountPerLevel, float unitTime, Action updateUI = null) : base(relic, updateUI)
        {
            this.shieldAmountPerLevel = shieldAmountPerLevel;
            this.unitTime = unitTime;
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
            if(Relic == null)
            {
                return;
            }

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
