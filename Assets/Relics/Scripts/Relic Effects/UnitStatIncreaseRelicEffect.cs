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

        public override RelicEffect CreateInstance()
        {
            return new UnitStatIncreaseRelicEffect(Relic, buffIncreasePerLevel, stat, updateUI);
        }

        public override string StatString => Stat.ToString().AddSpacesBeforeCapsRegex();

        public override string Description
        {
            get
            {
                return $"{StatString}: {(buffIncreasePerLevel * Relic.Level).GetSignedSimpleDescription()}";
            }
        }

        public UnitStatIncreaseRelicEffect(Relic relic, Buff buff, UnitStatsEnum stat, Action updateUI = null) : base(relic, buff, updateUI)
        {
            this.stat = stat;
        }

        public override void OnGainRelic()
        {
            Buff buff = buffIncreasePerLevel * Relic.Level;

            UnitBuffManager.Instance.AddStatBuff(Relic.relicSO.name, stat, buff);
        }

        public override void OnLevelChangeRelic(int currentLevel)
        {
            if (Relic == null)
            {
                Debug.LogError($"Relic empty in relicEffect: {this}");
                return;
            }

            Buff buff = buffIncreasePerLevel * currentLevel;

            UnitBuffManager.Instance.AddStatBuff(Relic.relicSO.name, stat, buff);
        }

        public override void OnRemoveRelic()
        {
            UnitBuffManager.Instance.RemoveStatBuff(Relic.relicSO.name, stat);
        }

        
    }
}
