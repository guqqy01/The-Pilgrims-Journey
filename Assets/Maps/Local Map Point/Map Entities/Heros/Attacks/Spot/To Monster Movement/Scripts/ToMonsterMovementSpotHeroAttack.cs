using UnityEngine;

namespace Attacks.Spot
{
    public class ToMonsterMovementSpotHeroAttack : SpotAttack
    {
        protected override SpotAttackInit BuildInit(Transform target, float? lifeTime, float movementSpeed)
        {
            Vector2 currentPos = (Vector2)target.position + Random.insideUnitCircle * spawnAccuracy.Value;

            float? lifeTimeEnd = lifeTime.HasValue ? Time.time + lifeTime.Value : null;

            return new SpotAttackInit(currentPos, lifeTimeEnd, movementSpeed);
        }

        protected override SpotAttackStats BuildStatsContext()
        {
            AttackStats heroAttackStats = BuildBaseAttackStats();

            if (AttackSO is not ToMonsterMovementSpotHeroAttackSO toMonsterSO)
                return null;

            RandomMovementStats randomMovementStats = new RandomMovementStats(toMonsterSO.randomChangeMovementTimeRange.randomChangeMovementTimeRange);

            return new ToMonsterMovementSpotAttackStats(heroAttackStats, SpotChaoticMovementStats, randomMovementStats, toMonsterSO.toMonsterMovementStats);
        }
    }
}