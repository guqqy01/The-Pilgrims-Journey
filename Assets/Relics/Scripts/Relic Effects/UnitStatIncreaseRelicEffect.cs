using UnityEngine;
using Units;
using LevelingSystem;

namespace Relics
{
    [AbstractTitle("Stat Increase/Normal")]
    public class UnitStatIncreaseRelicEffect : StatIncreaseRelicEffect
    {
        [SerializeField]
        private UnitStatsEnum stat;
        public UnitStatsEnum Stat => stat;

        public override string StatString => Stat.ToString().AddSpacesBeforeCapsRegex();

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

            UnitBuffManager.Instance.AddStatBuff(Relic.relicSO.name, stat, buff);
        }

        public override void OnLevelChangeRelic()
        {
            Buff buff = buffIncreasePerLevel * Relic.Level;

            UnitBuffManager.Instance.AddStatBuff(Relic.relicSO.name, stat, buff);
        }

        public override void OnRemoveRelic()
        {
            UnitBuffManager.Instance.RemoveStatBuff(Relic.relicSO.name, stat);
        }
    }
}
