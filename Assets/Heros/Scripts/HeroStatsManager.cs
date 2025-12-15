using UnityEngine;
namespace Heros
{
    public class HeroStatsManager : Singleton<HeroStatsManager>
    {
        public BuffableStat Health = new BuffableStat(0);
        public BuffableStat Shield = new BuffableStat(0);
        public BuffableStat ShieldCooldown = new BuffableStat(0f);
        public BuffableStat ShieldRegeneration = new BuffableStat(0f);

        public BuffableStat MovementSpeed = new BuffableStat(0f);

        public BuffableStat DamageBuff = new BuffableStat(0);
        public BuffableStat AttackSpeed = new BuffableStat(0);

        public BuffableStat NumberOfProjectiles = new BuffableStat(0);
        public BuffableStat CriticalChance = new BuffableStat(0f);
        public BuffableStat CriticalDamage = new BuffableStat(0);
        public BuffableStat AttackSize = new BuffableStat(0f);
        public BuffableStat Knockback = new BuffableStat(0f);

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

                HeroStats.NumberOfProjectiles => NumberOfProjectiles,
                HeroStats.CriticalChance => CriticalChance,
                HeroStats.CriticalDamage => CriticalDamage,
                HeroStats.Size => AttackSize,
                HeroStats.Knockback => Knockback,
                _ => null
            };
        }
    }
}

