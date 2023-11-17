using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


namespace Monsters
{
    public class PlantMonster : MonoBehaviour
    {
        public float range;

        public float attackSpeed;
        private float nextAttackTime;

        public int damage;

        public Projectile projectile;


        public Transform target;

        public LayerMask targetMask;

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
                    Projectile project = Instantiate(projectile, transform.position, Quaternion.identity, transform);

                    project.Init(damage, attackSpeed, false, target, target.position - transform.position, targetMask);  

                    nextAttackTime = Time.time + 1f / attackSpeed;
                }
            }
        }
    }

    
}