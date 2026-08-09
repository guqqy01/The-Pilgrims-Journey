using System;
using UnityEngine;
using Attacks.Projectiles;
namespace Monsters
{
    public class SapHydra : Monster
    {

        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private float projectileSpeed = 5f;

        [SerializeField]
        private float projectileTurnSpeed = 10f;

        public Projectile projectile;

        public Transform target;

        private State currentState;
        private State CurrentState
        {
            get
            {
                return currentState;
            }
            set
            {
                currentState = value;

                switch (value)
                {
                    case State.Idle:
                        animator.SetBool("Rapid", false);
                        animator.SetBool("Multi", false);
                        animator.SetBool("Burst", false);
                        break;
                    case State.Burst:
                        animator.SetBool("Rapid", false);
                        animator.SetBool("Multi", false);
                        animator.SetBool("Burst", true);
                        break;
                    case State.Multi:
                        animator.SetBool("Rapid", false);
                        animator.SetBool("Multi", true);
                        animator.SetBool("Burst", false);
                        break;
                    case State.Rapid:
                        animator.SetBool("Rapid", true);
                        animator.SetBool("Multi", false);
                        animator.SetBool("Burst", false);
                        break;
                }
            }
        }

        [SerializeField]
        private WeightedChanceList<State> stateChance;

        [SerializeField]
        private float knockbackMultiplier;

        [Header("Rapid Attack")]
        [SerializeField]
        private int rapidAttackDamage;
        private float timeWhenStartedRapidAttack;
        [SerializeField]
        private Vector2 minAndMaxTimeToDoRapidAttack;
        private float timeToDoRapidAttack;

        [Header("Multishot Attack")]
        [SerializeField]
        private int multishotAttackDamage;
        private float timeWhenStartedMultishotAttack;
        [SerializeField]
        private Vector2 minAndMaxTimeToDoMultishotAttack;
        private float timeToDoMultishotAttack;

        [SerializeField]
        private float multishotAngle;
        [SerializeField]
        private float numberOfShotsInMultishot;

        [Header("Burst Attack")]
        [SerializeField]
        private int burstAttackDamage;
        [SerializeField]
        private float numberOfShotsInBurstAttack;



        protected override void Update()
        {
            base.Update();

            if (target == null || Vector2.Distance(target.position, transform.position) > rangeToFindTarget)
                target = GetClosestTargetInRange();

            HandleAttacks();
        }

        private void HandleAttacks()
        {
            if (CurrentState != State.Idle && target == null)
            {
                CurrentState = State.Idle;
                return;
            }

            switch (CurrentState)
            {
                case State.Idle:
                    HandleIdleState();
                    break;
                case State.Rapid:
                    HandleRapidState();
                    break;
                case State.Multi:
                    HandleMultiState();
                    break;
                default: 
                    break;
            }
        }

        private void HandleIdleState()
        {
            if (target == null)
                return;

            State state = stateChance.GetRandom();

            switch (state)
            {
                case State.Rapid:
                    CurrentState = State.Rapid;
                    timeWhenStartedRapidAttack = Time.time;
                    timeToDoRapidAttack = minAndMaxTimeToDoRapidAttack.RandomBetweenTwoValues();
                    break;
                case State.Multi:
                    CurrentState = State.Multi;
                    timeWhenStartedMultishotAttack = Time.time;
                    timeToDoMultishotAttack = minAndMaxTimeToDoMultishotAttack.RandomBetweenTwoValues();
                    break;
                case State.Burst:
                    CurrentState = State.Burst;
                    break;
            }
        }

        private void HandleRapidState()
        {
            if (timeWhenStartedRapidAttack + timeToDoRapidAttack > Time.time)
                return;

            CurrentState = State.Idle;
        }

        private void HandleMultiState()
        {
            if (timeWhenStartedMultishotAttack + timeToDoMultishotAttack > Time.time)
                return;

            CurrentState = State.Idle;
        }

        public void RapidAttack()
        {
            if (target == null)
                return;

            Projectile project = Instantiate(projectile, transform.position, Quaternion.identity, transform);

            project.onHit += OnDamageUnit;
            ProjectileStats projectileStats = new ProjectileStats(rapidAttackDamage, 0, 0, knockbackMultiplier, projectileSpeed, projectileTurnSpeed);

            project.Init(projectileStats, false, target.position - transform.position, findTargetMask, false, target, ProjectileOwner.Monster);

            audioSource?.Play();
        }

        public void MultishotAttack()
        {
            if (target == null)
                return;

            Vector2 v2 = target.position - transform.position;

            float addAngle = Mathf.Deg2Rad * (multishotAngle / (numberOfShotsInMultishot - 1));
            Vector2 currentDirection = v2.RotateCounterClockwise(Mathf.Deg2Rad * (multishotAngle / 2f));

            for (int i = 0; i < numberOfShotsInMultishot; i++)
            {
                Projectile project = Instantiate(projectile, transform.position, Quaternion.identity, transform);

                project.onHit += OnDamageUnit;
                ProjectileStats projectileStats = new ProjectileStats(rapidAttackDamage, 0, 0, knockbackMultiplier, projectileSpeed, projectileTurnSpeed);

                project.Init(projectileStats, false, currentDirection, findTargetMask, false, target, ProjectileOwner.Monster);

                currentDirection = currentDirection.RotateClockwise(addAngle);
            }

            audioSource.Play();
        }

        public void BurstAttack()
        {
            CurrentState = State.Idle;

            if (target == null)
                return;

            Vector2 currentDirection = target.position - transform.position;

            float addAngle = Mathf.Deg2Rad * (360f / (numberOfShotsInBurstAttack - 1));

            for (int i = 0; i < numberOfShotsInBurstAttack; i++)
            {
                Projectile project = Instantiate(projectile, transform.position, Quaternion.identity, transform);

                project.onHit += OnDamageUnit;
                ProjectileStats projectileStats = new ProjectileStats(rapidAttackDamage, 0, 0, knockbackMultiplier, projectileSpeed, projectileTurnSpeed);

                project.Init(projectileStats, false, currentDirection, findTargetMask, false, target, ProjectileOwner.Monster);

                currentDirection = currentDirection.RotateClockwise(addAngle);
            }

            audioSource.Play();
        }

        protected override void ShockEffect(float shockTime)
        {
            throw new System.NotImplementedException();
        }

        [Serializable]
        public enum State
        {
            Idle,
            Rapid,
            Multi,
            Burst,
        }
    }
}
