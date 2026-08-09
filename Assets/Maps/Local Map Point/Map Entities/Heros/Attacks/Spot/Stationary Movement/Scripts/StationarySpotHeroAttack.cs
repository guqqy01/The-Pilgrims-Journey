using UnityEngine;

namespace Attacks.Spot
{
    public class StationarySpotHeroAttack : SpotAttack
    {
        protected override SpotAttackInit BuildInit(LayerMask hitLayerMask, Transform target, float? lifeTime, float movementSpeed)
        {
            Vector2 currentPos = (Vector2)target.position + Random.insideUnitCircle * spawnAccuracy.Value;

            float? lifeTimeEnd = lifeTime.HasValue ? Time.time + lifeTime.Value : null;

            return new SpotAttackInit(hitLayerMask, currentPos, lifeTimeEnd, 0);
        }

        protected override SpotAttackStats BuildStatsContext()
        {
            AttackStats heroAttackStats = BuildBaseAttackStats();

            return new SpotAttackStats(heroAttackStats, SpotChaoticMovementStats);
        }
    }

}
