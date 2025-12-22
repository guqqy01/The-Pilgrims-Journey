using UnityEngine;
using System;
namespace Relics
{
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

        public DestroyAfterNumberOfRounds(Relic relic, int numberOfRounds, int remainingNumberOfRounds, bool countingDown, Action updateUI = null) : base(relic, updateUI)
        {
            this.numberOfRounds = numberOfRounds;
            RemainingNumberOfRounds = remainingNumberOfRounds;
            this.countingDown = countingDown;
        }

        public override RelicEffect CreateInstance()
        {
            return new DestroyAfterNumberOfRounds(Relic, numberOfRounds, RemainingNumberOfRounds, countingDown, updateUI);
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
}
