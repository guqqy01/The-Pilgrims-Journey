using UnityEngine;
namespace Heros
{

    public class HeroStatsManager : Singleton<HeroStatsManager>
    {
        [Header("Stats")]
        public BuffableStat Health;
        public BuffableStat Shield;
        public BuffableStat ShieldCooldown;
        public BuffableStat ShieldRegeneration;

        public BuffableStat MovementSpeed;
        public BuffableStat Luck;
        public BuffableStat ExperienceGain;

        public BuffableStat DamageBuff;
        public BuffableStat AttackSpeed;

        public BuffableStat NumberOfAttacks;
        public BuffableStat CriticalChance;
        public BuffableStat CriticalDamage;
        public BuffableStat AttackSize;

        [Header("Other")]
        public Hero Hero;

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

