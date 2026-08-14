using System;
using UnityEngine;
using Attacks;
using Attacks.Projectiles;
namespace Monsters
{
    public class SapHydra : Monster
    {
        [SerializeField]
        private AudioSource audioSource;

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
        private AttacksManager attackManager;

        [Header("Rapid Attack")]
        private float timeWhenStartedRapidAttack;
        [SerializeField]
        private Vector2 minAndMaxTimeToDoRapidAttack;
        private float timeToDoRapidAttack;

        [SerializeField]
        private BaseAttackSO rapidAttackSO;

        [Header("Multishot Attack")]
        private float timeWhenStartedMultishotAttack;
        [SerializeField]
        private Vector2 minAndMaxTimeToDoMultishotAttack;
        private float timeToDoMultishotAttack;

        [SerializeField]
        private BaseAttackSO multishotAttackSO;

        [Header("Burst Attack")]
        [SerializeField]
        private BaseAttackSO burstAttackSO;

        protected override void Start()
        {
            base.Start();

            attackManager.AddAttack(rapidAttackSO);
            attackManager.AddAttack(multishotAttackSO);
            attackManager.AddAttack(burstAttackSO);

            attackManager.AttackTriggeredExternally(rapidAttackSO, true);
            attackManager.AttackTriggeredExternally(rapidAttackSO, true);
            attackManager.AttackTriggeredExternally(rapidAttackSO, true);
        }


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

            attackManager.TriggerAttack(rapidAttackSO);

            audioSource?.Play();
        }

        public void MultishotAttack()
        {
            if (target == null)
                return;

            attackManager.TriggerAttack(multishotAttackSO);

            audioSource.Play();
        }

        public void BurstAttack()
        {
            CurrentState = State.Idle;

            if (target == null)
                return;

            attackManager.TriggerAttack(burstAttackSO);

            audioSource.Play();
        }

        protected override void ShockEffect(float shockTime)
        {
            attackManager.DelayAllAttacks(shockTime);
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
