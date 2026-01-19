using System;
using UnityEngine;
using UnityEngine.U2D.Animation;
using Heros.Attacks;

namespace Heros
{
    [CreateAssetMenu()]
    public class HeroSO : ScriptableObject
    {
        [TextArea()]
        public string Description;

        public Sprite icon;

        public Hero prefab;

        public HeroAttackSO startingAttack;

        public SpriteLibraryAsset spriteLibrary;

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

                HeroStats.NumberOfAttacks => NumberOfAttacks,
                HeroStats.CriticalChance => CriticalChance,
                HeroStats.CriticalDamageMultiplier => CriticalDamageMultiplier,
                HeroStats.AttackSize => AttackSize,
                HeroStats.Knockback => Knockback,
                HeroStats.ProjectileSpeed => ProjectileSpeed,
                HeroStats.ProjectileTurnSpeed => ProjectileTurnSpeed,
                _ => null
            };
        }

        public string GetHeroStatsString()
        {
            string str = "";
            foreach (HeroStats stat in Enum.GetValues(typeof(HeroStats)))
            {
                Buff buff = GetBuff(stat);

                string buffDescription;

                if (stat == HeroStats.Health || 
                    stat == HeroStats.Shield || 
                    stat == HeroStats.MovementSpeed || 
                    stat == HeroStats.ShieldCooldown ||
                    stat == HeroStats.ShieldRegeneration)
                {
                    buffDescription = buff.GetSimpleDescription();
                }
                else
                {
                    buffDescription = buff.GetSignedSimpleDescription();
                }

                if (String.IsNullOrEmpty(buffDescription))
                    continue;

                string buffStr = stat.ToString().AddSpacesBeforeCapsRegex() + ": " + buffDescription;

                buffStr.AppendLineToTarget(ref str);
            }

            return str;
        }
    }
}
