using Heros;
using UnityEngine;
using Random = UnityEngine.Random;
namespace Attacks.Spot
{
    public class RandomMovementSpotAttack : SpotAttack<SpotAttackInit, RandomMovementSpotAttackStats>
    {
        protected override SpotAttackStats GetAttackStats()
        {
            AttackStats attackStats = base.GetAttackStats();

            if (AttackSO is not RandomMovementSpotAttackSO randomAttackSO)
                return null;

            RandomMovementStats randomMovementStats = new RandomMovementStats(randomAttackSO.randomChangeMovementTimeRange);

            return new RandomMovementSpotAttackStats(attackStats, SpotChaoticMovementStats, randomMovementStats);
        }
    }
}