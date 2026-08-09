using System;
using UnityEngine;
using Heros;
namespace Attacks.Spot
{

    public abstract class SpotHeroAttackSO : SpotAttackSOBase
    {
        public SpotAttackSpawn<SpotAttackInit, SpotAttackStats> spotAttackSpawnPrefab;

        public override Type AttackType => null; // concrete SOs should override this if needed

        public override GameObject SpawnPrefab => spotAttackSpawnPrefab == null ? null : spotAttackSpawnPrefab.gameObject;

        public override Buff GetBuffAtIndex(StatTypes heroStats, int index)
        {
            return heroStats switch
            {
                StatTypes.Accuracy => WeaponStatsAtLevel[index].spawnAccuracy,
                StatTypes.SpotHitRefreshRate => WeaponStatsAtLevel[index].hitRefreshRate,
                _ => null,
            };
        }
    }

}
