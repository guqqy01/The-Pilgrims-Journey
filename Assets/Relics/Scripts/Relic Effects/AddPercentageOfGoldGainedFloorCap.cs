using UnityEngine;
using System;
namespace Relics
{
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

        internal AddPercentageOfGoldGainedFloorCap(float percentageOfGoldExtra, int numberOfRoundsToGainGold, GoldGainEnum goldGain, Relic relic, Action updateUI = null) : base(relic, updateUI)
        {
            this.percentageOfGoldExtra = percentageOfGoldExtra;
            this.numberOfRoundsToGainGold = numberOfRoundsToGainGold;
            this.goldGain = goldGain;
        }

        public override RelicEffect CreateInstance()
        {
            return new AddPercentageOfGoldGainedFloorCap(percentageOfGoldExtra, numberOfRoundsToGainGold, goldGain, Relic, updateUI);
        }

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

        

        internal enum GoldGainEnum
        {
            GainOrLoss,
            ZeroMin,
        }
    }
}
