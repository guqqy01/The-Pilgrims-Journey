using LevelingSystem;
using System;
using Debug = UnityEngine.Debug;
namespace Relics
{
    [AbstractTitle("Stat Increase/Experience")]
    public class IncreaseExperienceRelicEffect : StatIncreaseRelicEffect
    {
        public override string StatString => "Experience Gain";

        public override string Description(Relic r)
        {
            return $"{StatString}: {(buffIncreasePerLevel * r.Level).GetSignedSimpleDescription()}";
        }

        public override string Description(RelicSO relicSO)
        {
            return $"{StatString} increase per Level: {buffIncreasePerLevel.GetSignedSimpleDescription()}";
        }

        public IncreaseExperienceRelicEffect() : base(default, default) { }

        public IncreaseExperienceRelicEffect(Buff buffIncreasePerLevel, Action updateUI = null) : base(buffIncreasePerLevel, updateUI)
        {
        }

        public override RelicEffect CreateInstance(Relic relic)
        {
            return new IncreaseExperienceRelicEffect(buffIncreasePerLevel, updateUI);
            
        }

        public override void OnGainRelic(Relic relic)
        {
            Buff buff = buffIncreasePerLevel * relic.Level;

            OverallStatsManager.Instance.AddBuff(relic.relicSO.name, buff, OverallStats.ExperienceGain);
        }

        public override void OnLevelChangeRelic(Relic relic, int currentLevel)
        {
            Buff buff = buffIncreasePerLevel * currentLevel;

            OverallStatsManager.Instance.AddBuff(relic.relicSO.name, buff, OverallStats.ExperienceGain);
        }

        public override void OnRemoveRelic(Relic relic)
        {
            OverallStatsManager.Instance.RemoveBuff(relic.relicSO.name, OverallStats.ExperienceGain);
        }
    }
}
