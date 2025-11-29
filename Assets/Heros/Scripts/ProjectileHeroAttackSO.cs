using UnityEngine;
using System;

namespace Heros
{
    [CreateAssetMenu(menuName = "Hero Attack/Projectile")]
    public class ProjectileHeroAttackSO : HeroAttackSO
    {
        public override Type HeroAttackType => typeof(ProjectileHeroAttack);

        public Projectile projectilePrefab;

        public float projectileSpeed;

        public bool projectileHoming;
    }
}

