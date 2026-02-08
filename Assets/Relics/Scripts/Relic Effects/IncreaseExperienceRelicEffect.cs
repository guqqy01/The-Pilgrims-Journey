using LevelingSystem;
using System;
using Debug = UnityEngine.Debug;
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
                return $"{StatString}: {(buffIncreasePerLevel * Relic.Level).GetSignedSimpleDescription()}";
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

        public override void OnLevelChangeRelic(int currentLevel)
        {
            if (Relic == null)
            {
                Debug.LogError($"Relic empty in relicEffect: {this}");
                return;
            }

            Buff buff = buffIncreasePerLevel * currentLevel;

            OverallStatsManager.Instance.AddBuff(Relic.relicSO.name, buff, OverallStats.ExperienceGain);
        }

        public override void OnRemoveRelic()
        {
            OverallStatsManager.Instance.RemoveBuff(Relic.relicSO.name, OverallStats.ExperienceGain);
        }
    }
}
