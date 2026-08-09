using Heros;
using UnityEngine;
using Random = UnityEngine.Random;
namespace Attacks.Spot
{
    public class RandomMovementSpotAttack : SpotAttack<SpotAttackInit, RandomMovementSpotAttackStats>
    {
        protected override SpotAttackInit BuildInit(LayerMask hitLayerMask, Transform target, float? lifeTime, float movementSpeed)
        {
            Vector2 currentPos = (Vector2)target.position + Random.insideUnitCircle * spawnAccuracy.Value;

            float? lifeTimeEnd = lifeTime.HasValue ? Time.time + lifeTime.Value : null;

            return new SpotAttackInit(hitLayerMask, currentPos, lifeTimeEnd, movementSpeed);
        }

        protected override SpotAttackStats BuildStatsContext()
        {
            AttackStats attackStats = BuildBaseAttackStats();

            if (AttackSO is not RandomMovementSpotAttackSO randomAttackSO)
                return null;

            RandomMovementStats randomMovementStats = new RandomMovementStats(randomAttackSO.randomChangeMovementTimeRange);

            return new RandomMovementSpotAttackStats(attackStats, SpotChaoticMovementStats, randomMovementStats);
        }
    }
}