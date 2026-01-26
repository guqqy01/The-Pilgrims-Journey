using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Heros.Attacks.Orbital
{
    public class OrbitalHeroAttackSpawn : HeroAttackSpawn<OrbitalHeroAttackInitContext, OrbitalHeroAttackStatsContext>
    {
        private Transform targetPositionTransform;

        private float orbitSpeed;

        public delegate void OnDestroyed(OrbitalHeroAttackSpawn orbitalDestroyed, Transform target);
        public OnDestroyed onDestroyed;

        protected override void InheritInit(OrbitalHeroAttackInitContext context)
        {
            targetPositionTransform = context.targetPositionTransform;
        }

        protected override void InheritSetupStats(OrbitalHeroAttackStatsContext heroAttackContext)
        {
            //Multiplied by 1.25 so it can catch up to the target if it needs to, otherwise the orbit speed will be controlled by the OrbitalHeroAttack through the targetTransform
            this.orbitSpeed = heroAttackContext.orbitSpeed * 1.25f;
        }

        protected override void FixedUpdateMovement()
        {
            if (targetPositionTransform == null)
            {
                DestroyAttack();
                return;
            }

            Vector3 toVector = targetPositionTransform.position - transform.position;
            Vector3 normalizedDirection = toVector.normalized;

            float distanceMovedThisFrame = orbitSpeed * Time.fixedDeltaTime;

            if (toVector.sqrMagnitude <= distanceMovedThisFrame * distanceMovedThisFrame)
            {
                transform.position = targetPositionTransform.position;
            }
            else
            {
                transform.position += normalizedDirection * distanceMovedThisFrame;
            }
        }

        public override void DestroyAttack()
        {
            onDestroyed?.Invoke(this, targetPositionTransform);

            base.DestroyAttack();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            onDestroyed = null;
        }
    }

    public class OrbitalHeroAttackStatsContext : HeroAttackStatsContext
    {
        public readonly float orbitSpeed;

        public OrbitalHeroAttackStatsContext(int damage, float orbitSpeed, float knockback, float hitSpeed, bool destroyProjectiles = false, int? amountOfHits = null, bool projectilesCountAsHit = false) : base(damage, knockback, hitSpeed, destroyProjectiles, amountOfHits, projectilesCountAsHit)
        {
            this.orbitSpeed = orbitSpeed;
        }
    }

    public class OrbitalHeroAttackInitContext : HeroAttackInitContext
    {
        public readonly Transform targetPositionTransform;

        public OrbitalHeroAttackInitContext(Transform targetPositionTransform)
        {
            this.targetPositionTransform = targetPositionTransform;
        }
    }
}

