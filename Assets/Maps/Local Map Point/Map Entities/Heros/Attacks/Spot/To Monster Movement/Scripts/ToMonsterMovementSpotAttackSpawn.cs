using Monsters;
using UnityEngine;
namespace Attacks.Spot
{
    public class ToMonsterMovementSpotAttackSpawn : BaseRandomMovementSpotAttackSpawn<ToMonsterMovementSpotAttackStats>
    {
        private ToMonsterMovementStats toMonsterMovementStats;

        private Monster targetMonster;
        private float timeToNextSearch;

        private static readonly Collider2D[] buffer = new Collider2D[32];

        protected override void FixedUpdateMovement()
        {
            HandleFixedToMonsterMovement();
            HandleFixedChaoticMovement();
            HandleMovement();
        }

        private Monster FindTargetMonster()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, toMonsterMovementStats.searchRadius, hitLayerMask);

            if (colliders == null || colliders.Length == 0)
            {
                return null;
            }

            float closestSqrDistance = float.MaxValue;
            Monster closestMonster = null;
            foreach (Collider2D collider in colliders)
            {
                float currentSqrDist = Vector2.SqrMagnitude((Vector2)collider.transform.position - (Vector2)transform.position);

                if (currentSqrDist < closestSqrDistance && collider.TryGetComponentInParent(out Monster monster))
                {
                    closestSqrDistance = currentSqrDist;
                    closestMonster = monster;
                }
            }

            return closestMonster;
        }

        private void HandleFixedToMonsterMovement()
        {
            if (Time.time >= timeToNextSearch)
            {
                targetMonster = FindTargetMonster();
                timeToNextSearch = Time.time + toMonsterMovementStats.timeSearchDelay;
            }

            if (targetMonster == null)
            {
                HandleFixedRandomMovement();
                return;
            }

            mainTargetPosition = targetMonster.transform.position;

            Vector2 toVector = mainTargetPosition - mainCurrentPosition;
            Vector2 direction = toVector.normalized;

            float distanceMovedThisFixedFrame = movementSpeed * Time.fixedDeltaTime;

            if (Vector2.SqrMagnitude(toVector) <= distanceMovedThisFixedFrame * distanceMovedThisFixedFrame)
            {
                mainCurrentPosition = mainTargetPosition;
            }
            else
            {
                mainCurrentPosition += direction * distanceMovedThisFixedFrame;
            }
        }

        protected override void InheritSetupStats(ToMonsterMovementSpotAttackStats heroAttackContext)
        {
            toMonsterMovementStats = heroAttackContext.toMonsterMovementStats;

            base.InheritSetupStats(heroAttackContext);
        }
    }
}