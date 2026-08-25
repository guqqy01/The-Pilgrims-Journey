using System.Collections.Generic;
using UnityEngine;
using System;
namespace Attacks.Spot
{
    public abstract class SpotAttack<TInit, TStats> : 
        Attack
            <
            SpotAttackSOBase<TInit, TStats>,
            SpotAttackInit,
            SpotAttackStats
            >
        where TInit : SpotAttackInit
        where TStats : SpotAttackStats
    {
        protected BuffableStat spawnAccuracy;

        protected SpotChaoticMovementStats SpotChaoticMovementStats;

        private readonly List<ISpotAttackSpawn> spotAttackSpawns = new List<ISpotAttackSpawn>();

        private bool projectilesCountToHits;
        private float? lifeTime;
        private float movementSpeed;

        protected override bool ProtectedAttack()
        {
            Transform target = GetTarget();
            if (target == null)
                return false;

            SpawnAttack(target);
            return true;
        }

        protected virtual void SpawnAttack(Transform target)
        {
            SpotAttackInit init = BuildInit(hitLayerMask, target, lifeTime, movementSpeed);

            SpotAttackSpawn<TInit, TStats> attackSpawn = GetAttackSpawnFromPoolAndSetup<SpotAttackSpawn<TInit, TStats>>(init);

            spotAttackSpawns.Add(attackSpawn);
        }

        protected virtual SpotAttackInit BuildInit(LayerMask hitLayerMask, Transform target, float? lifeTime, float movementSpeed)
        {
            return new SpotAttackInit(
                hitLayerMask,
                (Vector2)target.position + UnityEngine.Random.insideUnitCircle * spawnAccuracy.Value,
                Quaternion.identity, 
                lifeTime.HasValue ? Time.time + lifeTime.Value : null, 
                movementSpeed);
        }

        protected override void InheritSetup(SpotAttackSOBase<TInit, TStats> spotAttackSO, Func<StatTypes, Buff> entityBuffCallback)
        {
            SpotChaoticMovementStats = spotAttackSO.spotChaoticMovementStats;

            SetupBuffableStat(ref spawnAccuracy, entityBuffCallback, StatTypes.Accuracy);
            SetupBuffableStat(ref hitRefreshRate, entityBuffCallback, StatTypes.SpotHitRefreshRate);

            destroyProjectiles = spotAttackSO.destroyProjectiles;
            amountOfHits = spotAttackSO.amountOfHits == 0 ? null : spotAttackSO.amountOfHits;
            projectilesCountToHits = spotAttackSO.projectilesCountToHits;
            lifeTime = spotAttackSO.lifeTime == 0 ? null : spotAttackSO.lifeTime;
            movementSpeed = spotAttackSO.movementSpeed;
        }

        protected override void InheritLevelSetup()
        {

        }

        protected override SpotAttackStats GetAttackStats()
        {
            AttackStats attackStats = new AttackStats(
                damage,
                size,
                criticalChance,
                criticalDamageMultiplier,
                knockback,
                hitRefreshRate,
                destroyProjectiles,
                amountOfHits,
                projectilesCountToHits
            );

            return new SpotAttackStats(attackStats, SpotChaoticMovementStats);
        }
    }

}
