using UnityEngine;
using System.Collections.Generic;

namespace Heros
{
    public class ProjectileHeroAttack : HeroAttack
    {
        [SerializeField]
        protected Projectile projectilePrefab;

        [SerializeField]
        protected float projectileSpeed;

        [SerializeField]
        protected bool projectileHoming;

        protected override bool Attack()
        {
            Transform target = GetTarget();

            if (target == null)
                return false;

            Projectile newMissile = Instantiate(projectilePrefab, transform.position, Quaternion.LookRotation(target.position - transform.position, Vector3.back));

            newMissile.Init(damage, projectileSpeed, projectileHoming, target, target.position - transform.position, enemyLayerMask);

            return true;
        }
    }
}
