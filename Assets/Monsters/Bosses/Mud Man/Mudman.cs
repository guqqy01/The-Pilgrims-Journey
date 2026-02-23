using Heros;
using LocalMap;
using UnityEngine;

namespace Monsters
{
    public class Mudman : Monster
    {
        private static readonly int MovingHash = Animator.StringToHash("Moving");
        private static readonly int ShrinkHash = Animator.StringToHash("Shrink");
        private bool isMoving;

        [SerializeField]
        private float spawnRate = 1f;

        private float nextSpawnTime;

        [SerializeField]
        private MonsterSO miniMudmanSO;

        [SerializeField]
        private FloatRangeSlider timeToMove;

        private float nextTimeToShrinkAndMove;

        [BuffType(false)]
        [SerializeField]
        private Buff shrunkSpeedBuff;

        private const string shrunkBuffReference = "shrunk Buff";

        private bool shrunk
        {
            get
            {
                return animator.GetBool(ShrinkHash);
            }
            set
            {
                animator.SetBool(ShrinkHash, value);
            }
        }

        protected override void ShockEffect(float shockTime)
        {
            throw new System.NotImplementedException();
        }

        private void Awake()
        {
            nextTimeToShrinkAndMove = Time.time + Random.Range(timeToMove.Start, timeToMove.End);
        }

        protected override void Update()
        {
            base.Update();

            HandleShrinkAndGrow();
            HandleSpawningMiniMudman();

            bool shouldMove = HeroManager.Instance.ActiveHero != null;
            if (shouldMove != isMoving)
            {
                isMoving = shouldMove;
                animator.SetBool(MovingHash, isMoving);
            }
        }

        private void HandleShrink()
        {
            if (shrunk || Time.time < nextTimeToShrinkAndMove)
                return;

            shrunk = true;

            MonsterMovement.Speed.AddBuff(shrunkBuffReference, shrunkSpeedBuff);

            if(MonsterMovement is DirectMonsterMovement directMonsterMovement)
            {
                directMonsterMovement.SetTargetPosition(GetRandomPoint());
            }
        }

        private Vector2 GetRandomPoint()
        {
            return new Vector2(Random.Range(0f, LocalMapGenerator.Instance.MapSizeX), Random.Range(0f, LocalMapGenerator.Instance.MapSizeY));
        }

        private void HandleMoveWhileShrunk()
        {
            if (!shrunk)
                return;

            if (MonsterMovement.AtTargetPosition)
            {
                HandleGrow();
            }
        }

        private void HandleGrow()
        {
            if (!shrunk)
                return;

            shrunk = false;
            nextTimeToShrinkAndMove = Time.time + Random.Range(timeToMove.Start, timeToMove.End);
            MonsterMovement.Speed.RemoveBuff(shrunkBuffReference);

            if (MonsterMovement is DirectMonsterMovement directMonsterMovement)
            {
                directMonsterMovement.MakeHeroTarget();
            }
        }

        private void HandleShrinkAndGrow()
        {
            HandleShrink();
            HandleMoveWhileShrunk();
            HandleGrow();
        }

        private void HandleSpawningMiniMudman()
        {
            if (Time.time < nextSpawnTime)
                return;

            MonsterSpawnManager.Instance.SpawnMonster(miniMudmanSO, transform.position);

            nextSpawnTime = Time.time + 1f / spawnRate;
        }
    }
}

