using UnityEngine;

namespace Heros
{
    [CreateAssetMenu()]
    public class HeroSO : ScriptableObject
    {
        [TextArea()]
        public string Description;

        public Hero prefab;

        [Header("Defence")]
        public Buff Health;
        public Buff Shield;
        public Buff ShieldCooldown;
        public Buff ShieldRegeneration;

        [Header("Utility")]
        public Buff MovementSpeed;
        public Buff Luck;
        public Buff ExperienceGain;

        [Header("Attack")]
        public Buff DamageBuff;
        public Buff AttackSpeed;
        public Buff NumberOfAttacks;
        public Buff CriticalChance;
        public Buff CriticalDamage;
        public Buff AttackSize;

        public Buff GetBuff(HeroStats stat)
        {
            return stat switch
            {
                HeroStats.Health => Health,
                HeroStats.Shield => Shield,
                HeroStats.ShieldRegeneration => ShieldRegeneration,
                HeroStats.ShieldCooldown => ShieldCooldown,

                HeroStats.MovementSpeed => MovementSpeed,
                HeroStats.Luck => Luck,
                HeroStats.ExperienceGain => ExperienceGain,

                HeroStats.Damage => DamageBuff,
                HeroStats.AttackSpeed => AttackSpeed,

                HeroStats.NumberOfAttacks => NumberOfAttacks,
                HeroStats.CriticalChance => CriticalChance,
                HeroStats.CriticalDamage => CriticalDamage,
                HeroStats.Size => AttackSize,
                _ => null
            };
        }
    }

}
