using UnityEngine;
namespace Heros.Attacks.Spot
{
    [CreateAssetMenu(menuName = "Hero Attack/Spot/Random Movement")]
    public class RandomMovementSpotHeroAttackSO : SpotHeroAttackSOBase
    {
        public SpotAttackSpawn<RandomMovementSpotAttackStatsContext> spotAttackSpawnPrefab;
        public Vector2 randomChangeMovementTimeRange;
        public ToMonsterMovementStats toMonsterMovementStats;

        public override Type HeroAttackType => typeof(RandomMovementSpotHeroAttack);
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