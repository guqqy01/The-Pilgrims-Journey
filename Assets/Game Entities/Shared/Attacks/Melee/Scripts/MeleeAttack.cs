using UnityEngine;
namespace Attacks.Melee
{
    public class MeleeAttack : Attack<MeleeAttackSO, MeleeAttackSpawn>
    {
        private BaseMovement movement;

        private void Awake()
        {
            movement = GetComponent<BaseMovement>();
        }

        protected override bool Attack()
        {
            float rotation;

            Vector2 inputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            if (inputDirection.sqrMagnitude > 0.1f)
            {
                rotation = Vector2.SignedAngle(Vector2.up, inputDirection);
            }
            else
            {
                rotation = movement.LastDirection switch
                {
                    Direction.North => 0f,
                    Direction.West => 90f,
                    Direction.South => 180f,
                    Direction.East => 270f,
                    _ => 0f // fallback
                };
            }

            MeleeAttackSpawn attackInstance = GetAttackSpawnFromPool(transform.position, Quaternion.Euler(0, 0, rotation), transform);

            attackInstance.Init(hitLayerMask, damage, criticalChance, criticalDamageMultiplier, destroyProjectiles, knockback);

            return true;
        }

        protected override void InheritSetup(MeleeAttackSO heroAttackSO)
        {
            destroyProjectiles = heroAttackSO.destroyProjectiles;
        }

        protected override void InheritLevelSetup()
        {

        }

        private Transform GetOrCreatePoolParent()
        {
            GameObject poolParent = GameObject.Find("Pools");
            if (poolParent == null)
            {
                poolParent = new GameObject("Pools");
            }
            return poolParent.transform;
        }
    }
}
