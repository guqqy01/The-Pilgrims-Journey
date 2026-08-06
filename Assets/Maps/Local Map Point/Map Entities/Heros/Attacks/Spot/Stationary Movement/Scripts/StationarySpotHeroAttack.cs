using UnityEngine;

namespace Heros.Attacks.Spot
{
    public class StationarySpotHeroAttack : SpotHeroAttack
    {
        protected override SpotAttackInitContext BuildInitContext(Transform target, float? lifeTime, float movementSpeed)
        {
            Vector2 currentPos = (Vector2)target.position + Random.insideUnitCircle * spawnAccuracy.Value;

            float? lifeTimeEnd = lifeTime.HasValue ? Time.time + lifeTime.Value : null;

            return new SpotAttackInitContext(currentPos, lifeTimeEnd, 0);
        }

        protected override SpotAttackStatsContext BuildStatsContext()
        {
            AttackStats heroAttackStats = BuildBaseHeroAttackStats();

            return new SpotAttackStatsContext(heroAttackStats, SpotChaoticMovementStats);
        }
    }

}
