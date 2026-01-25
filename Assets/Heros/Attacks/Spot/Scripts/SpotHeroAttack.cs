using Heros.Attacks.Orbital;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heros.Attacks.Spot
{
    public class SpotHeroAttack<TSpotAttackSpawn> : HeroAttack where TSpotAttackSpawn : SpotAttackStatsContext
    {
        private BuffableStat spawnAccuracy;

        private SpotAttackSpawn<TSpotAttackSpawn> spotAttackSpawnPrefab;

        private readonly List<SpotAttackSpawn<TSpotAttackSpawn>> spotAttackSpawns = new List<SpotAttackSpawn<TSpotAttackSpawn>>();

        private bool destroyProjectiles;
        private int? amountOfHits;
        private bool projectilesCountToHits;

        private bool SpawnSpotAttack()
        {
            SpotAttackSpawn<TSpotAttackSpawn> orbitalHeroAttackSpawn = Instantiate(spotAttackSpawnPrefab, transform.position, Quaternion.identity, transform);

            SpotAttackInitContext initContext = new SpotAttackInitContext();

            HeroAttackStats heroStats = new HeroAttackStats(damage, knockback, attackSpeed, destroyProjectiles, amountOfHits, projectilesCountToHits);

            SpotChaoticMovementStats spotChaoticMovementStats = new SpotChaoticMovementStats();

            SpotAttackStatsContext statsContext = new SpotAttackStatsContext(heroStats, );

            orbitalHeroAttackSpawn.Init(initContext, statsContext);

            spotAttackSpawns.Add(orbitalHeroAttackSpawn);

            return true;
        }

        protected override bool Attack()
        {
            return SpawnSpotAttack();
        }

        protected override void InheritLevelSetup()
        {
            foreach (var spotAttack in spotAttackSpawns)
            {
                if (spotAttack == null) continue;

                SpotAttackStatsContext statsContext = new SpotAttackStatsContext(damage, knockback, attackSpeed, destroyProjectiles, amountOfHits, projectilesCountToHits);

                spotAttack.SetupStats(statsContext);
            }
        }

        protected override void InheritSetup(HeroSO heroSO, HeroAttackSO heroAttackSO)
        {
            if (heroAttackSO is not SpotHeroAttackSO spotHeroAttackSO)
                return;

            SetupBuffableStat(ref spawnAccuracy, HeroStats.Accuracy);


            spotAttackSpawnPrefab = spotHeroAttackSO.spotAttackSpawnPrefab;
            destroyProjectiles = spotHeroAttackSO.destroyProjectiles;
            amountOfHits = spotHeroAttackSO.amountOfHits == 0 ? null : spotHeroAttackSO.amountOfHits;
            projectilesCountToHits = spotHeroAttackSO.projectilesCountToHits;
        }
    }
}
