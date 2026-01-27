using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heros.Attacks.Spot
{

    public abstract class SpotHeroAttackSO<TAttack, TSpawn, TInit, TStats> : SpotHeroAttackSOBase
        where TAttack : SpotHeroAttack<TSpawn, TInit, TStats>
        where TSpawn : SpotAttackSpawn<TStats>
        where TInit : SpotAttackInitContext
        where TStats : SpotAttackStatsContext
    {
        public override Type HeroAttackType => typeof(TAttack);
        
        public TSpawn spotAttackSpawnPrefab;

        public override GameObject SpawnPrefab => spotAttackSpawnPrefab.gameObject;

        public override Buff GetBuffAtIndex(HeroStats heroStats, int index)
        {
            return heroStats switch
            {
                HeroStats.Accuracy => WeaponStatsAtLevel[index].spawnAccuracy,
                HeroStats.SpotHitRefreshRate => WeaponStatsAtLevel[index].hitRefreshRate,
                _ => null,
            };
        }
    }
}
