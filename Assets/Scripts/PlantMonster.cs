using System.Collections;
using UnityEngine;


namespace Monsters
{
    public class PlantMonster : MonoBehaviour
    {
        [SerializeField]
        private float range = 3f;

        [SerializeField]
        private float attackSpeed = 1f;

        [SerializeField]
        private float projectileSpeed = 5f;
        private float nextAttackTime;
        [SerializeField]
        private int damage = 1;

        public Projectile projectile;

        public Transform target;

        public LayerMask targetMask;

        [SerializeField]
        private Animator ani;

        public Transform GetClosestTargetInRange()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, targetMask);

            float closestDistance = float.MaxValue;
            Transform closestTarget = null;

            foreach (Collider2D collider in colliders)
            {
                float distance = Vector2.Distance(collider.transform.position, transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = collider.transform;
                }
            }

            return closestTarget;
        }



        private void Update()
        {
            if (target == null || Vector2.Distance(target.position, transform.position) > range)
                target = GetClosestTargetInRange();

            if (target != null)
            {
                if (Time.time >= nextAttackTime)
                {
                    ani.SetTrigger("Shoot");

                    Projectile project = Instantiate(projectile, transform.position, Quaternion.identity, transform);

                    project.Init(damage, projectileSpeed, false, target, target.position - transform.position, targetMask);  

                    nextAttackTime = Time.time + 1f / attackSpeed;
                }
            }
        }
    }

    
}