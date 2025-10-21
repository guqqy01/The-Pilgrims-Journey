using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Units;

namespace Relics
{
    public class StartShieldEffect : RelicEffect
    {
        [SerializeField]
        private int shieldAmountPerLevel;
        [SerializeField]
        private float unitTime;

        private float timeToStop;

        public override void OnPlayEffect()
        {
            timeToStop = Time.time + unitTime;

            SpawningManager.Instance.onUnitSpawned += AddShieldToUnit;
        }

        private void AddShieldToUnit(Unit unit)
        {
            if (Time.time >= timeToStop)
            {
                SpawningManager.Instance.onUnitSpawned -= AddShieldToUnit;
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

    public class StartMovementEffect : RelicEffect
    {
        [SerializeField]
        private int movementAmount;
        [SerializeField]
        private float unitTime;
        [SerializeField]
        private float movementBuffTime;

        private float timeToStop;

        public override void OnPlayEffect()
        {
            timeToStop = Time.time + unitTime;

            SpawningManager.Instance.onUnitSpawned += AddMovementBuffToUnit;
        }

        private void AddMovementBuffToUnit(Unit unit)
        {
            if (Time.time >= timeToStop)
            {
                SpawningManager.Instance.onUnitSpawned -= AddMovementBuffToUnit;
                return;
            }

            Buff moveBuff = new Buff(
                0f,
                new IntOrFloatObject(0f),
                new IntOrFloatObject(movementAmount) // consider renaming shieldAmount ? movementAmount
            );
            unit.Movement.ModifySpeedForTime("StartMovementBuffEffect" + GetHashCode(), moveBuff, movementBuffTime);
        }
    }

    public class AddPercentageOfGoldGainedFloorCap : RelicEffect
    {
        [SerializeField]
        private float percentageOfGoldExtra;

        [SerializeField]
        private int numberOfRoundsToGainGold;

        [SerializeField, HideInInspector]
        private int numberOfRoundsRemaining;
        [SerializeField, HideInInspector]
        private int startGold;
        [SerializeField]
        private GoldGainEnum goldGain;

        public int CurrentAmountOfGoldToBeGained => Mathf.CeilToInt((ResourceManager.Instance.Gold - startGold) * percentageOfGoldExtra);

        [HideInInspector]
        public int TotalNumberOfGoldGained;

        private bool reset;

        public override string GetTopText()
        {
            //Debug.Log($"Number of Rounds Remaining: {numberOfRoundsRemaining}");
            return numberOfRoundsRemaining.ToString();
        }
        public override string GetBottomText()
        {
            int goldGained = CurrentAmountOfGoldToBeGained;
            switch (goldGain)
            {
                case GoldGainEnum.GainOrLoss:
                    break;
                case GoldGainEnum.ZeroMin:
                    goldGained = Mathf.Max(0, goldGained);
                    break;
            }

            return goldGained.ToString();
        }

        public override void Init(Relic relic)
        {
            base.Init(relic);
            startGold = ResourceManager.Instance.Gold;
            numberOfRoundsRemaining = numberOfRoundsToGainGold;
            ResourceManager.Instance.onGoldChange += UpdateUITest;
        }

        private void UpdateUITest(int goldAmount)
        {
            updateUI?.Invoke();
        }

        public override void AfterPlayEffect()
        {
            //Debug.Log("AfterPlayEffect");

            numberOfRoundsRemaining--;

            if(numberOfRoundsRemaining <= 0)
            {
                int goldGained = CurrentAmountOfGoldToBeGained;

                switch (goldGain)
                {
                    case GoldGainEnum.GainOrLoss:
                        break;
                    case GoldGainEnum.ZeroMin:
                        goldGained = Mathf.Max(0, goldGained);
                        break;
                }

                TotalNumberOfGoldGained += goldGained;

                ResourceManager.Instance.ModifyGold(goldGained);

                reset = true;
            }

            updateUI?.Invoke();
        }

        public override void Reset()
        {
            if (!reset)
                return;

            startGold = ResourceManager.Instance.Gold;
            numberOfRoundsRemaining = numberOfRoundsToGainGold;

            reset = false;
        }

        private enum GoldGainEnum
        {
            GainOrLoss,
            ZeroMin,
        }
    }

    public class DestroyAfterNumberOfRounds : RelicEffect
    {
        [SerializeField]
        private int numberOfRounds;
        [HideInInspector] 
        public int RemainingNumberOfRounds;
        [SerializeField, HideInInspector]
        private bool countingDown = false;

        public override void Init(Relic relic)
        {
            base.Init(relic);
            RemainingNumberOfRounds = numberOfRounds;
        }

        public override string GetTopText()
        {
            return RemainingNumberOfRounds.ToString();
        }

        public override void OnPlayEffect()
        {
            if (!countingDown)
            {
                countingDown= true;
                RemainingNumberOfRounds = numberOfRounds;
            }
        }

        public override void AfterPlayEffect()
        {
            if (countingDown)
            {
                RemainingNumberOfRounds--;

                if(RemainingNumberOfRounds <= 0)
                {
                    RelicManager.Instance.RemoveRelicAfterRound(Relic);
                }
            }
            updateUI?.Invoke();
        }
    }

    public class DemonItemRelicEffect : RelicEffect
    {


        public override void Init(Relic relic)
        {
            base.Init(relic);
        }

        private void AddBuffsToUnit(Unit unit)
        {
        }
    }
}
