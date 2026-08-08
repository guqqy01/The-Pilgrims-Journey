using System.Collections.Generic;
using UnityEngine;
using Heros;
namespace Attacks.Spot
{
    public abstract class SpotAttack : Attack<SpotAttackSOBase>
    {
        protected BuffableStat spawnAccuracy;
        protected BuffableStat hitRefreshRate;

        protected SpotChaoticMovementStats SpotChaoticMovementStats;

        // store prefab GameObject instead of a generic typed component
        protected GameObject spawnPrefabGameObject;

        private readonly List<ISpotAttackSpawn> spotAttackSpawns = new List<ISpotAttackSpawn>();

        private bool projectilesCountToHits;
        private float? lifeTime;
        private float movementSpeed;

        protected override bool Attack()
        {
            Transform target = GetTarget();
            if (target == null)
                return false;

            SpawnAttack(target);
            return true;
        }

        protected virtual void SpawnAttack(Transform target)
        {
            if (spawnPrefabGameObject == null)
            {
                Debug.LogError("Spawn prefab not set on SpotHeroAttack");
                return;
            }

            GameObject instance = Instantiate(spawnPrefabGameObject);

            var spawn = instance.GetComponent<ISpotAttackSpawn>();
            if (spawn == null)
            {
                Debug.LogError($"Spawn prefab {spawnPrefabGameObject.name} does not implement ISpotAttackSpawn");
                Destroy(instance);
                return;
            }

            SpotAttackInit initContext = BuildInit(target, lifeTime, movementSpeed);
            SpotAttackStats statsContext = BuildStatsContext();

            spawn.Init(initContext, statsContext);

            spotAttackSpawns.Add(spawn);
        }

        protected virtual SpotAttackInit BuildInit(Transform target, float? lifeTime, float movementSpeed)
        {
            return new SpotAttackInit((Vector2)target.position + UnityEngine.Random.insideUnitCircle * spawnAccuracy.Value, lifeTime.HasValue ? Time.time + lifeTime.Value : (float?)null, movementSpeed);
        }

        protected virtual SpotAttackStats BuildStatsContext()
        {
            return new SpotAttackStats(BuildBaseAttackStats(), SpotChaoticMovementStats);
        }

        protected override void InheritSetup(BuffableEntitySO buffableEntitySO, SpotAttackSOBase spotAttackSO)
        {
            SpotChaoticMovementStats = spotAttackSO.spotChaoticMovementStats;

            SetupBuffableStat(ref spawnAccuracy, StatTypes.Accuracy);
            SetupBuffableStat(ref hitRefreshRate, StatTypes.SpotHitRefreshRate);

            // Use the SO's SpawnPrefab GameObject directly. Don't try to get a generic component from the prefab,
            // because concrete spawn components use different generic type parameters and GetComponent<T> won't match.
            spawnPrefabGameObject = spotAttackSO.SpawnPrefab;

            destroyProjectiles = spotAttackSO.destroyProjectiles;
            amountOfHits = spotAttackSO.amountOfHits == 0 ? null : spotAttackSO.amountOfHits;
            projectilesCountToHits = spotAttackSO.projectilesCountToHits;
            lifeTime = spotAttackSO.lifeTime == 0 ? null : spotAttackSO.lifeTime;
            movementSpeed = spotAttackSO.movementSpeed;
        }

        protected override void InheritLevelSetup()
        {

        }

        protected AttackStats BuildBaseAttackStats()
        {
            return new AttackStats(
                damage.Value,
                criticalChance.Value,
                criticalDamageMultiplier.Value,
                knockback.Value,
                hitRefreshRate.Value,
                destroyProjectiles,
                amountOfHits,
                projectilesCountToHits
            );
        }

    }

}
