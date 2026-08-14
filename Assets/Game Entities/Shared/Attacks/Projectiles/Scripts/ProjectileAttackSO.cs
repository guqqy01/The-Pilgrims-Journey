using UnityEngine;
using System;

namespace Attacks.Projectiles
{
    [CreateAssetMenu(menuName = "Attack/Projectile")]
    public class ProjectileAttackSO : AttackSO<Projectile, ProjectileWeaponStatsAtLevel>
    {
        public override Type AttackType => typeof(ProjectileAttack);

        public bool projectileHoming;
        public bool bounce;
        public float coneAngle = 40f;

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

