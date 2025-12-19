using UnityEngine;
using System;

namespace Heros
{
    public class HeroStatBuffsManager : Singleton<HeroStatBuffsManager>
    {
        private int currentHealth;
        public int CurrentHealth
        {
            get
            {
                return Mathf.Min(currentHealth, maxHealth.Value);
            }
        }

        private readonly BuffableStat maxHealth = new BuffableStat(0);
        public BuffableStat MaxHealth => maxHealth;

        private readonly BuffableStat shield = new BuffableStat(0);
        public BuffableStat Shield => shield;

        private readonly BuffableStat shieldCooldown = new BuffableStat(0f);
        public BuffableStat ShieldCooldown => shieldCooldown;

        private readonly BuffableStat shieldRegeneration = new BuffableStat(0f);
        public BuffableStat ShieldRegeneration => shieldRegeneration;

        private readonly BuffableStat movementSpeed = new BuffableStat(0f);
        public BuffableStat MovementSpeed => movementSpeed;

        private readonly BuffableStat damageBuff = new BuffableStat(0);
        public BuffableStat DamageBuff => damageBuff;

        private readonly BuffableStat attackSpeed = new BuffableStat(0);
        public BuffableStat AttackSpeed => attackSpeed;

        private readonly BuffableStat numberOfProjectiles = new BuffableStat(0);
        public BuffableStat NumberOfProjectiles => numberOfProjectiles;

        private readonly BuffableStat criticalChance = new BuffableStat(0f);
        public BuffableStat CriticalChance => criticalChance;

        private readonly BuffableStat criticalDamageMultiplier = new BuffableStat(0);
        public BuffableStat CriticalDamageMultiplier => criticalDamageMultiplier;

        private readonly BuffableStat attackSize = new BuffableStat(0f);
        public BuffableStat AttackSize => attackSize;

        private readonly BuffableStat knockback = new BuffableStat(0f);
        public BuffableStat Knockback => knockback;

        public delegate void OnBuffsChanged();
        public OnBuffsChanged buffsChanged;

        public IntOrFloatObject AddBuff(string reference, Buff buff, HeroStats heroStat)
        {
            IntOrFloatObject value;

            switch (heroStat)
            {
                case HeroStats.Health:
                    value = maxHealth.AddBuff(reference, buff);
                    break;
                case HeroStats.Shield:
                    value = shield.AddBuff(reference, buff);
                    break;
                case HeroStats.ShieldCooldown:
                    value = shieldCooldown.AddBuff(reference, buff);
                    break;
                case HeroStats.ShieldRegeneration:
                    value = shieldRegeneration.AddBuff(reference, buff);
                    break;
                case HeroStats.MovementSpeed:
                    value = movementSpeed.AddBuff(reference, buff);
                    break;
                case HeroStats.Damage:
                    value = damageBuff.AddBuff(reference, buff);
                    break;
                case HeroStats.AttackSpeed:
                    value = attackSpeed.AddBuff(reference, buff);
                    break;
                case HeroStats.NumberOfProjectiles:
                    value = numberOfProjectiles.AddBuff(reference, buff);
                    break;
                case HeroStats.CriticalChance:
                    value = criticalChance.AddBuff(reference, buff);
                    break;
                case HeroStats.CriticalDamageMultiplier:
                    value = criticalDamageMultiplier.AddBuff(reference, buff);
                    break;
                case HeroStats.AttackSize:
                    value = attackSize.AddBuff(reference, buff);
                    break;
                case HeroStats.Knockback:
                    value = knockback.AddBuff(reference, buff);
                    break;
                default:
                    throw new NotImplementedException();
            }

            buffsChanged?.Invoke();

            return value;
        }

        public IntOrFloatObject RemoveBuff(string reference, HeroStats heroStat)
        {
            IntOrFloatObject value;

            switch (heroStat)
            {
                case HeroStats.Health:
                    value = maxHealth.RemoveBuff(reference);
                    break;
                case HeroStats.Shield:
                    value = shield.RemoveBuff(reference);
                    break;
                case HeroStats.ShieldCooldown:
                    value = shieldCooldown.RemoveBuff(reference);
                    break;
                case HeroStats.ShieldRegeneration:
                    value = shieldRegeneration.RemoveBuff(reference);
                    break;
                case HeroStats.MovementSpeed:
                    value = movementSpeed.RemoveBuff(reference);
                    break;
                case HeroStats.Damage:
                    value = damageBuff.RemoveBuff(reference);
                    break;
                case HeroStats.AttackSpeed:
                    value = attackSpeed.RemoveBuff(reference);
                    break;
                case HeroStats.NumberOfProjectiles:
                    value = numberOfProjectiles.RemoveBuff(reference);
                    break;
                case HeroStats.CriticalChance:
                    value = criticalChance.RemoveBuff(reference);
                    break;
                case HeroStats.CriticalDamageMultiplier:
                    value = criticalDamageMultiplier.RemoveBuff(reference);
                    break;
                case HeroStats.AttackSize:
                    value = attackSize.RemoveBuff(reference);
                    break;
                case HeroStats.Knockback:
                    value = knockback.RemoveBuff(reference);
                    break;
                default:
                    throw new NotImplementedException();
            }

            buffsChanged?.Invoke();

            return value;
        }

        public Buff GetBuff(HeroStats stat)
        {
            return stat switch
            {
                HeroStats.Health => maxHealth,
                HeroStats.Shield => shield,
                HeroStats.ShieldRegeneration => shieldRegeneration,
                HeroStats.ShieldCooldown => shieldCooldown,

                HeroStats.MovementSpeed => movementSpeed,

                HeroStats.Damage => damageBuff,
                HeroStats.AttackSpeed => attackSpeed,

                HeroStats.NumberOfProjectiles => numberOfProjectiles,
                HeroStats.CriticalChance => criticalChance,
                HeroStats.CriticalDamageMultiplier => criticalDamageMultiplier,
                HeroStats.AttackSize => attackSize,
                HeroStats.Knockback => knockback,
                _ => null
            };
        }
    }
}

