using UnityEngine;
using System;
namespace Relics
{
    [Serializable]
    public abstract class StatIncreaseRelicEffect : RelicEffect
    {
        [SerializeField]
        protected Buff buffIncreasePerLevel;
        public Buff BuffIncreasePerLevel => buffIncreasePerLevel;

        public abstract string StatString { get; }

        public StatIncreaseRelicEffect(Buff buffIncreasePerLevel, Action updateUI = null) : base(updateUI)
        {
            this.buffIncreasePerLevel = buffIncreasePerLevel;
        }

        
    }
}
