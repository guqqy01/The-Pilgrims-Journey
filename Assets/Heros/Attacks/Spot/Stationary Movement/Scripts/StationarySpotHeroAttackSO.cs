using UnityEngine;

namespace Heros.Attacks.Spot
{
    [CreateAssetMenu(menuName = "Hero Attack/Spot/Stationary")]
    public class StationarySpotHeroAttackSO : SpotHeroAttackSOBase
    {
        public SpotAttackSpawn<SpotAttackStatsContext> spotAttackSpawnPrefab;

        public override Type HeroAttackType => typeof(StationarySpotHeroAttack);
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
