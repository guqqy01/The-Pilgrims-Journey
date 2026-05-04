using UnityEngine;
using Units;
using LevelingSystem;
using System;

namespace Relics
{
    [AbstractTitle("Stat Increase/Normal")]
    public class UnitStatIncreaseRelicEffect : StatIncreaseRelicEffect
    {
        [SerializeField]
        private UnitStatsEnum stat;
        public UnitStatsEnum Stat => stat;

        public override RelicEffect CreateInstance(Relic relic)
        {
            return new UnitStatIncreaseRelicEffect(buffIncreasePerLevel, stat, updateUI);
        }

        public override string StatString => Stat.ToString().AddSpacesBeforeCapsRegex();

        public override string Description(Relic relic)
        {
            return $"{StatString}: {(buffIncreasePerLevel * relic.Level).GetSignedSimpleDescription()}";
        }

        public override string Description(RelicSO relicSO)
        {
            return $"{StatString} Increase Per Level: {(buffIncreasePerLevel).GetSignedSimpleDescription()}";
        }

        public UnitStatIncreaseRelicEffect() : base(default, default)
        {

        }

        public UnitStatIncreaseRelicEffect(Buff buff, UnitStatsEnum stat, Action updateUI = null) : base(buff, updateUI)
        {
            this.stat = stat;
        }

        public override void OnGainRelic(Relic relic)
        {
            Buff buff = buffIncreasePerLevel * relic.Level;

            UnitBuffManager.Instance.AddStatBuff(relic.relicSO.name, stat, buff);
        }

        public override void OnLevelChangeRelic(Relic relic, int currentLevel)
        {
            Buff buff = buffIncreasePerLevel * currentLevel;

            UnitBuffManager.Instance.AddStatBuff(relic.relicSO.name, stat, buff);
        }

        public override void OnRemoveRelic(Relic relic)
        {
            UnitBuffManager.Instance.RemoveStatBuff(relic.relicSO.name, stat);
        }

        
    }
}
