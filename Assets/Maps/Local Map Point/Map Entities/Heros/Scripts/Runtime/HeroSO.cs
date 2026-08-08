using System;
using UnityEngine;
using UnityEngine.U2D.Animation;
using Attacks;

public abstract class BuffableEntitySO : ScriptableObject
{
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

    public Buff GetBuff(StatTypes stat)
    {
        return stat switch
        {
            StatTypes.Health => Health,
            StatTypes.Shield => Shield,
            StatTypes.ShieldRegeneration => ShieldRegeneration,
            StatTypes.ShieldCooldown => ShieldCooldown,

            StatTypes.MovementSpeed => MovementSpeed,

            StatTypes.Damage => DamageBuff,
            StatTypes.AttackSpeed => AttackSpeed,

            StatTypes.NumberOfAttacks => NumberOfAttacks,
            StatTypes.CriticalChance => CriticalChance,
            StatTypes.CriticalDamageMultiplier => CriticalDamageMultiplier,
            StatTypes.AttackSize => AttackSize,
            StatTypes.Knockback => Knockback,
            StatTypes.ProjectileSpeed => ProjectileSpeed,
            StatTypes.ProjectileTurnSpeed => ProjectileTurnSpeed,
            _ => null
        };
    }
}



namespace Heros
{
    
    [CreateAssetMenu()]
    public class HeroSO : BuffableEntitySO
    {
        [TextArea()]
        public string Description;

        public Sprite icon;

        public Hero prefab;

        public BaseAttackSO startingAttack;

        public SpriteLibraryAsset spriteLibrary;

        public string GetHeroStatsString()
        {
            string str = "";
            foreach (StatTypes stat in Enum.GetValues(typeof(StatTypes)))
            {
                Buff buff = GetBuff(stat);

                if (buff == null)
                    continue;

                string buffDescription;

                if (stat == StatTypes.Health || 
                    stat == StatTypes.Shield || 
                    stat == StatTypes.MovementSpeed || 
                    stat == StatTypes.ShieldCooldown ||
                    stat == StatTypes.ShieldRegeneration)
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
