using LevelingSystem;
using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
namespace Relics
{
    [AbstractTitle("Stat Increase/Luck")]
    [Serializable]
    public class IncreaseLuckRelicEffect : StatIncreaseRelicEffect
    {
        public override string StatString => "Luck";
        protected override bool BuffIsInt => false;
        public override string Description(Relic relic)
        {
            return $"{StatString}: {(buffIncreasePerLevel * relic.Level).GetSignedSimpleDescription()}";
        }

        public override string Description(RelicSO relicSO)
        {
            return $"{StatString} Increase Per Level: {(buffIncreasePerLevel).GetSignedSimpleDescription()}";
        }

        public IncreaseLuckRelicEffect() : base(default, default) { }

        public IncreaseLuckRelicEffect(Buff buffIncreasePerLevel, Action updateUI = null) : base(buffIncreasePerLevel, updateUI)
        {
            
        }

        public override RelicEffect CreateInstance(Relic relic)
        {
            return new IncreaseLuckRelicEffect(buffIncreasePerLevel, updateUI);
        }

        public override void OnGainRelic(Relic relic)
        {
            Buff buff = buffIncreasePerLevel * relic.Level;

            OverallStatsManager.Instance.AddBuff(relic.relicSO.name, buff, OverallStats.Luck);
        }

        public override void OnLevelChangeRelic(Relic relic, int currentLevel)
        {
            if (relic == null)
            {
                Debug.LogError($"Relic empty in relicEffect: {this}");
                return;
            }

            Buff buff = buffIncreasePerLevel * currentLevel;

            OverallStatsManager.Instance.AddBuff(relic.relicSO.name, buff, OverallStats.Luck);
        }

        public override void OnRemoveRelic(Relic relic)
        {
            OverallStatsManager.Instance.RemoveBuff(relic.relicSO.name, OverallStats.Luck);
        }

        
    }
}
