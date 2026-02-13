using Heros;
using UnityEngine;
namespace Monsters
{
    public class SkullBeetle : Monster
    {
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int MovingHash = Animator.StringToHash("Moving");
        private bool isMoving;

        [SerializeField]
        private MonsterAnimationHelper deathAnimationHelper;

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

        private Transform target;

        [SerializeField]
        private LayerMask attackHitMask;

        protected override void ShockEffect(float shockTime)
        {
            throw new System.NotImplementedException();
        }

        protected override void Start()
        {
            base.Start();

            deathAnimationHelper.onAttack += FromAnimatorShootProjectiles;

            deathAnimationHelper.onAttackFinish += OnAttackFinished;
        }


        protected override void Update()
        {
            base.Update();

            bool shouldMove = HeroManager.Instance.ActiveHero != null;
            if (shouldMove != isMoving)
            {
                isMoving = shouldMove;
                animator.SetBool(MovingHash, isMoving);
            }

            HandleAttack();
        }

        private void HandleAttack()
        {
            if (target == null || Vector2.Distance(target.position, transform.position) > rangeToFindTarget)
                target = GetClosestTargetInRange();

            if (target != null)
            {
                if (Time.time >= nextAttackTime)
                {
                    PlayAttackAnimation();
                }
            }
        }

        public void FromAnimatorShootProjectiles()
        {
            if (HeroManager.Instance.ActiveHero == null)
                return;

            Projectile project = Instantiate(projectile, transform.position, Quaternion.identity, transform);

            project.onHit += OnDamageUnit;

            ProjectileStats projectileStats = new ProjectileStats(damage, 0, 0, knockbackMultiplier, projectileSpeed, projectileTurnSpeed);

            project.Init(projectileStats, false, HeroManager.Instance.ActiveHero.transform.position - transform.position, attackHitMask, false, null, HealthAndShieldManager);
        }

        private void PlayAttackAnimation()
        {
            animator.SetTrigger(AttackHash);

            MonsterMovement.Moving = false;

            nextAttackTime = Time.time + 1f / attackSpeed;
        }

        public void OnAttackFinished()
        {
            MonsterMovement.Moving = true;
        }
    }
}

