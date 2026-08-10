using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
namespace Attacks.Projectiles
{
    public enum ProjectileOwner
    {
        Monster,
        Hero,
        Neutral
    }

    [RequireComponent(typeof(Collider2D))]
    public class Projectile : AttackSpawn<ProjectileInit, ProjectileStats>
    {
        private ProjectileStats projectileStats;

        private bool homing;
        private bool bounce;

        private Transform target;

        private Vector2 direction;

        [SerializeField]
        private bool canHitSameTargetMultipleTimes = false;

        private bool deactivated = false;

        private readonly HashSet<HealthAndShieldManager> hitHealth = new HashSet<HealthAndShieldManager>();

        private const float projectileLifetime = 20f;
        private float lifeTimeEnd;

        private HashSet<HealthAndShieldManager> ignoreExceptions;

        // Pool callback handled by base via SetReturnToPoolCallback

        private ProjectileOwner owner = ProjectileOwner.Monster;

        public ProjectileOwner Owner => owner;

        // Public subscription point for external listeners (keeps compatibility)
        public Action<OnHitInfo> onHit;

        protected override void OnDestroy()
        {
            onHit = null;
            base.OnDestroy();
        }

        // Backwards-compatible Init with owner
        public void Init(ProjectileStats projectileStats, bool homing, Vector2 direction, LayerMask layerMask, bool bounce, Transform target, ProjectileOwner owner, params HealthAndShieldManager[] ignoreExceptions)
        {
            this.projectileStats = projectileStats;

            ProjectileInit projectileInit = new ProjectileInit(layerMask, homing, bounce, target, direction, owner, ignoreExceptions);

            Init(projectileInit, projectileStats);
        }

        // Provide InitWithOwner for existing callers
        public void InitWithOwner(ProjectileStats projectileHitStats, bool homing, Vector2 direction, LayerMask layerMask, bool bounce, Transform target, ProjectileOwner owner)
        {
            Init(projectileHitStats, homing, direction, layerMask, bounce, target, owner);
        }

        protected override void FixedUpdateMovement()
        {
            // movement is handled in FixedUpdateMovement to match AttackSpawn update
            // actual movement logic is applied in FixedUpdateMovement
            if (homing && target != null)
            {
                direction = (target.position - transform.position).normalized;

                Quaternion targetRotation = Quaternion.FromToRotation(Vector3.right, direction);

                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    projectileStats.TurningSpeed * Time.deltaTime
                );
            }

            transform.position += projectileStats.Speed * Time.deltaTime * transform.right;

            if (Time.time > lifeTimeEnd)
            {
                DestroyAttack();
            }
        }

        protected override void InheritInit(ProjectileInit initContext)
        {
            homing = initContext.Homing;
            bounce = initContext.Bounce;
            target = initContext.Target;
            direction = initContext.Direction;
            owner = initContext.Owner;
            ignoreExceptions = (initContext.IgnoreExceptions ?? Array.Empty<HealthAndShieldManager>()).ToHashSet();

            

            transform.right = direction;
        }

        protected override void InheritSetupStats(ProjectileStats statsContext)
        {
            if ((projectileStats == null || !projectileStats.HasExplicitStats) && statsContext != null)
            {
                projectileStats = new ProjectileStats(
                    statsContext.Damage,
                    statsContext.CriticalChance,
                    statsContext.CriticalDamageMultiplier,
                    statsContext.Knockback,
                    statsContext.Speed,
                    statsContext.TurningSpeed);
            }

            lifeTimeEnd = Time.time + projectileLifetime;
            deactivated = false;
            hitHealth.Clear();
            currentAmountOfHits = 0;
        }

        protected virtual bool AttackActive(Collider2D collision)
        {
            return true;
        }

        // Per-projectile validation that does NOT mutate hitHistory. Returns the HealthAndShieldManager when valid.
        private bool ProjectilePreChecks(HealthAndShieldManager health, Collider2D collision)
        {
            if (deactivated)
                return false;

            if (ignoreExceptions != null && health != null && ignoreExceptions.Contains(health))
                return false;

            if (!AttackActive(collision))
                return false;

            return true;
        }

        protected override void OnTriggerStay2D(Collider2D collision)
        {
            // First run shared pre-checks from AttackSpawn (cooldowns, layer mask, component lookup)
            if (!TryBeginHit(collision, out HealthAndShieldManager healthAndShieldManager))
                return;

            // Then run projectile-specific checks and hit-history. We only add to hitHealth after shared checks succeed,
            // otherwise we risk leaving entries in hitHealth that permanently prevent hits.
            if (!ProjectilePreChecks(healthAndShieldManager, collision))
                return;

            if (!hitLayerMask.ContainsLayer(collision.gameObject.layer))
                return;

            if (!hitHealth.Add(healthAndShieldManager))
                return;

            Vector2 damageDirection = (healthAndShieldManager.transform.position - transform.position).normalized;

            // Use the projectile's attack stats (supports criticals, etc.)
            OnHitInfo info = healthAndShieldManager.Damage(projectileStats, damageDirection);

            HandleBounce(collision);

            // invoke external listeners
            onHit?.Invoke(info);

            // raise base event handlers (audio, etc.)
            RaiseOnHit(info);

            // update cooldown mapping
            UpdateHitCooldown(healthAndShieldManager);

            HandleAmountOfHits();
        }

        public override void DestroyAttack()
        {
            deactivated = true;
            base.DestroyAttack();
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!canHitSameTargetMultipleTimes)
                return;

            if (deactivated)
                return;

            if (!hitLayerMask.ContainsLayer(collision.gameObject.layer))
                return;

            if (!collision.TryGetComponentInParent(out HealthAndShieldManager healthAndShieldManager))
                return;

            hitHealth.Remove(healthAndShieldManager);
        }

        private void HandleBounce(Collider2D collider2D)
        {
            if (!bounce)
                return;

            Vector2 currentDirection = transform.right;

            Vector2 directionToHitTarget = (collider2D.transform.position - transform.position).normalized;

            Vector2 reflectionVector = Vector2.Reflect(currentDirection, -directionToHitTarget);

            transform.right = reflectionVector;
        }

        // Called by pool when this instance is retrieved from the pool
        public override void OnPoolGet()
        {
            deactivated = false;
            transform.localScale = Vector3.one;
            hitHealth.Clear();
            currentAmountOfHits = 0;
            lifeTimeEnd = Time.time + projectileLifetime;
        }

        // Called by pool when this instance is released back into the pool
        public override void OnPoolRelease()
        {
            base.OnPoolRelease();
            deactivated = false;
            onHit = null;
            transform.localScale = Vector3.one;
            hitHealth.Clear();
            currentAmountOfHits = 0;
            ignoreExceptions = null;
        }
    }



}
