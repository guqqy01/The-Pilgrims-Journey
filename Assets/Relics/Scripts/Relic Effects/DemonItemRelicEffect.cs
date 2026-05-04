using System.Collections;
using System.Collections.Generic;
using Units;
using System;
namespace Relics
{

    public class DemonItemRelicEffect : RelicEffect
    {
        public override string Description(Relic relic)
        {
            return $"Not implemented";
        }

        public override string Description(RelicSO relic)
        {
            return $"Not implemented";
        }

        public DemonItemRelicEffect() : base(default)
        {
        }

        public DemonItemRelicEffect(Action updateUI = null) : base(updateUI)
        {
        }

        public override RelicEffect CreateInstance(Relic relic)
        {
            return new DemonItemRelicEffect(updateUI);
        }

        public override void OnGainRelic(Relic relic)
        {

        }

        private void AddBuffsToUnit(Unit unit)
        {
        }
    }
}
