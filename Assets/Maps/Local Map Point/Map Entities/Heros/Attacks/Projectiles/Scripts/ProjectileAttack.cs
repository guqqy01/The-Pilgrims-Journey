using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Heros;
namespace Attacks.Projectiles
{
    public class ProjectileAttack : Attack<ProjectileAttackSO>
    {
        [SerializeField]
        protected Projectile projectilePrefab;

        [SerializeField]
        protected BuffableStat numberOfProjectiles;

        [SerializeField]
        protected BuffableStat projectileSpeed;

        [SerializeField]
        protected BuffableStat projectileTurnSpeed;

        [SerializeField]
        protected bool projectileHoming;

        protected bool bounce;

        [SerializeField]
        private float coneAngle = 40f;

        private const float spawnFluctuationPercent = 0.5f;
        private const float spawnFluctuationMax = 0.2f;

        // pooling
        private ProjectilePool projectilePool;

        protected override bool Attack()
        {
            var target = GetTarget();

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

            Projectile newMissile;

            if (projectilePool != null)
            {
                newMissile = projectilePool.Get(transform.position, rotation);
            }
            else
            {
                newMissile = Instantiate(projectilePrefab, transform.position, rotation);
            }

            string reference = "baseSize";
            
            size.AddBuff(reference, new Buff(0f, newMissile.transform.localScale.x, 0));
            float newSizeX = size;
            size.RemoveBuff(reference);

            size.AddBuff(reference, new Buff(0f, newMissile.transform.localScale.y, 0));
            float newSizeY = size;
            size.RemoveBuff(reference);

            newMissile.transform.localScale =  new Vector3(newSizeX, newSizeY, newMissile.transform.localScale.z);

            ProjectileStats projectileStats = new ProjectileStats(damage, criticalChance, criticalDamageMultiplier, knockback, projectileSpeed, projectileTurnSpeed, destroyProjectiles, amountOfHits, projectilesCountAsHits);
            
            newMissile.InitWithOwner(projectileStats, projectileHoming, direction, hitLayerMask, bounce, target, ProjectileOwner.Hero);
        }

        protected override void InheritSetup(BuffableEntitySO buffableEntitySO, ProjectileAttackSO projectileHeroAttackSO)
        {
            SetupBuffableStat(ref numberOfProjectiles, StatTypes.NumberOfAttacks);
            SetupBuffableStat(ref projectileSpeed, StatTypes.ProjectileSpeed);
            SetupBuffableStat(ref projectileTurnSpeed, StatTypes.ProjectileTurnSpeed);

            projectilePrefab = projectileHeroAttackSO.projectilePrefab;

            projectileHoming = projectileHeroAttackSO.projectileHoming;

            bounce = projectileHeroAttackSO.bounce;

            // create or get shared pool for projectiles to reduce allocations
            if (projectilePrefab != null)
            {
                projectilePool = ProjectilePoolManager.GetPool(projectilePrefab, GetOrCreatePoolParent());
            }
        }

        protected override void InheritLevelSetup()
        {
            numberOfProjectiles.AddBuff("Weapon", AttackSO.GetBuff(StatTypes.NumberOfAttacks, Level));
            projectileSpeed.AddBuff("Weapon", AttackSO.GetBuff(StatTypes.ProjectileSpeed, Level));
            projectileTurnSpeed.AddBuff("Weapon", AttackSO.GetBuff(StatTypes.ProjectileTurnSpeed, Level));
        }

        private Transform GetOrCreatePoolParent()
        {
            GameObject poolParent = GameObject.Find("Pools");
            if (poolParent == null)
            {
                poolParent = new GameObject("Pools");
            }
            return poolParent.transform;
        }
    }
}
