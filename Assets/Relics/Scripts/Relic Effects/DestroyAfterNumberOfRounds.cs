using UnityEngine;

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
