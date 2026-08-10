using UnityEngine;


namespace Monsters
{
    public enum MonsterMovementType
    {
        Stationary,
        Direct,
    }

    public abstract class MonsterMovement : BaseMovement
    {
        [SerializeField]
        protected bool mirrorWhenTargetToTheLeft = false;

        public void Setup(MonsterSO monsterSO)
        {
            BaseInit(monsterSO.movementSpeed);
        }

        protected abstract Vector2? GetTargetPosition();

        public override void HandleFixedUpdateMovement()
        {
            Vector2? nullableTargetPosition = GetTargetPosition();

            if (!nullableTargetPosition.HasValue)
                return;

            Vector2 targetPosition = nullableTargetPosition.Value;

            Vector2 toVector = targetPosition - rb.position;
            float sqrDistance = toVector.sqrMagnitude;

            if (sqrDistance == 0f) return;

            Vector2 direction = toVector.normalized;
            Vector2 moveVector = Speed * Time.fixedDeltaTime * direction;

            HandleMoveMonster(moveVector);

            HandleMirroring(moveVector.x);
        }



        protected void HandleMoveMonster(Vector2 moveVector)
        {
            Vector2? targetPosition = GetTargetPosition();

            Vector2 targetPos = targetPosition.HasValue ? targetPosition.Value : default;
            float sqrDistance = (targetPos - rb.position).sqrMagnitude;

            if (moveVector.sqrMagnitude >= sqrDistance)
            {
                rb.MovePosition(targetPos);
            }
            else
            {
                rb.MovePosition(moveVector + rb.position);
            }
        }

        protected void HandleMirroring(float xMovement)
        {
            if (visualTransform == null || !mirrorWhenTargetToTheLeft)
                return;

            visualTransform.localScale = new Vector3((xMovement > 0 ? 1 : -1 ) * initialXScale, visualTransform.localScale.y, visualTransform.localScale.z);
        }
    }
}