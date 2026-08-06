using UnityEngine;

namespace Heros.Attacks.Spot
{
    public class ToMonsterMovementSpotHeroAttack : SpotHeroAttack
    {
        protected override SpotAttackInitContext BuildInitContext(Transform target, float? lifeTime, float movementSpeed)
        {
            Vector2 currentPos = (Vector2)target.position + Random.insideUnitCircle * spawnAccuracy.Value;

            float? lifeTimeEnd = lifeTime.HasValue ? Time.time + lifeTime.Value : null;

            return new SpotAttackInitContext(currentPos, lifeTimeEnd, movementSpeed);
        }

        protected override SpotAttackStatsContext BuildStatsContext()
        {
            AttackStats heroAttackStats = BuildBaseHeroAttackStats();

            if (HeroAttackSO is not ToMonsterMovementSpotHeroAttackSO toMonsterSO)
                return null;

            RandomMovementStats randomMovementStats = new RandomMovementStats(toMonsterSO.randomChangeMovementTimeRange.randomChangeMovementTimeRange);

            return new ToMonsterMovementSpotAttackStatsContext(heroAttackStats, SpotChaoticMovementStats, randomMovementStats, toMonsterSO.toMonsterMovementStats);
        }
    }
}