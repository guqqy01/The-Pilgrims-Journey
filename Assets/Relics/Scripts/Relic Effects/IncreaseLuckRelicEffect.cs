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

            OverallBuffManager.Instance.Luck.AddBuff(Relic.relicSO.name, buff);
        }

        public override void OnLevelChangeRelic()
        {
            Buff buff = buffIncreasePerLevel * Relic.Level;

            OverallBuffManager.Instance.Luck.AddBuff(Relic.relicSO.name, buff);
        }

        public override void OnRemoveRelic()
        {
            OverallBuffManager.Instance.Luck.RemoveBuff(Relic.relicSO.name);
        }
    }
}
