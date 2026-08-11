using UnityEngine;

namespace Attacks.Spot
{
    public class ToMonsterMovementSpotAttack : SpotAttack<SpotAttackInit, ToMonsterMovementSpotAttackStats>
    {
        protected override SpotAttackStats GetAttackStats()
        {
            AttackStats heroAttackStats = base.GetAttackStats();

            if (AttackSO is not ToMonsterMovementSpotAttackSO toMonsterSO)
                return null;

            RandomMovementStats randomMovementStats = new RandomMovementStats(toMonsterSO.randomChangeMovementTimeRange.randomChangeMovementTimeRange);

            return new ToMonsterMovementSpotAttackStats(heroAttackStats, SpotChaoticMovementStats, randomMovementStats, toMonsterSO.toMonsterMovementStats);
        }
    }
}