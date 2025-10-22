using System;
using UnityEngine;
namespace Relics
{

    [Serializable]
    public abstract class RelicEffect
    {
        public Relic Relic { private set; get; }

        public delegate void UpdateUI();
        public UpdateUI updateUI;

        public void Init(Relic relic)
        {
            Relic = relic;
            OnGainRelic();
        }

        public RelicEffect Clone
        {
            get
            {
                return (RelicEffect)MemberwiseClone();
            }
        }

        public virtual void OnGainRelic() { }
        public virtual void OnRemoveRelic() { }
        public virtual void OnLevelChangeRelic() { }

        public virtual string GetBottomText() { return ""; }
        public virtual string GetTopText() { return ""; }
        public virtual void OnPlayEffect() { }
        public virtual void AfterPlayEffect() { }
        public virtual void Reset() { }
    }

    public class UnitStatIncreaseRelicEffect : RelicEffect
    {
        [SerializeField]
        private UnitStatsEnum stat;

        [SerializeField]
        private Buff buffIncreasePerLevel;

        public override void OnGainRelic()
        {
            buffIncreasePerLevel *= Relic.Level;

            OverallBuffManager.Instance.AddStatBuff(Relic.relicSO.name, stat, buffIncreasePerLevel);
        }

        public override void OnLevelChangeRelic()
        {
            buffIncreasePerLevel *= Relic.Level;

            OverallBuffManager.Instance.AddStatBuff(Relic.relicSO.name, stat, buffIncreasePerLevel);
        }

        public override void OnRemoveRelic()
        {
            OverallBuffManager.Instance.RemoveStatBuff(Relic.relicSO.name, stat);
        }
    }
}
