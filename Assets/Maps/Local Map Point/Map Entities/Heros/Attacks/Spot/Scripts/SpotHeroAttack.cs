using Heros.Attacks.Orbital;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Heros.Attacks.Spot
{
    public abstract class SpotHeroAttack : HeroAttack<SpotHeroAttackSOBase>
    {
        protected BuffableStat spawnAccuracy;
        protected BuffableStat hitRefreshRate;

        protected SpotChaoticMovementStats SpotChaoticMovementStats;

        // store prefab GameObject instead of a generic typed component
        protected GameObject spawnPrefabGameObject;

        private readonly List<ISpotAttackSpawn> spotAttackSpawns = new List<ISpotAttackSpawn>();

        private bool destroyProjectiles;
        private int? amountOfHits;
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

            SpotAttackInitContext initContext = BuildInitContext(target, lifeTime, movementSpeed);
            SpotAttackStatsContext statsContext = BuildStatsContext();

            spawn.Init(initContext, statsContext);

            spotAttackSpawns.Add(spawn);
        }

        protected virtual SpotAttackInitContext BuildInitContext(Transform target, float? lifeTime, float movementSpeed)
        {
            return new SpotAttackInitContext((Vector2)target.position + UnityEngine.Random.insideUnitCircle * spawnAccuracy.Value, lifeTime.HasValue ? Time.time + lifeTime.Value : (float?)null, movementSpeed);
        }

        protected virtual SpotAttackStatsContext BuildStatsContext()
        {
            return new SpotAttackStatsContext(BuildBaseHeroAttackStats(), SpotChaoticMovementStats);
        }

        protected override void InheritSetup(HeroSO heroSO, SpotHeroAttackSOBase spotHeroAttackSO)
        {
            SpotChaoticMovementStats = spotHeroAttackSO.spotChaoticMovementStats;

            SetupBuffableStat(ref spawnAccuracy, HeroStats.Accuracy);
            SetupBuffableStat(ref hitRefreshRate, HeroStats.SpotHitRefreshRate);

            // Use the SO's SpawnPrefab GameObject directly. Don't try to get a generic component from the prefab,
            // because concrete spawn components use different generic type parameters and GetComponent<T> won't match.
            spawnPrefabGameObject = spotHeroAttackSO.SpawnPrefab;

            destroyProjectiles = spotHeroAttackSO.destroyProjectiles;
            amountOfHits = spotHeroAttackSO.amountOfHits == 0 ? null : spotHeroAttackSO.amountOfHits;
            projectilesCountToHits = spotHeroAttackSO.projectilesCountToHits;
            lifeTime = spotHeroAttackSO.lifeTime == 0 ? null : spotHeroAttackSO.lifeTime;
            movementSpeed = spotHeroAttackSO.movementSpeed;
        }

        protected override void InheritLevelSetup()
        {

        }

        protected HeroAttackStats BuildBaseHeroAttackStats()
        {
            return new HeroAttackStats(
                damage.Value,
                knockback.Value,
                hitRefreshRate.Value,
                destroyProjectiles,
                amountOfHits,
                projectilesCountToHits
            );
        }

    }

}
