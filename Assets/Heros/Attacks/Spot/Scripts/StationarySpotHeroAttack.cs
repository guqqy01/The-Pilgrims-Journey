using UnityEngine;

namespace Heros.Attacks.Spot
{
    public class StationarySpotHeroAttack : SpotHeroAttack<StationarySpotAttackSpawn, SpotAttackInitContext, SpotAttackStatsContext>
    {
        protected override SpotAttackInitContext BuildInitContext(Transform target, float? lifeTime)
        {
            Vector2 currentPos = (Vector2)target.position + Random.insideUnitCircle * spawnAccuracy.Value;

            float? lifeTimeEnd = lifeTime.HasValue ? Time.time + lifeTime.Value : null;

            return new SpotAttackInitContext(currentPos, lifeTimeEnd);
        }

        protected override SpotAttackStatsContext BuildStatsContext()
        {
            HeroAttackStats heroAttackStats = BuildBaseHeroAttackStats();

            return new SpotAttackStatsContext(heroAttackStats, SpotChaoticMovementStats);
        }
    }

}
