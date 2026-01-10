using Heros;
using UnityEngine;


namespace Monsters
{
    public enum MonsterMovementType
    {
        Stationary,
        Direct,
    }

    public class MonsterMovement : BaseMovement
    {
        public Vector2Int StationaryPosition;

        [SerializeField]
        private LayerMask directTargetLayerMask;

        [SerializeField]
        private float directStoppingDistance;

        [SerializeField]
        private MonsterMovementType movementType;

        [SerializeField]
        private bool mirrorWhenTargetToTheLeft = false;

        public override void HandleFixedUpdateMovement()
        {
            switch (movementType)
            {
                case MonsterMovementType.Stationary:
                    HandleStationaryMovement();
                    break;
                case MonsterMovementType.Direct:
                    HandleDirectMovement();
                    break;
            }
        }

        public void OverrideToDirect()
        {
            this.movementType = MonsterMovementType.Direct;
        }
        private void Start()
        {
            StationaryPosition =  new Vector2Int(Mathf.RoundToInt(rb.position.x), Mathf.RoundToInt(rb.position.y));
        }

        public void Setup(MonsterSO monsterSO)
        {
            BaseInit(monsterSO.movementSpeed);
        }

        private void HandleStationaryMovement()
        {
            Vector2 toVector = StationaryPosition - rb.position;
            float sqrDistance = toVector.sqrMagnitude;

            if (sqrDistance == 0f) return;

            Vector2 direction = toVector / Mathf.Sqrt(sqrDistance); // or just normalized
            Vector2 moveVector = Speed * Time.fixedDeltaTime * direction;

            if (moveVector.sqrMagnitude >= sqrDistance)
            {
                rb.MovePosition(StationaryPosition);
            }
            else
            {
                rb.MovePosition(moveVector + rb.position);
            }

            if (visualTransform != null && mirrorWhenTargetToTheLeft)
            {
                if (moveVector.x > 0)
                {
                    visualTransform.localScale = new Vector3(initialXScale, visualTransform.localScale.y, visualTransform.localScale.z);
                }
                else
                {
                    visualTransform.localScale = new Vector3(-initialXScale, visualTransform.localScale.y, visualTransform.localScale.z);
                }
            }
        }

        private void HandleDirectMovement()
        {
            Hero activeHero = HeroManager.Instance.ActiveHero;

            if (activeHero == null)
                return;

            Vector3 direction = (activeHero.transform.position - transform.position).normalized;

            Vector2 TargetPosition = activeHero.transform.position - direction * directStoppingDistance;

            float distance = Vector2.Distance(TargetPosition, rb.position);

            if (distance == 0)
                return;

            Vector2 moveVector = Speed * Time.fixedDeltaTime * direction;

            if (moveVector.sqrMagnitude > distance * distance)
            {
                rb.MovePosition(TargetPosition);
            }
            else
            {
                rb.MovePosition(moveVector + rb.position);
            }

            if(visualTransform != null && mirrorWhenTargetToTheLeft)
            {
                if (moveVector.x > 0)
                {
                    visualTransform.localScale = new Vector3(initialXScale, visualTransform.localScale.y, visualTransform.localScale.z);
                }
                else
                {
                    visualTransform.localScale = new Vector3(-initialXScale, visualTransform.localScale.y, visualTransform.localScale.z);
                }
            }
        }
    }
}