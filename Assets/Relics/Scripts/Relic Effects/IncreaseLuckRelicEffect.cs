using LevelingSystem;

namespace Relics
{
    [AbstractTitle("Stat Increase/Luck")]
    public class IncreaseLuckRelicEffect : StatIncreaseRelicEffect
    {
        public override string StatString => "Luck";

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

            OverallStatsManager.Instance.AddBuff(Relic.relicSO.name, buff, OverallStats.Luck);
        }

        public override void OnLevelChangeRelic()
        {
            Buff buff = buffIncreasePerLevel * Relic.Level;

            OverallStatsManager.Instance.AddBuff(Relic.relicSO.name, buff, OverallStats.Luck);
        }

        public override void OnRemoveRelic()
        {
            OverallStatsManager.Instance.RemoveBuff(Relic.relicSO.name, OverallStats.Luck);
        }
    }
}
