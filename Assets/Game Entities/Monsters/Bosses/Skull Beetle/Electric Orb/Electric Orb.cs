using UnityEngine;
using Attacks.Projectiles;
namespace Monsters
{
    public class ElectricOrb : Monster
    {
        private Monster boss;

        [SerializeField]
        private LineRenderer lineRenderer;

        [Tooltip("Amount of shield regenerated each second")]
        [SerializeField]
        private float shieldRegenerationRate;

        private float amountToAddThisTick;

        [Header("Under Attack Settings")]
        private bool underAttack;

        private float timeToStartRegenerating = 3f;

        private float nextTimeToBeginRegeneration;

        [Header("Attack Settings")]
        [Tooltip("Amount of burst attacks per second")]
        [SerializeField]
        private float attackSpeed = 0.3f;
        [Tooltip("When in burst, how many attacks per second")]
        [SerializeField]
        private float burstAttackSpeed = 3f;

        [SerializeField]
        private int amountOfAttacksInBurst = 3;

        [SerializeField]
        private float projectileSpeed = 5f;

        private bool doingBurst;
        private int currentBurstAmount;

        [SerializeField]
        private float projectileTurnSpeed = 10f;

        private float nextAttackTime;
        private float nextBurstAttackTime;

        [SerializeField]
        private int damage = 3;

        [SerializeField]
        private float knockbackMultiplier;

        [SerializeField]
        private Projectile projectilePrefab;

        private Transform target;

        [SerializeField]
        private LayerMask attackHitMask;

        protected override void Start()
        {
            base.Start();

            HealthAndShieldManager.Shield.onShieldChange += OnShieldChange;
        }

        private void OnDestroy()
        {
            if(HealthAndShieldManager != null && HealthAndShieldManager.Shield != null)
                HealthAndShieldManager.Shield.onShieldChange -= OnShieldChange;
        }

        private void OnShieldChange(StatChangeInfo statChangeInfo)
        {
            if(statChangeInfo.HitInfoType == HitInfoType.Damage)
            {
                nextTimeToBeginRegeneration = Time.time + timeToStartRegenerating;
                underAttack = true;
                doingBurst = false;
            }
        }

        protected override void Update()
        {
            base.Update();

            HandleUnderAttack();

            TryStartBurstAttack();

            HandleBurstAttack();

            HandleFindBoss();

            HandleShieldBoss();

            HandleLineRendering();
        }

        private void HandleUnderAttack()
        {
            if (!underAttack)
                return;

            if(Time.time >= nextTimeToBeginRegeneration)
            {
                underAttack = false;
            }
        }

        private void TryStartBurstAttack()
        {
            if (doingBurst)
                return;

            if (!underAttack)
                return;

            if (!TargetValid())
                target = GetClosestTargetInRange();

            if (target != null)
            {
                if (Time.time >= nextAttackTime)
                {
                    doingBurst = true;
                    currentBurstAmount = 0;
                    nextBurstAttackTime = Time.time;
                }
            }
        }

        private void HandleBurstAttack()
        {
            if (!doingBurst)
                return;

            if (!underAttack)
                return;

            if (!TargetValid())
                target = GetClosestTargetInRange();

            if (target != null)
            {
                if (Time.time >= nextBurstAttackTime)
                {
                    Attack();

                    currentBurstAmount++;

                    if (currentBurstAmount >= amountOfAttacksInBurst)
                    {
                        ResetAttack();
                        return;
                    }

                    if (burstAttackSpeed == 0f)
                        nextBurstAttackTime = float.MaxValue;
                    else
                        nextBurstAttackTime = Time.time + 1f / burstAttackSpeed;
                }
            }
            else
            {
                ResetAttack();
            }
        }

        private bool TargetValid()
        {
            return (target != null && Vector2.Distance(target.position, transform.position) <= rangeToFindTarget);
        }

        private void Attack()
        {
            if (target == null)
            {
                ResetAttack();
                return;
            }

            Projectile projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity, transform);

            projectile.onHit += OnDamageUnit;

            ProjectileStats projectileStats = new ProjectileStats(damage, 1, 0, 0, knockbackMultiplier, projectileSpeed, projectileTurnSpeed);
            ProjectileInit projectileInit = new ProjectileInit(attackHitMask, transform.position, Quaternion.identity, false, false, target, target.position - transform.position, null, HealthAndShieldManager);
            projectile.Init(projectileInit, projectileStats);
        }

        private void ResetAttack()
        {
            doingBurst = false;
            currentBurstAmount = 0;

            if (attackSpeed == 0f)
                nextAttackTime = float.MaxValue;
            else
                nextAttackTime = Time.time + 1f / attackSpeed;
        }

        private void HandleFindBoss()
        {
            if (boss != null && boss.gameObject != null)
                return;

            boss = MonsterSpawnManager.Instance.BossMonster;
        }

        private void HandleShieldBoss()
        {
            if (boss == null || boss.gameObject == null || underAttack)
                return;

            amountToAddThisTick += shieldRegenerationRate * Time.deltaTime;

            if (amountToAddThisTick < 1f) return;
            int amountToAdd = Mathf.FloorToInt(amountToAddThisTick);
            boss.HealthAndShieldManager.Shield.RegenerateShield(amountToAdd);
            amountToAddThisTick -= amountToAdd;
        }

        private void HandleLineRendering()
        {
            if (boss == null || boss.gameObject == null || underAttack)
            {
                if(lineRenderer.positionCount > 0)
                    lineRenderer.positionCount = 0;

                return;
            }

            Matrix4x4 localMatrix = transform.worldToLocalMatrix;



            lineRenderer.SetPositions(new Vector3[2] { localMatrix.MultiplyPoint(transform.position), localMatrix.MultiplyPoint(boss.transform.position)});
        }

        protected override void ShockEffect(float shockTime)
        {
            
        }
    }
}

