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

        private HeroMovement heroMovement;
        public HeroMovement HeroMovement
        {
            get
            {
                if(heroMovement == null)
                    heroMovement = FindFirstObjectByType<HeroMovement>();

                return heroMovement;
            }
        }

        [SerializeField]
        private MonsterMovementType movementType;

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
        protected override void Start()
        {
            base.Start();
            StationaryPosition =  new Vector2Int(Mathf.RoundToInt(rb.position.x), Mathf.RoundToInt(rb.position.y));
        }

        private void HandleStationaryMovement()
        {
            float distance = Vector2.Distance(StationaryPosition, rb.position);

            if (distance == 0)
                return;

            Vector2 toVector = StationaryPosition - rb.position;
            Vector2 direction = toVector.normalized;

            Vector2 moveVector = Speed * Time.fixedDeltaTime * direction;

            if(moveVector.sqrMagnitude > distance * distance)
            {
                rb.MovePosition(StationaryPosition);
            }
            else
            {
                rb.MovePosition(moveVector + rb.position);
            }
        }

        private void HandleDirectMovement()
        {
            if (HeroMovement == null)
                return;

            Vector2 direction = (HeroMovement.rb.position - rb.position).normalized;

            Vector2 TargetPosition = rb.position - HeroMovement.rb.position - direction * directStoppingDistance;

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
        }
    }
}