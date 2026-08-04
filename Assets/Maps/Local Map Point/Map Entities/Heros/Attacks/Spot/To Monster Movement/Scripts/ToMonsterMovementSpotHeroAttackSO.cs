using UnityEngine;
using System;
namespace Heros.Attacks.Spot
{
    [CreateAssetMenu(menuName = "Hero Attack/Spot/To Monster Movement")]
    public class ToMonsterMovementSpotHeroAttackSO : SpotHeroAttackSOBase
    {
        public SpotAttackSpawn<ToMonsterMovementSpotAttackStatsContext> spotAttackSpawnPrefab;
        public RandomMovementStats randomChangeMovementTimeRange;
        public ToMonsterMovementStats toMonsterMovementStats;

        public override Type HeroAttackType => typeof(ToMonsterMovementSpotHeroAttack);
        public override GameObject SpawnPrefab => spotAttackSpawnPrefab == null ? null : spotAttackSpawnPrefab.gameObject;

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