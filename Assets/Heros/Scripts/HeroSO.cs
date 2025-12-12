using UnityEngine;

namespace Heros
{
    [CreateAssetMenu()]
    public class HeroSO : ScriptableObject
    {
        [TextArea()]
        public string Description;

        public Hero prefab;

        [Header("Utiltiy")]
        public Buff BaseMovementSpeedBuff;
        public Buff BaseLuckBuff;
        public Buff BaseExperienceGainBuff;

        [Header("Defence")]
        public Buff BaseHealthBuff;
        public Buff BaseShieldBuff;
        public Buff BaseShieldCooldownBuff;
        public Buff BaseShieldRegenerationBuff;

        [Header("Attack")]
        public Buff BaseAttackSpeedBuff;
        public Buff BaseAttackDamageBuff;
        public Buff BaseNumberOfAttacksBuff;
        public Buff BaseCriticalChanceBuff;
        public Buff BaseCriticalDamageBuff;
        public Buff BaseAttackSizeBuff;
    }

}
