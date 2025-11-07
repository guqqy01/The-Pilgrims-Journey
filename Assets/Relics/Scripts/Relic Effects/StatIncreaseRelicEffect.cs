using UnityEngine;
namespace Relics
{
    public abstract class StatIncreaseRelicEffect : RelicEffect
    {
        [SerializeField]
        protected Buff buffIncreasePerLevel;
        public Buff BuffIncreasePerLevel => buffIncreasePerLevel;

        public abstract string StatString { get; }
    }
}
