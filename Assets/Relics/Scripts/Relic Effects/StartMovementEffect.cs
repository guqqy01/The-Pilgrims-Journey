using UnityEngine;
using Units;
using System;
namespace Relics
{
    public class StartMovementEffect : RelicEffect
    {
        [SerializeField]
        private float movementAmountPerLevel;
        [SerializeField]
        private float unitTime;
        [SerializeField]
        private float movementBuffTime;

        private float timeToStop;

        public override string Description
        {
            get
            {
                return $"Gives the units spawned in the first {unitTime} seconds a speed boost of {movementAmountPerLevel * Relic.Level}";
            }
        }

        public override RelicEffect CreateInstance()
        {
            return new StartMovementEffect(Relic, movementAmountPerLevel, unitTime, movementBuffTime, updateUI);
        }

        public StartMovementEffect(Relic relic, float movementAmountPerLevel, float unitTime, float movementBuffTime, Action updateUI = null) : base(relic, updateUI)
        {
            this.movementAmountPerLevel = movementAmountPerLevel;
            this.unitTime = unitTime;
            this.movementBuffTime = movementBuffTime;
        }

        public override void OnPlayEffect()
        {
            timeToStop = Time.time + unitTime;

            UnitSpawningManager.Instance.onUnitSpawned += AddMovementBuffToUnit;
        }

        private void AddMovementBuffToUnit(Unit unit)
        {
            if (Time.time >= timeToStop)
            {
                UnitSpawningManager.Instance.onUnitSpawned -= AddMovementBuffToUnit;
                return;
            }

            Buff moveBuff = new Buff(
                0f,
                new IntOrFloatObject(0f),
                new IntOrFloatObject(movementAmountPerLevel) // consider renaming shieldAmount ? movementAmount
            );
            unit.Movement.ModifySpeedForTime("StartMovementBuffEffect" + GetHashCode(), moveBuff, movementBuffTime);
        }

        
    }
}
