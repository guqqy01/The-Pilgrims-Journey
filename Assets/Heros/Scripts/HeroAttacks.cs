using UnityEngine;

namespace Hero
{
    [RequireComponent(typeof (HeroMovement))]
    public class HeroAttacks : MonoBehaviour
    {
        private HeroMovement heroMovement;

        [SerializeField]
        private MeleeSwipeAttack meleeSwipeAttackPrefab;

        [SerializeField]
        private float cooldown = 1f;

        private float nextAttackTime;

        [SerializeField]
        private int damage = 3;

        private void Awake()
        {
            heroMovement = GetComponent<HeroMovement>();
        }

        private void Update()
        {
            if (Time.time >= nextAttackTime)
            {
                Attack();

                nextAttackTime = Time.time + cooldown;
            }
        }

        private void Attack()
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
            
            

            var attack = Instantiate(meleeSwipeAttackPrefab, transform.position, Quaternion.Euler(0, 0, rotation), transform);

            attack.Init(damage);
        }
    }
}

