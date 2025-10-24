using System.Collections;
using System.Collections.Generic;
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

            SpawningManager.Instance.onUnitSpawned += AddShieldToUnit;
        }

        public override void Reset()
        {
            SpawningManager.Instance.onUnitSpawned -= AddShieldToUnit;
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
                new IntOrFloatObject(movementAmountPerLevel) // consider renaming shieldAmount ? movementAmount
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

        public int CurrentAmountOfGoldToBeGained 
        {
            get
            {
                int goldGained = Mathf.CeilToInt((ResourceManager.Instance.Gold - startGold) * percentageOfGoldExtra);

                switch (goldGain)
                {
                    case GoldGainEnum.GainOrLoss:
                        break;
                    case GoldGainEnum.ZeroMin:
                        goldGained = Mathf.Max(0, goldGained);
                        break;
                }

                return goldGained;
            }
            
        }

        [HideInInspector]
        public int TotalNumberOfGoldGained;

        private bool reset;

        public override string Description
        {
            get
            {
                return $"Grants you {percentageOfGoldExtra}% of the gold difference between {numberOfRoundsToGainGold} rounds. Current Amount of gold to be gained in {numberOfRoundsRemaining} rounds is {CurrentAmountOfGoldToBeGained}";
            }
        }

        public override string GetTopText()
        {
            //Debug.Log($"Number of Rounds Remaining: {numberOfRoundsRemaining}");
            return numberOfRoundsRemaining.ToString();
        }
        public override string GetBottomText()
        {
            int goldGained = CurrentAmountOfGoldToBeGained;

            return goldGained.ToString();
        }

        public override void OnGainRelic()
        {
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

        public override string Description
        {
            get
            {
                return $"This relic will be destroyed in {RemainingNumberOfRounds} rounds.";
            }
        }

        public override void OnGainRelic()
        {
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
        public override string Description
        {
            get
            {
                return $"Not implemented";
            }
        }

        public override void OnGainRelic()
        {

        }

        private void AddBuffsToUnit(Unit unit)
        {
        }
    }
}
