using System;
using UnityEngine;
using Random = UnityEngine.Random;
namespace Heros.Attacks.Spot
{

    public class SpotAttackSpawn<TSpotStatsContext> : HeroAttackSpawn<SpotAttackInitContext, TSpotStatsContext> where TSpotStatsContext : SpotAttackStatsContext
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
        }

        protected override void InheritSetupStats(TSpotStatsContext heroAttackContext)
        {
            chaoticMovementStats = heroAttackContext.spotChaoticMovementStats;
        }
    }

    [Serializable]
    public struct ToMonsterMovementStats
    {
        public float timeSearchDelay;
        public float searchRadius;
    }
    [Serializable]
    public struct HeroMovementSpotAttackStats
    {
        public float maxDistance;
    }
    [Serializable]
    public struct RandomMovementStats
    {
        public Vector2 randomChangeMovementTimeRange;

        public RandomMovementStats(Vector2 randomChangeMovementTimeRange)
        {
            this.randomChangeMovementTimeRange = randomChangeMovementTimeRange;
        }
    }

    [Serializable]
    public struct SpotChaoticMovementStats
    {
        public float chaoticMovementSpeed;
        public float chaoticMovementRadius;

        public Vector2 chaoticChangeMovementTimeRange;

        public SpotChaoticMovementStats(float chaoricMovementSpeed, float chaoticMovementRadius, Vector2 chaoticChangeMovementTimeRange)
        {
            this.chaoticMovementSpeed = chaoricMovementSpeed;
            this.chaoticMovementRadius = chaoticMovementRadius;
            this.chaoticChangeMovementTimeRange = chaoticChangeMovementTimeRange;
        }
    }

    public class SpotAttackInitContext : HeroAttackInitContext
    {
        public readonly Vector2 currentPosition;
        public readonly float? lifeTimeEnd;

        public SpotAttackInitContext(Vector2 currentPosition, float? lifeTimeEnd)
        {
            this.currentPosition = currentPosition;
            this.lifeTimeEnd = lifeTimeEnd;
        }
    }

    public class SpotAttackStatsContext : HeroAttackStatsContext
    {
        public readonly SpotChaoticMovementStats spotChaoticMovementStats;
        public SpotAttackStatsContext(  HeroAttackStats heroAttackStats, 
                                        SpotChaoticMovementStats spotChaoticMovementStats) : 
                                        base(heroAttackStats)
        {
            this.spotChaoticMovementStats = spotChaoticMovementStats;
        }
    }

    public class HeroMovementSpotAttackStatsContext : SpotAttackStatsContext
    {
        public readonly HeroMovementSpotAttackStats heroMovementSpotAttackStats;
        public HeroMovementSpotAttackStatsContext(  HeroAttackStats heroAttackStats, 
                                                        SpotChaoticMovementStats spotChaoticMovementStats,
                                                        HeroMovementSpotAttackStats heroMovementSpotAttackStats) : 
                                                        base(heroAttackStats, spotChaoticMovementStats)
        {
            this.heroMovementSpotAttackStats = heroMovementSpotAttackStats;
        }
    }

    public class RandomMonsterMovementSpotAttackStatsContext : SpotAttackStatsContext
    {
        public readonly RandomMovementStats randomMovementStats;

        public RandomMonsterMovementSpotAttackStatsContext(HeroAttackStats heroAttackStats,
                                        SpotChaoticMovementStats spotChaoticMovementStats,
                                        RandomMovementStats randomMovementStats) :
                                        base(heroAttackStats, spotChaoticMovementStats)
        {
            this.randomMovementStats = randomMovementStats;
        }
    }

    public class ToMonsterMovementSpotAttackStatsContext : RandomMonsterMovementSpotAttackStatsContext
    {
        public readonly ToMonsterMovementStats toMonsterMovementStats;

        public ToMonsterMovementSpotAttackStatsContext(HeroAttackStats heroAttackStats,
                                        SpotChaoticMovementStats spotChaoticMovementStats,
                                        RandomMovementStats randomMovementStats,
                                        ToMonsterMovementStats toMonsterMovementStats) :
                                        base(heroAttackStats, spotChaoticMovementStats, randomMovementStats)
        {
            this.toMonsterMovementStats = toMonsterMovementStats;
        }
    }
}