using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Attacks.Orbitals
{
    public class OrbitalAttackSpawn : AttackSpawn<OrbitalAttackInit, OrbitalAttackStats>
    {
        private Transform targetPositionTransform;

        private float orbitSpeed;

        public delegate void OnDestroyed(OrbitalAttackSpawn orbitalDestroyed, Transform target);
        public OnDestroyed onDestroyed;

        protected override void InheritInit(OrbitalAttackInit context)
        {
            targetPositionTransform = context.targetPositionTransform;
        }

        protected override void InheritSetupStats(OrbitalAttackStats heroAttackContext)
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
}

