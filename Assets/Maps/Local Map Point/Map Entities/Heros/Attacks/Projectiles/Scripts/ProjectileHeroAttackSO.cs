using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Heros.Attacks.Projectiles
{
    [CreateAssetMenu(menuName = "Hero Attack/Projectile")]
    public class ProjectileHeroAttackSO : HeroAttackSO<ProjectileWeaponStatsAtLevel>
    {
        public override Type HeroAttackType => typeof(ProjectileHeroAttack);

        public Projectile projectilePrefab;

        public bool projectileHoming;
        public bool bounce;
        public override Buff GetBuffAtIndex(HeroStats heroStats, int index)
        {
            return heroStats switch
            {
                HeroStats.NumberOfAttacks => WeaponStatsAtLevel[index].numberOfProjectiles,
                HeroStats.ProjectileSpeed => WeaponStatsAtLevel[index].projectileSpeed,
                HeroStats.ProjectileTurnSpeed => WeaponStatsAtLevel[index].projectileTurnSpeed,
                HeroStats.AttackDurability => WeaponStatsAtLevel[index].amountOfHits,
                _ => null,
            };
        }

    }
}

