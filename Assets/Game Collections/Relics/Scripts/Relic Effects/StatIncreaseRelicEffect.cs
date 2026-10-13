using UnityEngine;
using System;
namespace Relics
{
    [Serializable]
    public abstract class StatIncreaseRelicEffect : RelicEffect
    {
        [SerializeField]
        [BuffType(nameof(BuffIsInt))]
        protected Buff buffIncreasePerLevel;
        public Buff BuffIncreasePerLevel => buffIncreasePerLevel;

        protected abstract bool BuffIsInt { get; }

        public abstract string StatString { get; }

        public StatIncreaseRelicEffect(Buff buffIncreasePerLevel, Action updateUI = null) : base(updateUI)
        {
            this.buffIncreasePerLevel = buffIncreasePerLevel;
        }

        
    }
}
