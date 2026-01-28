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

        protected SpotAttackSpawn<SpotAttackStatsContext> spawnPrefab;

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
            if (spawnPrefab == null)
            {
                Debug.LogError("Spawn prefab not set on SpotHeroAttack");
                return;
            }

            GameObject instance = Instantiate(spawnPrefab.gameObject);

            var spawn = instance.GetComponent<ISpotAttackSpawn>();
            if (spawn == null)
            {
                Debug.LogError($"Spawn prefab {spawnPrefab.gameObject.name} does not implement ISpotAttackSpawn");
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

            var go = spotHeroAttackSO.SpawnPrefab;
            if (go == null)
            {
                Debug.LogError("SpotHeroAttackSO SpawnPrefab is null");
            }
            else
            {
                var comp = go.GetComponent<SpotAttackSpawn<SpotAttackStatsContext>>();
                if (comp != null)
                {
                    spawnPrefab = comp;
                }
                else
                {
                    // try to find any SpotAttackSpawn on the prefab
                    var any = go.GetComponentInChildren<SpotAttackSpawn<SpotAttackStatsContext>>();
                    if (any != null)
                        spawnPrefab = any;
                }
            }

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
