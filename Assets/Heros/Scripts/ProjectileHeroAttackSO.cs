using UnityEngine;
using System;

namespace Heros
{
    [CreateAssetMenu(menuName = "Hero Attack/Projectile")]
    public class ProjectileHeroAttackSO : HeroAttackSO
    {
        public override Type HeroAttackType => typeof(ProjectileHeroAttack);

        public Projectile projectilePrefab;

        public Buff numberOfProjectiles;

        public float projectileSpeed;

        public float projectileTurnSpeed;

        public bool projectileHoming;
    }
}

