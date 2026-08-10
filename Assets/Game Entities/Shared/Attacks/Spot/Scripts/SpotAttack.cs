using System.Collections.Generic;
using UnityEngine;
using Heros;
namespace Attacks.Spot
{
    public abstract class SpotAttack<TInit, TStats> : 
        Attack
            <
            SpotAttackSOBase<TInit, TStats>, 
            SpotAttackSpawn<TInit, TStats>
            >
        where TInit : SpotAttackInit
        where TStats : SpotAttackStats
    {
        protected BuffableStat spawnAccuracy;
        protected BuffableStat hitRefreshRate;

        protected SpotChaoticMovementStats SpotChaoticMovementStats;

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
            SpotAttackSpawn<TInit, TStats> attackSpawn = GetAttackSpawnFromPool(transform.position, Quaternion.identity);

            SpotAttackInit initContext = BuildInit(hitLayerMask, target, lifeTime, movementSpeed);
            SpotAttackStats statsContext = BuildStatsContext();

            attackSpawn.Init(initContext, statsContext);

            spotAttackSpawns.Add(attackSpawn);
        }

        protected virtual SpotAttackInit BuildInit(LayerMask hitLayerMask, Transform target, float? lifeTime, float movementSpeed)
        {
            return new SpotAttackInit(hitLayerMask, (Vector2)target.position + UnityEngine.Random.insideUnitCircle * spawnAccuracy.Value, lifeTime.HasValue ? Time.time + lifeTime.Value : (float?)null, movementSpeed);
        }

        protected virtual SpotAttackStats BuildStatsContext()
        {
            return new SpotAttackStats(BuildBaseAttackStats(), SpotChaoticMovementStats);
        }

        protected override void InheritSetup(SpotAttackSOBase<TInit, TStats> spotAttackSO)
        {
            SpotChaoticMovementStats = spotAttackSO.spotChaoticMovementStats;

            SetupBuffableStat(ref spawnAccuracy, StatTypes.Accuracy);
            SetupBuffableStat(ref hitRefreshRate, StatTypes.SpotHitRefreshRate);

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
            Debug.Log(amountOfHits);

            int? convertedAmountOfHits = amountOfHits == null ? null : amountOfHits.Value;

            return new AttackStats(
                damage.Value,
                criticalChance.Value,
                criticalDamageMultiplier.Value,
                knockback.Value,
                hitRefreshRate.Value,
                destroyProjectiles,
                convertedAmountOfHits,
                projectilesCountToHits
            );
        }

    }

}
