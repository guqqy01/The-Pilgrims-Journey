using Heros.Attacks.Orbital;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Heros.Attacks.Spot
{
    public abstract class SpotHeroAttack<TSpawn, TInit, TStats> : HeroAttack
        where TSpawn : SpotAttackSpawn<TStats>
        where TInit : SpotAttackInitContext
        where TStats : SpotAttackStatsContext
    {
        protected BuffableStat spawnAccuracy;
        protected BuffableStat hitRefreshStat;

        protected SpotChaoticMovementStats SpotChaoticMovementStats;

        protected TSpawn spawnPrefab;

        private readonly List<TSpawn> spotAttackSpawns = new List<TSpawn>();

        private bool destroyProjectiles;
        private int? amountOfHits;
        private bool projectilesCountToHits;
        private float? lifeTime;


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
            TSpawn spawn = Instantiate(spawnPrefab);

            TInit initContext = BuildInitContext(target, lifeTime);
            TStats statsContext = BuildStatsContext();

            spawn.Init(initContext, statsContext);
        }

        protected abstract TInit BuildInitContext(Transform target, float? lifeTime);
        protected abstract TStats BuildStatsContext();

        protected override void InheritSetup(HeroSO heroSO, HeroAttackSO heroAttackSO)
        {
            if(heroAttackSO is not SpotHeroAttackSOBase spotHeroAttackSO)
                return;

            SpotChaoticMovementStats = spotHeroAttackSO.spotChaoticMovementStats;

            SetupBuffableStat(ref spawnAccuracy, HeroStats.Accuracy);
            SetupBuffableStat(ref hitRefreshStat, HeroStats.AttackSpeed, AddBuffParameters.Multiplier);

            hitRefreshStat.AddBuff("hitRefresh", spotHeroAttackSO.weaponStats[Level - 1].hitRefreshRate);
            spawnPrefab = spotHeroAttackSO.SpawnPrefab.GetComponent<TSpawn>();
            destroyProjectiles = spotHeroAttackSO.destroyProjectiles;
            amountOfHits = spotHeroAttackSO.amountOfHits == 0 ? null : spotHeroAttackSO.amountOfHits;
            projectilesCountToHits = spotHeroAttackSO.projectilesCountToHits;
            lifeTime = spotHeroAttackSO.lifeTime == 0 ? null : spotHeroAttackSO.lifeTime;
        }

        protected override void InheritLevelSetup()
        {
            
        }

        protected HeroAttackStats BuildBaseHeroAttackStats()
        {
            float hitRefreshRate = HeroAttackSO.WeaponStats.Select(x => (SpotWeaponStatsAtLevel)x).ToArray()[Level - 1].hitRefreshRate.Value(0);

            return new HeroAttackStats(
                damage.Value,
                knockback.Value,
                hitRefreshRate,
                destroyProjectiles,
                amountOfHits,
                projectilesCountToHits
            );
        }

    }

}
