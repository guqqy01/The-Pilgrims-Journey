using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using static UnityEngine.GraphicsBuffer;
using System.Linq;

namespace Heros.Attacks.Projectiles
{
    public class ProjectileHeroAttack : HeroAttack<ProjectileHeroAttackSO>
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
            string reference = "baseSize";

            size.AddBuff(reference, new Buff(0f, newMissile.transform.localScale.x, 0));
            float newSizeX = size;
            size.RemoveBuff(reference);

            size.AddBuff(reference, new Buff(0f, newMissile.transform.localScale.y, 0));
            float newSizeY = size;
            size.RemoveBuff(reference);

            newMissile.transform.localScale =  new Vector3(newSizeX, newSizeY, newMissile.transform.localScale.z);

            ProjectileStats projectileStats = new ProjectileStats(damage, criticalChance, criticalDamageMultiplier, knockback, projectileSpeed, projectileTurnSpeed);

            newMissile.Init(projectileStats, projectileHoming, direction, enemyLayerMask, target);
        }

        protected override void InheritSetup(HeroSO heroSO, ProjectileHeroAttackSO projectileHeroAttackSO)
        {
            SetupBuffableStat(ref numberOfProjectiles, HeroStats.NumberOfAttacks);
            SetupBuffableStat(ref projectileSpeed, HeroStats.ProjectileSpeed);
            SetupBuffableStat(ref projectileTurnSpeed, HeroStats.ProjectileTurnSpeed);

            projectilePrefab = projectileHeroAttackSO.projectilePrefab;

            projectileHoming = projectileHeroAttackSO.projectileHoming;
        }

        protected override void InheritLevelSetup()
        {
            numberOfProjectiles.AddBuff("Weapon", HeroAttackSO.GetBuff(HeroStats.NumberOfAttacks, Level));
            projectileSpeed.AddBuff("Weapon", HeroAttackSO.GetBuff(HeroStats.ProjectileSpeed, Level));
            projectileTurnSpeed.AddBuff("Weapon", HeroAttackSO.GetBuff(HeroStats.ProjectileTurnSpeed, Level));
        }
    }
}
