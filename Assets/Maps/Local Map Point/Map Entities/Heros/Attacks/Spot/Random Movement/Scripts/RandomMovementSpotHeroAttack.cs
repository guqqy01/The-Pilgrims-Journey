using UnityEngine;
using Random = UnityEngine.Random;
namespace Heros.Attacks.Spot
{
    public class RandomMovementSpotHeroAttack : SpotHeroAttack
    {
        protected override SpotAttackInitContext BuildInitContext(Transform target, float? lifeTime, float movementSpeed)
        {
            Vector2 currentPos = (Vector2)target.position + Random.insideUnitCircle * spawnAccuracy.Value;

            float? lifeTimeEnd = lifeTime.HasValue ? Time.time + lifeTime.Value : null;

            return new SpotAttackInitContext(currentPos, lifeTimeEnd, movementSpeed);
        }

        protected override SpotAttackStatsContext BuildStatsContext()
        {
            HeroAttackStats heroAttackStats = BuildBaseHeroAttackStats();

            if (HeroAttackSO is not RandomMovementSpotHeroAttackSO randomHeroAttackSO)
                return null;

            RandomMovementStats randomMovementStats = new RandomMovementStats(randomHeroAttackSO.randomChangeMovementTimeRange);

            return new RandomMovementSpotAttackStatsContext(heroAttackStats, SpotChaoticMovementStats, randomMovementStats);
        }
    }
}