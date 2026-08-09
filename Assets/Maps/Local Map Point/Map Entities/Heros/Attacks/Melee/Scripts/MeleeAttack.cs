using UnityEngine;
namespace Attacks.Melee
{
    public class MeleeAttack : Attack<MeleeAttackSO>
    {
        private BaseMovement movement;
        private MeleeAttackSpawn meleeAttackPrefab;

        // pooling
        private AttackSpawnPool meleeAttackPool;

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

            MeleeAttackSpawn attackInstance = null;

            if (meleeAttackPool != null)
            {
                var pooled = meleeAttackPool.Get(transform.position, Quaternion.Euler(0, 0, rotation));
                attackInstance = pooled as MeleeAttackSpawn;
                if (attackInstance == null)
                {
                    // unexpected type fallback
                    Destroy(pooled.gameObject);
                    attackInstance = Instantiate(meleeAttackPrefab, transform.position, Quaternion.Euler(0, 0, rotation), transform);
                }
                else
                {
                    attackInstance.transform.SetParent(transform);
                }
            }
            else
            {
                attackInstance = Instantiate(meleeAttackPrefab, transform.position, Quaternion.Euler(0, 0, rotation), transform);
            }

            attackInstance.Init(hitLayerMask, damage, criticalChance, criticalDamageMultiplier, destroyProjectiles, knockback);

            return true;
        }

        protected override void InheritSetup(BuffableEntitySO buffableEntitySO, MeleeAttackSO heroAttackSO)
        {
            meleeAttackPrefab = heroAttackSO.meleeAttackSpawn;
            destroyProjectiles = heroAttackSO.destroyProjectiles;

            if (meleeAttackPrefab != null)
            {
                meleeAttackPool = AttackSpawnPoolManager.GetPool(meleeAttackPrefab, GetOrCreatePoolParent());
            }
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
