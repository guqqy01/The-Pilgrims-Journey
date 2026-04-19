using Heros;
using LocalMap;
using UnityEngine;


namespace Monsters
{
    public class DirectMonsterMovement : MonsterMovement
    {
        [SerializeField]
        private LayerMask directTargetLayerMask;

        [SerializeField]
        private float directStoppingDistance;

        private Transform _target;
        private Vector2? _targetPosition;

        [SerializeField] private float separationRadius = 0.8f;
        [SerializeField] private float separationStrength = 1.5f;
        [SerializeField] private LayerMask enemyLayerMask;
        public Vector2? TargetPosition
        {
            get
            {
                if (_target == null && _targetPosition == null)
                    return null;

                if (_target != null && _targetPosition != null)
                {
                    Debug.LogWarning("Target and target postion are set, defaulting to target");
                    return _target.position;
                }

                if (_target != null)
                {
                    Vector3 direction = (_target.position - transform.position).normalized;
                    Vector2 targetPosition = _target.position - direction * directStoppingDistance;

                    return targetPosition;
                }
                else
                {
                    return _targetPosition;
                }
            }
        }

        public override void HandleFixedUpdateMovement()
        {
            bool targetIsHero = _target != null && HeroManager.Instance.ActiveHero != null
        && _target == HeroManager.Instance.ActiveHero.transform;

            FlowField currentFlowField;

            if (targetIsHero)
            {
                currentFlowField = FlowFieldManager.Instance.CurrentHeroField;
            }
            else
            {
                if (TargetPosition.HasValue)
                {
                    currentFlowField = FlowFieldManager.Instance.GetField(TargetPosition.Value);
                }
                else
                {
                    currentFlowField = null;
                }
            }

            

            Vector2 flowDir = Vector2.zero;

            if (currentFlowField != null)
            {
                flowDir = currentFlowField.SampleDirection(rb.position);
            }
            
            if(currentFlowField == null || flowDir == Vector2.zero)
            {
                if (LocalMapGenerator.Instance.TryFindNearestFullyWalkable(transform.position, out Vector2 walkablePosition))
                {
                    flowDir = (walkablePosition - (Vector2)transform.position).normalized;
                }
            }

            Vector2 separation = GetSeparationForce();
            Vector2 finalDir = (flowDir + separation).normalized;
            Vector2 moveVector = Speed * Time.fixedDeltaTime * finalDir;

            // If the intended position isn't walkable, try axis-separated fallback
            Vector2 intendedPos = rb.position + moveVector;
            if (!LocalMapGenerator.Instance.IsPositionWalkable(intendedPos))
            {
                Vector2 moveX = new Vector2(moveVector.x, 0f);
                Vector2 moveY = new Vector2(0f, moveVector.y);

                bool xWalkable = LocalMapGenerator.Instance.IsPositionWalkable(rb.position + moveX);
                bool yWalkable = LocalMapGenerator.Instance.IsPositionWalkable(rb.position + moveY);

                if (xWalkable && !yWalkable)
                    moveVector = moveX;
                else if (yWalkable && !xWalkable)
                    moveVector = moveY;
                else if (!xWalkable && !yWalkable)
                    moveVector = Vector2.zero;
                // if both walkable, keep diagonal but this is an edge case
            }

            if ((finalDir == Vector2.zero || moveVector == Vector2.zero) && LocalMapGenerator.Instance.TryFindNearestFullyWalkable(rb.position, out Vector2 closestPos))
            {
                Vector2 toClosestDir = (closestPos - rb.position).normalized;
                moveVector = Speed * Time.fixedDeltaTime * toClosestDir;
            }

            HandleMoveMonster(moveVector);
            HandleMirroring(moveVector.x);
        }

        private Vector2 GetSeparationForce()
        {
            Vector2 force = Vector2.zero;

            // OverlapCircleNonAlloc to avoid allocations
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position, separationRadius, enemyLayerMask);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].gameObject == gameObject) continue;

                Vector2 away = (Vector2)transform.position - (Vector2)hits[i].transform.position;
                float strength = 1f - (away.magnitude / separationRadius);
                force += away.normalized * strength;
            }

            return force * separationStrength;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
            _targetPosition = null;
        }

        public void SetTargetPosition(Vector2 targetPosition)
        {
            _target = null;
            _targetPosition = targetPosition;
        }

        private void Start()
        {
            MakeHeroTarget();
        }

        public void MakeHeroTarget()
        {
            Hero activeHero = HeroManager.Instance.ActiveHero;

            if (activeHero == null)
            {
                _target = null;
                _targetPosition = null;
                return;
            }

            _target = activeHero.transform;
            _targetPosition = null;
        }

        protected override Vector2? GetTargetPosition()
        {
            return TargetPosition;
        }

        protected override bool GetIfAtTargetPosition()
        {
            if (!TargetPosition.HasValue)
                return true;

            return TargetPosition.Value.Approximately(rb.position);
        }
    }
}