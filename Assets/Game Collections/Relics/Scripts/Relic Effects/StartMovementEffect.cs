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

        public override string Description(Relic relic)
        {
                return $"Gives the units spawned in the first {unitTime} seconds a speed boost of {movementAmountPerLevel * relic.Level}";
        }

        public override string Description(RelicSO relicSO)
        {
            return $"Gives the units spawned in the first {unitTime} seconds a speed boost of ({movementAmountPerLevel}) per level";
        }

        public override RelicEffect CreateInstance(Relic relic)
        {
            return new StartMovementEffect(movementAmountPerLevel, unitTime, movementBuffTime, updateUI);
        }

        public StartMovementEffect(float movementAmountPerLevel, float unitTime, float movementBuffTime, Action updateUI = null) : base(updateUI)
        {
            this.movementAmountPerLevel = movementAmountPerLevel;
            this.unitTime = unitTime;
            this.movementBuffTime = movementBuffTime;
        }

        public override void OnPlayEffect(Relic relic)
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
                BuffType.Additional,
                0f,
                new IntOrFloatObject(0f),
                new IntOrFloatObject(movementAmountPerLevel) // consider renaming shieldAmount ? movementAmount
            );
            unit.Movement.ModifySpeedForTime("StartMovementBuffEffect" + GetHashCode(), moveBuff, movementBuffTime);
        }

        
    }
}
