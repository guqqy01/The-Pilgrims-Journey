using System.Collections.Generic;
using System.Linq;
using Units;
using Unity.VisualScripting;
using UnityEngine;
using Attacks.Projectiles;
namespace Monsters
{
    public class Wasp : Monster
    {
        private static Dictionary<Wasp, Transform> targets = new Dictionary<Wasp, Transform>();

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
        private float knockbackMultiplier = 0.1f;

        [Header("Poison")]
        [SerializeField]
        private int poisonDamage;
        [SerializeField]
        private float poisonTime;


        private Transform target;

        [SerializeField]
        private Projectile waspStinger;

        protected override void ShockEffect(float shockTime)
        {
            nextAttackTime = Mathf.Max(Time.time, nextAttackTime) + shockTime;
        }

        protected override void OnDamageUnit(OnHitInfo hitInfo)
        {
            base.OnDamageUnit(hitInfo);

            if(hitInfo.DamageToHealth > 0)
            {
                if(hitInfo.HealthAndShieldManager.TryGetComponent(out PoisonDebuff poisonDebug))
                {
                    poisonDebug.Add(damage, poisonTime);
                }
            }

        }

        protected override void Update()
        {
            base.Update();

            if (target == null || Vector2.Distance(target.position, transform.position) > rangeToFindTarget)
            {
                if(targets.ContainsKey(this))
                {
                    targets.Remove(this);
                }

                target = GetClosestTargetInRange(targets.Values.NotNull().ToList());

                if(target != null)
                {
                    targets.Add(this, target);
                }
            }

            if (target != null)
            {
                if (Time.time >= nextAttackTime)
                {
                    //ani.SetTrigger("Shoot");

                    Projectile project = Instantiate(waspStinger, transform.position, Quaternion.identity, transform);

                    project.onHit += OnDamageUnit;

                    ProjectileStats projectileStats = new ProjectileStats(damage, 0, 0, knockbackMultiplier, projectileSpeed, projectileTurnSpeed);

                    project.Init(projectileStats, true, target.position - transform.position, findTargetMask, false, target, ProjectileOwner.Monster);

                    audioSource?.Play();

                    nextAttackTime = Time.time + 1f / attackSpeed;
                }
            }
        }
    }
}

