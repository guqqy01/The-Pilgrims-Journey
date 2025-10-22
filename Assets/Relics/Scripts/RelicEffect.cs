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
            Buff buff = buffIncreasePerLevel * Relic.Level;

            OverallBuffManager.Instance.AddStatBuff(Relic.relicSO.name, stat, buff);
        }

        public override void OnLevelChangeRelic()
        {
            Buff buff = buffIncreasePerLevel * Relic.Level;

            OverallBuffManager.Instance.AddStatBuff(Relic.relicSO.name, stat, buff);
        }

        public override void OnRemoveRelic()
        {
            OverallBuffManager.Instance.RemoveStatBuff(Relic.relicSO.name, stat);
        }
    }

    public class IncreaseLuckRelicEffect : RelicEffect
    {
        [SerializeField]
        private Buff luckIncreasePerLevel;

        public override void OnGainRelic()
        {
            Buff buff = luckIncreasePerLevel * Relic.Level;

            OverallBuffManager.Instance.Luck.AddBuff(Relic.relicSO.name, buff);
        }

        public override void OnLevelChangeRelic()
        {
            Buff buff = luckIncreasePerLevel * Relic.Level;

            OverallBuffManager.Instance.Luck.AddBuff(Relic.relicSO.name, buff);
        }

        public override void OnRemoveRelic()
        {
            OverallBuffManager.Instance.Luck.RemoveBuff(Relic.relicSO.name);
        }
    }

    public class IncreaseExperienceRelicEffect : RelicEffect
    {
        [SerializeField]
        private Buff experienceIncreasePerLevel;

        public override void OnGainRelic()
        {
            Buff buff = experienceIncreasePerLevel * Relic.Level;

            OverallBuffManager.Instance.ExperienceGain.AddBuff(Relic.relicSO.name, buff);
        }

        public override void OnLevelChangeRelic()
        {
            Buff buff = experienceIncreasePerLevel * Relic.Level;

            OverallBuffManager.Instance.ExperienceGain.AddBuff(Relic.relicSO.name, buff);
        }

        public override void OnRemoveRelic()
        {
            OverallBuffManager.Instance.ExperienceGain.RemoveBuff(Relic.relicSO.name);
        }
    }
}
