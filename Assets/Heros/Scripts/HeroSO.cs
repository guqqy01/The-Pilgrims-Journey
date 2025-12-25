using UnityEngine;

namespace Heros
{
    [CreateAssetMenu()]
    public class HeroSO : ScriptableObject
    {
        [TextArea()]
        public string Description;

        public Hero prefab;

        public HeroAttackSO startingAttack;

        [Header("Defence")]
        [BuffType(true)]
        public Buff Health;
        [BuffType(true)]
        public Buff Shield;
        [BuffType(false)]
        public Buff ShieldCooldown;
        [BuffType(false)]
        public Buff ShieldRegeneration;

        [Header("Utility")]
        [BuffType(false)]
        public Buff MovementSpeed;
        [BuffType(false)]
        public Buff Luck;
        [BuffType(false)]
        public Buff ExperienceGain;

        [Header("Attack")]
        [BuffType(true)]
        public Buff DamageBuff;
        [BuffType(false)]
        public Buff AttackSpeed;
        [BuffType(true)]
        public Buff NumberOfAttacks;
        [BuffType(false)]
        [Tooltip("critical percentage, value 100 => 100% chance to crit")]
        public Buff CriticalChance;
        [BuffType(false)]
        [Tooltip("critical damage multiplier, value 100 => +100% damage increase")]
        public Buff CriticalDamageMultiplier;
        [BuffType(false)]
        public Buff AttackSize;
        [BuffType(false)]
        public Buff Knockback;

        [BuffType(false)]
        public Buff ProjectileSpeed;
        [BuffType(false)]
        public Buff ProjectileTurnSpeed;

        public Buff GetBuff(HeroStats stat)
        {
            return stat switch
            {
                HeroStats.Health => Health,
                HeroStats.Shield => Shield,
                HeroStats.ShieldRegeneration => ShieldRegeneration,
                HeroStats.ShieldCooldown => ShieldCooldown,

                HeroStats.MovementSpeed => MovementSpeed,

                HeroStats.Damage => DamageBuff,
                HeroStats.AttackSpeed => AttackSpeed,

                HeroStats.NumberOfProjectiles => NumberOfAttacks,
                HeroStats.CriticalChance => CriticalChance,
                HeroStats.CriticalDamageMultiplier => CriticalDamageMultiplier,
                HeroStats.AttackSize => AttackSize,
                HeroStats.Knockback => Knockback,
                HeroStats.ProjectileSpeed => ProjectileSpeed,
                HeroStats.ProjectileTurnSpeed => ProjectileTurnSpeed,
                _ => null
            };
        }
    }
}
