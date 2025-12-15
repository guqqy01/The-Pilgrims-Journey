using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using static UnityEngine.GraphicsBuffer;

namespace Heros
{
    public class ProjectileHeroAttack : HeroAttack
    {
        [SerializeField]
        protected Projectile projectilePrefab;

        [SerializeField]
        protected BuffableStat numberOfProjectiles;

        [SerializeField]
        protected float projectileSpeed;

        [SerializeField]
        protected float projectileTurnSpeed;

        [SerializeField]
        protected bool projectileHoming;

        [SerializeField]
        private float coneAngle = 40f;

        private const float spawnFluctuationPercent = 0.5f;
        private const float spawnFluctuationMax = 0.2f;


        protected override bool Attack()
        {
            Transform target = GetTarget();

            if (target == null)
                return false;

            Vector2 direction = (target.position - transform.position).normalized;

            List<Vector2> directions;

            if(projectileHoming)
                directions = direction.SplitConeDirections(numberOfProjectiles, coneAngle * Mathf.Deg2Rad);
            else
                directions = direction.SplitConeDirectionsAlwaysHaveMainDirection(numberOfProjectiles, coneAngle * Mathf.Deg2Rad);

            for (int i = 0; i < numberOfProjectiles; i++)
            {
                float fluctuation = numberOfProjectiles > 1 ? Mathf.Clamp(Random.Range(0f, (1f / attackSpeed * spawnFluctuationPercent)), 0f, spawnFluctuationMax) : 0f;

                float spawnTime = Time.time + fluctuation;

                StartCoroutine(SpawnProjectile(spawnTime, directions[i], target));
            }

            return true;
        }

        protected IEnumerator SpawnProjectile(float spawnTime, Vector2 direction, Transform target)
        {
            while(spawnTime > Time.time)
                yield return null;

            Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);

            Projectile newMissile = Instantiate(projectilePrefab, transform.position, rotation);

            ProjectileHitStats projectileStats = new ProjectileHitStats(damage, criticalChance, criticalDamageMultiplier, knockback, projectileSpeed, projectileTurnSpeed);

            newMissile.Init(projectileStats, projectileHoming, direction, enemyLayerMask, target);
        }

        protected override void InheritSetup(HeroSO heroSO, HeroAttackSO heroAttackSO)
        {
            if (heroAttackSO is not ProjectileHeroAttackSO projectileAttack)
            {
                return;
            }

            numberOfProjectiles = HeroStatBase.GetBuffableStat(heroSO, HeroStats.NumberOfProjectiles);
            numberOfProjectiles.AddBuff("Weapon Base", projectileAttack.numberOfProjectiles);

            projectilePrefab = projectileAttack.projectilePrefab;

            projectileSpeed = projectileAttack.projectileSpeed;

            projectileTurnSpeed = projectileAttack.projectileTurnSpeed;

            projectileHoming = projectileAttack.projectileHoming;
        }
    }
}
