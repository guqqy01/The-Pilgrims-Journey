using UnityEngine;
using System;
using Heros;

namespace Attacks.Projectiles
{
    [CreateAssetMenu(menuName = "Hero Attack/Projectile")]
    public class ProjectileHeroAttackSO : AttackSO<ProjectileWeaponStatsAtLevel>
    {
        public override Type HeroAttackType => typeof(ProjectileHeroAttack);

        public Projectile projectilePrefab;

        public bool projectileHoming;
        public bool bounce;
        public override Buff GetBuffAtIndex(StatTypes heroStats, int index)
        {
            return heroStats switch
            {
                StatTypes.NumberOfAttacks => WeaponStatsAtLevel[index].numberOfProjectiles,
                StatTypes.ProjectileSpeed => WeaponStatsAtLevel[index].projectileSpeed,
                StatTypes.ProjectileTurnSpeed => WeaponStatsAtLevel[index].projectileTurnSpeed,
                StatTypes.AttackDurability => WeaponStatsAtLevel[index].amountOfHits,
                _ => null,
            };
        }

    }
}

