using LevelingSystem;
using System;
namespace Relics
{
    [AbstractTitle("Stat Increase/Experience")]
    public class IncreaseExperienceRelicEffect : StatIncreaseRelicEffect
    {
        public override string StatString => "Experience Gain";

        public override string Description
        {
            get
            {
                return $"{StatString}: {(buffIncreasePerLevel * Relic.Level).GetSimpleDescription()}";
            }
        }

        public IncreaseExperienceRelicEffect(Relic relic, Buff buffIncreasePerLevel, Action updateUI = null) : base(relic, buffIncreasePerLevel, updateUI)
        {
        }

        public override RelicEffect CreateInstance()
        {
            return new IncreaseExperienceRelicEffect(Relic, buffIncreasePerLevel, updateUI);
            
        }

        public override void OnGainRelic()
        {
            Buff buff = buffIncreasePerLevel * Relic.Level;

            OverallStatsManager.Instance.AddBuff(Relic.relicSO.name, buff, OverallStats.ExperienceGain);
        }

        public override void OnLevelChangeRelic()
        {
            Buff buff = buffIncreasePerLevel * Relic.Level;

            OverallStatsManager.Instance.AddBuff(Relic.relicSO.name, buff, OverallStats.ExperienceGain);
        }

        public override void OnRemoveRelic()
        {
            OverallStatsManager.Instance.RemoveBuff(Relic.relicSO.name, OverallStats.ExperienceGain);
        }
    }
}
