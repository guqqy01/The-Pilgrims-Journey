using UnityEngine;
using System;
using Heros;
namespace Attacks.Spot
{
    [CreateAssetMenu(menuName = "Attack/Spot/Random Movement")]
    public class RandomMovementSpotAttackSO : SpotAttackSOBase<SpotAttackInit, RandomMovementSpotAttackStats>
    {
        public Vector2 randomChangeMovementTimeRange;

        public override Type AttackType => typeof(RandomMovementSpotAttack);

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