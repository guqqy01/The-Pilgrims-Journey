using Attacks.Projectiles;
using Heros;
using TMPro;
using UnityEngine;
namespace Monsters
{
    public class Death : Monster
    {
        private static readonly int AttackHash = Animator.StringToHash("Attack");

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

        private Vector2? targetPosition = null;

        protected override void Update()
        {
            base.Update();

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

        protected override void Start()
        {
            base.Start();

            deathAnimationHelper.onAttack += FromAnimatorShootProjectiles;

            deathAnimationHelper.onAttackFinish += OnAttackFinished;
        }

        private void OnDestroy()
        {
            if(deathAnimationHelper != null)
            {
                deathAnimationHelper.onAttack -= FromAnimatorShootProjectiles;

                deathAnimationHelper.onAttackFinish -= OnAttackFinished;
            }
            
        }

        protected override void ShockEffect(float shockTime)
        {
            
        }

        public void FromAnimatorShootProjectiles()
        {
            if (HeroManager.Instance.ActiveHero == null)
                return;

            if (!targetPosition.HasValue)
                return;

            Projectile project = Instantiate(projectile, transform.position, Quaternion.identity);
            project.transform.localScale = transform.localScale;

            project.onHit += OnDamageUnit;

            ProjectileStats projectileStats = new ProjectileStats(damage, 1, 0, 0, knockbackMultiplier, projectileSpeed, projectileTurnSpeed);
            ProjectileInit projectileInit = new ProjectileInit(attackHitMask, transform.position, Quaternion.identity, false, false, target, targetPosition.Value - (Vector2)transform.position, null, HealthAndShieldManager);
            project.Init(projectileInit, projectileStats);

            targetPosition = null;
        }

        private void PlayAttackAnimation()
        {
            animator.SetTrigger(AttackHash);

            MonsterMovement.Moving = false;
            targetPosition = target.position;
            nextAttackTime = Time.time + 1f / attackSpeed;
        }

        public void OnAttackFinished()
        {
            MonsterMovement.Moving = true;
        }
    }

}
