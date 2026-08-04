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

        public override string Description(Relic relic)
        {
                return $"This relic will be destroyed in {RemainingNumberOfRounds} rounds.";
        }

        public override string Description(RelicSO relicSO)
        {
            return $"This relic will be destroyed in {RemainingNumberOfRounds} rounds.";
        }

        public DestroyAfterNumberOfRounds(int numberOfRounds, int remainingNumberOfRounds, bool countingDown, Action updateUI = null) : base(updateUI)
        {
            this.numberOfRounds = numberOfRounds;
            RemainingNumberOfRounds = remainingNumberOfRounds;
            this.countingDown = countingDown;
        }

        public override RelicEffect CreateInstance(Relic r)
        {
            return new DestroyAfterNumberOfRounds(numberOfRounds, RemainingNumberOfRounds, countingDown, updateUI);
        }

        public override void OnGainRelic(Relic relic)
        {
            RemainingNumberOfRounds = numberOfRounds;
        }

        public override string GetTopText()
        {
            return RemainingNumberOfRounds.ToString();
        }

        public override void OnPlayEffect(Relic relic)
        {
            if (!countingDown)
            {
                countingDown= true;
                RemainingNumberOfRounds = numberOfRounds;
            }
        }

        public override void AfterPlayEffect(Relic relic)
        {
            if (countingDown)
            {
                RemainingNumberOfRounds--;

                if(RemainingNumberOfRounds <= 0)
                {
                    RelicManager.Instance.RemoveRelicAfterRound(relic);
                }
            }
            updateUI?.Invoke();
        }
    }
}
