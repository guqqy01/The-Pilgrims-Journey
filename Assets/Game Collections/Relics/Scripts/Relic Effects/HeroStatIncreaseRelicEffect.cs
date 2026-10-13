using UnityEngine;
using System;
using Heros;

namespace Relics
{
    [AbstractTitle("Stat Increase/Hero")]
    [Serializable]
    public class HeroStatIncreaseRelicEffect : StatIncreaseRelicEffect
    {
        [SerializeField]
        private StatTypes stat;
        public StatTypes Stat => stat;

        protected override bool BuffIsInt => stat.IsIntStat();

        public override RelicEffect CreateInstance(Relic relic)
        {
            return new HeroStatIncreaseRelicEffect(buffIncreasePerLevel, stat, updateUI);
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

        public HeroStatIncreaseRelicEffect() : base(default, default)
        {

        }

        public HeroStatIncreaseRelicEffect(Buff buff, StatTypes stat, Action updateUI = null) : base(buff, updateUI)
        {
            this.stat = stat;
        }

        public override void OnGainRelic(Relic relic)
        {
            Buff buff = buffIncreasePerLevel * relic.Level;
            HeroStatBuffsManager.IfReady(m => m.AddBuff(relic.relicSO.name, stat, buff));
        }

        public override void OnLevelChangeRelic(Relic relic, int currentLevel)
        {
            Buff buff = buffIncreasePerLevel * currentLevel;

            HeroStatBuffsManager.IfReady(m => m.AddBuff(relic.relicSO.name, stat, buff));
        }

        public override void OnRemoveRelic(Relic relic)
        {
            HeroStatBuffsManager.IfReady(m => m.RemoveBuff(relic.relicSO.name, stat));
        }
    }
}
