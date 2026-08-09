using UnityEngine;
using System;
using Heros;
namespace Attacks.Spot
{
    [CreateAssetMenu(menuName = "Hero Attack/Spot/To Monster Movement")]
    public class ToMonsterMovementSpotHeroAttackSO : SpotAttackSOBase
    {
        public SpotAttackSpawn<SpotAttackInit, ToMonsterMovementSpotAttackStats> spotAttackSpawnPrefab;
        public RandomMovementStats randomChangeMovementTimeRange;
        public ToMonsterMovementStats toMonsterMovementStats;

        public override Type AttackType => typeof(ToMonsterMovementSpotHeroAttack);
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