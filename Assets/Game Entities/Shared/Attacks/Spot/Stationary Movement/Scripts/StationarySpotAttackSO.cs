using UnityEngine;
using System;
using Heros;
namespace Attacks.Spot
{
    [CreateAssetMenu(menuName = "Hero Attack/Spot/Stationary")]
    public class StationarySpotAttackSO : SpotAttackSOBase<SpotAttackInit, SpotAttackStats>
    {
        public override Type AttackType => typeof(StationarySpotAttack);

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
