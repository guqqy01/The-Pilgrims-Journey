using LevelingSystem;

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

        public override void OnGainRelic()
        {
            Buff buff = buffIncreasePerLevel * Relic.Level;

            OverallBuffManager.Instance.ExperienceGain.AddBuff(Relic.relicSO.name, buff);
        }

        public override void OnLevelChangeRelic()
        {
            Buff buff = buffIncreasePerLevel * Relic.Level;

            OverallBuffManager.Instance.ExperienceGain.AddBuff(Relic.relicSO.name, buff);
        }

        public override void OnRemoveRelic()
        {
            OverallBuffManager.Instance.ExperienceGain.RemoveBuff(Relic.relicSO.name);
        }
    }
}
