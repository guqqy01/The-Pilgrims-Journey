using UnityEngine;
using Random = UnityEngine.Random;
namespace Heros.Attacks.Spot
{
    public interface ISpotAttackSpawn
    {
        void Init(SpotAttackInitContext initContext, SpotAttackStatsContext statsContext);
    }

    public class SpotAttackSpawn<TSpotStatsContext> : HeroAttackSpawn<SpotAttackInitContext, TSpotStatsContext>, ISpotAttackSpawn where TSpotStatsContext : SpotAttackStatsContext
    {
        protected float? lifeEndTime;

        protected float movementSpeed;

        protected Vector2 mainCurrentPosition;
        protected Vector2 mainTargetPosition;

        protected Vector2 finalCurrentPosition => mainCurrentPosition + chaoticCurrentPosition;

        protected override void FixedUpdateMovement()
        {
            HandleFixedChaoticMovement();
            HandleMovement();
        }

        private void Update()
        {
            HandleLifeTime();
        }

        protected void HandleMovement()
        {
            transform.position = finalCurrentPosition;
        }

        private void HandleLifeTime()
        {
            if (lifeEndTime == null)
                return;

            if (Time.time < lifeEndTime)
                return;

            DestroyAttack();
        }



        #region Chaotic Movement

        private SpotChaoticMovementStats chaoticMovementStats;

        private Vector2 chaoticTargetPosition;
        private Vector2 chaoticCurrentPosition;

        private float timeToNextChaoticMovement;

        protected void HandleFixedChaoticMovement()
        {
            SetupNextChaoticTarget();

            Vector2 toVector = chaoticTargetPosition - chaoticCurrentPosition;
            Vector2 direction = toVector.normalized;

            float distanceMovedThisFixedFrame = chaoticMovementStats.chaoticMovementSpeed * Time.fixedDeltaTime;

            if (Vector2.SqrMagnitude(toVector) <= distanceMovedThisFixedFrame * distanceMovedThisFixedFrame)
            {
                chaoticCurrentPosition = chaoticTargetPosition;
            }
            else
            {
                chaoticCurrentPosition += direction * distanceMovedThisFixedFrame;
            }
        }

        private void SetupNextChaoticTarget()
        {
            if (Time.time <= timeToNextChaoticMovement && !(chaoticCurrentPosition).Approximately(chaoticTargetPosition))
                return;

            chaoticTargetPosition = Random.insideUnitCircle * chaoticMovementStats.chaoticMovementRadius;
            timeToNextChaoticMovement = Time.time + chaoticMovementStats.chaoticChangeMovementTimeRange.RandomBetweenTwoValues();
        }

        #endregion

        protected override void InheritInit(SpotAttackInitContext heroAttackContext)
        {
            mainCurrentPosition = heroAttackContext.currentPosition;
            lifeEndTime = heroAttackContext.lifeTimeEnd;
            movementSpeed = heroAttackContext.movementSpeed;
        }

        protected override void InheritSetupStats(TSpotStatsContext heroAttackContext)
        {
            chaoticMovementStats = heroAttackContext.spotChaoticMovementStats;

        }

        // Explicit non-generic Init so callers can use the base types at runtime (via interface)
        public void Init(SpotAttackInitContext initContext, SpotAttackStatsContext statsContext)
        {
            base.Init(initContext, (TSpotStatsContext)statsContext);
        }
    }
}