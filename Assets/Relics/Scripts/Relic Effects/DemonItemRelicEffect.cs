using System.Collections;
using System.Collections.Generic;
using Units;
using System;
namespace Relics
{

    public class DemonItemRelicEffect : RelicEffect
    {
        public override string Description
        {
            get
            {
                return $"Not implemented";
            }
        }

        public DemonItemRelicEffect(Relic relic, Action updateUI = null) : base(relic, updateUI)
        {
        }

        public override RelicEffect CreateInstance()
        {
            return new DemonItemRelicEffect(Relic, updateUI);
        }

        public override void OnGainRelic()
        {

        }

        private void AddBuffsToUnit(Unit unit)
        {
        }
    }
}
