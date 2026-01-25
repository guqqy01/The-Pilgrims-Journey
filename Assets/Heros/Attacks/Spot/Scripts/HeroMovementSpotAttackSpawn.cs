using UnityEngine;
using Random = UnityEngine.Random;
namespace Heros.Attacks.Spot
{
    public class HeroMovementSpotAttackSpawn : SpotAttackSpawn<HeroMovementSpotAttackStatsContext>
    {
        private HeroSpotMovementType heroSpotMovementType;

        private Transform heroTransform;
        private Transform HeroTransform
        {
            get
            {
                if (heroTransform == null)
                {
                    heroTransform = HeroManager.Instance.ActiveHero.transform;
                }

                return heroTransform;
            }
        }

        protected override void FixedUpdateMovement()
        {
            switch (heroSpotMovementType)
            {
                case HeroSpotMovementType.ToHero:
                    HandleFixedToHeroMovement();
                    break;
                case HeroSpotMovementType.FromHero:
                    HandleFixedFromHeroMovement();
                    break;
                case HeroSpotMovementType.PingPong:
                    HandleFixedPingPongMovement();
                    break;
            }

            base.FixedUpdateMovement();
        }

        private HeroMovementSpotAttackStats heroMovementSpotAttackStats;

        private bool pingPongToHero = false;

        private void HandleFixedToHeroMovement()
        {
            if (HeroTransform == null) return;

            mainTargetPosition = HeroTransform.position;

            Vector2 toVector = mainTargetPosition - mainCurrentPosition;
            Vector2 direction = toVector.normalized;

            float distanceMovedThisFixedFrame = movementSpeed * Time.fixedDeltaTime;

            if (Vector2.SqrMagnitude(toVector) <= distanceMovedThisFixedFrame * distanceMovedThisFixedFrame)
            {
                mainCurrentPosition = mainTargetPosition;

                if(heroSpotMovementType == HeroSpotMovementType.ToHero)
                    DestroyAttack();
            }
            else
            {
                mainCurrentPosition += direction * distanceMovedThisFixedFrame;
            }
        }

        private void HandleFixedFromHeroMovement()
        {
            if (HeroTransform == null) return;

            Vector2 heroPos = HeroTransform.position;

            Vector2 direction;
            if (heroPos.Approximately(mainCurrentPosition))
            {
                direction = Random.insideUnitCircle.normalized;
            }
            else
            {
                direction = (mainCurrentPosition - (Vector2)HeroTransform.position).normalized;
            }

            float distanceMovedThisFixedFrame = movementSpeed * Time.fixedDeltaTime;

            mainCurrentPosition += direction * distanceMovedThisFixedFrame;

            if (Vector2.SqrMagnitude(mainCurrentPosition - (Vector2)HeroTransform.position) >= heroMovementSpotAttackStats.maxDistance * heroMovementSpotAttackStats.maxDistance)
            {
                if (heroSpotMovementType == HeroSpotMovementType.FromHero)
                    DestroyAttack();
            }
        }

        private void HandleFixedPingPongMovement()
        {
            if (pingPongToHero)
            {
                HandleFixedToHeroMovement();

                if (mainCurrentPosition.Approximately(mainTargetPosition))
                {
                    pingPongToHero = false;
                }
            }
            else
            {
                HandleFixedFromHeroMovement();

                if (Vector2.SqrMagnitude(mainCurrentPosition - (Vector2)HeroTransform.position) >= heroMovementSpotAttackStats.maxDistance * heroMovementSpotAttackStats.maxDistance)
                {
                    pingPongToHero = true;
                }
            }
        }

        protected override void InheritSetupStats(HeroMovementSpotAttackStatsContext heroAttackContext)
        {
            heroMovementSpotAttackStats = heroAttackContext.heroMovementSpotAttackStats;

            base.InheritSetupStats(heroAttackContext);
        }
    }
}