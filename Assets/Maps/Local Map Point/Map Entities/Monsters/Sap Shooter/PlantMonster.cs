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

        [SerializeField]
        private float projectileTurnSpeed = 10f;

        private float nextAttackTime;
        [SerializeField]
        private int damage = 1;

        [SerializeField]
        private float knockbackMultiplier;

        public Projectile projectile;

        public Transform target;


        protected override void Update()
        {
            base.Update();

            if (target == null || Vector2.Distance(target.position, transform.position) > rangeToFindTarget)
                target = GetClosestTargetInRange();

            if (target != null)
            {
                if (Time.time >= nextAttackTime)
                {
                    animator.SetTrigger("Shoot");

                    Projectile project = Instantiate(projectile, transform.position, Quaternion.identity, transform);

                    project.onHit += OnDamageUnit;

                    ProjectileStats projectileStats = new ProjectileStats(damage, 0, 0, knockbackMultiplier, projectileSpeed, projectileTurnSpeed);

                    project.Init(projectileStats, false, target.position - transform.position, findTargetMask, false, target, ProjectileOwner.Monster);

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