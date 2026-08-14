using Heros;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
namespace Attacks.Projectiles
{
    public class ProjectileAttack : Attack<ProjectileAttackSO, ProjectileInit, ProjectileStats>
    {
        [SerializeField]
        protected BuffableStat numberOfProjectiles;

        [SerializeField]
        protected BuffableStat projectileSpeed;

        [SerializeField]
        protected BuffableStat projectileTurnSpeed;

        [SerializeField]
        protected bool homing;

        protected bool bounce;

        private const float spawnFluctuationPercent = 0.5f;
        private const float spawnFluctuationMax = 0.2f;

        protected override bool ProtectedAttack()
        {
            var target = GetTarget();

            if (target == null)
                return false;

            Vector2 direction = (target.position - transform.position).normalized;

            List<Vector2> directions;

            if(homing)
                directions = direction.SplitConeDirections(numberOfProjectiles, AttackSO.coneAngle * Mathf.Deg2Rad);
            else
                directions = direction.SplitConeDirectionsAlwaysHaveMainDirection(numberOfProjectiles, AttackSO.coneAngle * Mathf.Deg2Rad);

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
            ProjectileInit projectileInit = new ProjectileInit(hitLayerMask, transform.position, rotation, homing, bounce, target, direction, null);
            Projectile attackSpawn = GetAttackSpawnFromPoolAndSetup<Projectile>(projectileInit);
        }

        protected override ProjectileStats GetAttackStats()
        {
            return new ProjectileStats(
                damage, 
                size,
                criticalChance, 
                criticalDamageMultiplier, 
                knockback, 
                projectileSpeed, 
                projectileTurnSpeed, 
                destroyProjectiles, 
                amountOfHits, 
                projectilesCountAsHits);
        }

        protected override void InheritSetup(ProjectileAttackSO projectileHeroAttackSO)
        {
            SetupBuffableStat(ref numberOfProjectiles, StatTypes.NumberOfAttacks);
            SetupBuffableStat(ref projectileSpeed, StatTypes.ProjectileSpeed);
            SetupBuffableStat(ref projectileTurnSpeed, StatTypes.ProjectileTurnSpeed);

            homing = projectileHeroAttackSO.projectileHoming;

            bounce = projectileHeroAttackSO.bounce;
        }

        protected override void InheritLevelSetup()
        {
            numberOfProjectiles.AddBuff("Weapon", AttackSO.GetBuff(StatTypes.NumberOfAttacks, Level));
            projectileSpeed.AddBuff("Weapon", AttackSO.GetBuff(StatTypes.ProjectileSpeed, Level));
            projectileTurnSpeed.AddBuff("Weapon", AttackSO.GetBuff(StatTypes.ProjectileTurnSpeed, Level));
        }

        
    }
}
