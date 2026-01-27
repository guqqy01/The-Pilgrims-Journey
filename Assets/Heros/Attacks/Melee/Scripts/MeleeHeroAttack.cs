using UnityEngine;

namespace Heros.Attacks.Melee
{
    public class MeleeHeroAttack : HeroAttack<MeleeHeroAttackSO>
    {
        private HeroMovement heroMovement;
        private MeleeAttackSpawn meleeAttackPrefab;
        private bool destroyProjectiles;


        private void Awake()
        {
            heroMovement = GetComponent<HeroMovement>();
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
                rotation = heroMovement.LastDirection switch
                {
                    Direction.North => 0f,
                    Direction.West => 90f,
                    Direction.South => 180f,
                    Direction.East => 270f,
                    _ => 0f // fallback
                };
            }

            var attack = Instantiate(meleeAttackPrefab, transform.position, Quaternion.Euler(0, 0, rotation), transform);

            attack.Init(damage, destroyProjectiles, knockback);

            return true;
        }

        protected override void InheritSetup(HeroSO heroSO, MeleeHeroAttackSO heroAttackSO)
        {
            meleeAttackPrefab = heroAttackSO.meleeAttackSpawn;
            destroyProjectiles = heroAttackSO.destroyProjectiles;
        }

        protected override void InheritLevelSetup()
        {
            
        }
    }
}
