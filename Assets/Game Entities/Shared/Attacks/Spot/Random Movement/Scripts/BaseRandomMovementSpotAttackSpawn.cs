using UnityEngine;
using Random = UnityEngine.Random;
namespace Attacks.Spot
{
    public abstract class BaseRandomMovementSpotAttackSpawn<TRandomMovementStatsContext> : SpotAttackSpawn<SpotAttackInit, TRandomMovementStatsContext> 
        where TRandomMovementStatsContext : RandomMovementSpotAttackStats
    {
        private RandomMovementStats randomMovementStats;

        private Vector2 randomDirection;

        private float timeToNextRandomMovement;

        private const float randomWanderDistance = 100f;

        protected override void FixedUpdateMovement()
        {
            HandleFixedRandomMovement();
            base.FixedUpdateMovement();
        }

        protected void HandleFixedRandomMovement()
        {
            SetupNextRandomDirection();

            mainTargetPosition = (Vector2)transform.position + randomDirection * randomWanderDistance;

            mainCurrentPosition += movementSpeed * Time.fixedDeltaTime * randomDirection;
        }

        private void SetupNextRandomDirection()
        {
            if (Time.time <= timeToNextRandomMovement)
                return;

            randomDirection = Random.insideUnitCircle.normalized;
            timeToNextRandomMovement = Time.time + randomMovementStats.randomChangeMovementTimeRange.RandomBetweenTwoValues();
        }

        protected override void InheritSetupStats(TRandomMovementStatsContext heroAttackContext)
        {
            randomMovementStats = heroAttackContext.randomMovementStats;

            base.InheritSetupStats(heroAttackContext);
        }
    }
}