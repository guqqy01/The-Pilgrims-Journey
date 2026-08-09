using UnityEngine;
using System;
using Heros;
namespace Attacks.Spot
{
    [CreateAssetMenu(menuName = "Hero Attack/Spot/Random Movement")]
    public class RandomMovementSpotAttackSO : SpotAttackSOBase
    {
        public SpotAttackSpawn<SpotAttackInit, RandomMovementSpotAttackStats> spotAttackSpawnPrefab;
        public Vector2 randomChangeMovementTimeRange;

        public override Type AttackType => typeof(RandomMovementSpotAttack);
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