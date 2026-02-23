using Heros;
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