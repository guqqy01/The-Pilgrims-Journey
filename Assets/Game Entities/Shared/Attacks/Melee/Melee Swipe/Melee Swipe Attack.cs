using UnityEngine;
namespace Attacks.Melee
{
    public class MeleeAttackSpawn : AttackSpawn<AttackInit, AttackStats>
    {
        protected override void FixedUpdateMovement()
        {
        }

        protected override void InheritInit(AttackInit initContext)
        {
        }

        protected override void InheritSetupStats(AttackStats statsContext)
        {
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            // Allow base to handle projectile destruction and hit counting, then apply immediate damage for melee on enter.
            base.OnTriggerEnter2D(collision);

            // Apply damage immediately on trigger enter (so melee is responsive)
            HandleColliderHits(collision);
        }
    }

}