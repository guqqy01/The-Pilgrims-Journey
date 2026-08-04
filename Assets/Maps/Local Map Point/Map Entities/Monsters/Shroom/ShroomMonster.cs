using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Units;

namespace Monsters
{
    public class ShroomMonster : Monster
    {
        [SerializeField]
        private float attackSpeed = 1f;

        private float nextAttackTime;
        [SerializeField]
        private int damage = 1;

        [SerializeField]
        private ParticleSystem ps;

        [SerializeField]
        private AudioSource popSource;


        protected override void Update()
        {
            base.Update();

            if (Time.time >= nextAttackTime)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, rangeToFindTarget, findTargetMask);

                if (colliders.Length > 0)
                {
                    animator.SetTrigger("Attack");

                    nextAttackTime = Time.time + 1f / attackSpeed;
                }
            }
        }

        public void Attack()
        {
            ps.Play();

            popSource.Play();

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, rangeToFindTarget, findTargetMask);

            for (int i = 0; i < colliders.Length; i++)
            {
                if (1 << colliders[i].gameObject.layer == findTargetMask)
                {
                    if (colliders[i].TryGetComponentInParent(out HealthAndShieldManager healthAndShieldManager))
                    {
                        OnHitInfo hitInfo = healthAndShieldManager.Damage(damage, (healthAndShieldManager.transform.position - transform.position).normalized);

                        OnDamageUnit(hitInfo);
                    }
                }
            }
        }
        protected override void ShockEffect(float time)
        {
            nextAttackTime = Mathf.Max(Time.time, nextAttackTime) + time;
        }
    }
}
