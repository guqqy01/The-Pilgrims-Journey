using UnityEngine;
using System;
using Heros;
namespace Attacks.Spot
{
    [CreateAssetMenu(menuName = "Hero Attack/Spot/Stationary")]
    public class StationarySpotHeroAttackSO : SpotAttackSOBase
    {
        public SpotAttackSpawn<SpotAttackInit, SpotAttackStats> spotAttackSpawnPrefab;

        public override Type AttackType => typeof(StationarySpotHeroAttack);
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
