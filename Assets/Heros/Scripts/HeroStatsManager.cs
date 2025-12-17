using UnityEngine;
using System;

namespace Heros
{
    public class HeroStatsManager : Singleton<HeroStatsManager>
    {
        private int currentHealth;
        public int CurrentHealth
        {
            get
            {
                return Mathf.Min(currentHealth, MaxHealth);
            }
        }


        public BuffableStat MaxHealth = new BuffableStat(0);
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

        public delegate void UpdateUI();
        public UpdateUI updateUI;

        public IntOrFloatObject AddBuff(string reference, Buff buff, HeroStats heroStat)
        {
            IntOrFloatObject value;

            switch (heroStat)
            {
                case HeroStats.Health:
                    value = MaxHealth.AddBuff(reference, buff);
                    break;
                default:
                    throw new NotImplementedException();
            }

            updateUI?.Invoke();

            return value;
        }

        public IntOrFloatObject RemoveBuff(string reference, Buff buff, HeroStats heroStat)
        {
            switch (heroStat)
            {
                case HeroStats.Health:
                    return MaxHealth.RemoveBuff(reference);
                default:
                    throw new NotImplementedException();
            }
        }

        public Buff GetBuff(HeroStats stat)
        {
            return stat switch
            {
                HeroStats.Health => MaxHealth,
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

