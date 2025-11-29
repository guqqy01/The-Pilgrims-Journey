using System.Collections;
using UnityEngine;


namespace Monsters
{
    public class PlantMonster : Monster
    {
        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private float attackSpeed = 1f;

        [SerializeField]
        private float projectileSpeed = 5f;
        private float nextAttackTime;
        [SerializeField]
        private int damage = 1;

        [SerializeField]
        private float knockbackMulitplier;

        public Projectile projectile;

        public Transform target;


        protected override void Update()
        {
            base.Update();

            if (target == null || Vector2.Distance(target.position, transform.position) > range)
                target = GetClosestTargetInRange();

            if (target != null)
            {
                if (Time.time >= nextAttackTime)
                {
                    ani.SetTrigger("Shoot");

                    Projectile project = Instantiate(projectile, transform.position, Quaternion.identity, transform);

                    project.onHit += OnDamageUnit; 
                    
                    project.Init(damage, projectileSpeed, knockbackMulitplier, false, target, target.position - transform.position, targetMask);

                    audioSource?.Play();

                    nextAttackTime = Time.time + 1f / attackSpeed;
                }
            }
        }

        protected override void ShockEffect(float time)
        {
            nextAttackTime = Mathf.Max(Time.time, nextAttackTime) + time;
        }
    }
}