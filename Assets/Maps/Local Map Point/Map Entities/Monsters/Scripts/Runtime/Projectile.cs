using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Attacks;

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

    private LayerMask layerMask;

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
    public void Init(ProjectileStats projectileHitStats, bool homing, Vector2 direction, LayerMask layerMask, bool bounce, Transform target, ProjectileOwner owner, params HealthAndShieldManager[] ignoreExceptions)
    {
        this.projectileStats = projectileHitStats;

        var initContext = new ProjectileInit
        {
            homing = homing,
            bounce = bounce,
            target = target,
            direction = direction,
            layerMask = layerMask,
            owner = owner,
            ignoreExceptions = ignoreExceptions
        };

        Init(initContext, projectileHitStats);
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
        homing = initContext.homing;
        bounce = initContext.bounce;
        target = initContext.target;
        direction = initContext.direction;
        layerMask = initContext.layerMask;
        owner = initContext.owner;
        ignoreExceptions = (initContext.ignoreExceptions ?? Array.Empty<HealthAndShieldManager>()).ToHashSet();

        // Ensure base hit layer mask used by AttackSpawn.TryBeginHit matches the projectile's configured layer mask
        hitLayerMask = initContext.layerMask;

        transform.right = direction;
    }

    protected override void InheritSetupStats(ProjectileStats statsContext)
    {
        // Use projectile-specific stats if provided; if projectileStats already set, prefer it
        if ((projectileStats == null || projectileStats.Damage == 0) && statsContext != null)
        {
            // Construct gameplay ProjectileStats using available fields. Map missing critical fields to sensible defaults.
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
        if (!TryBeginHit(collision, out HealthAndShieldManager healthAndShieldManager, out bool existed))
            return;

        // Then run projectile-specific checks and hit-history. We only add to hitHealth after shared checks succeed,
        // otherwise we risk leaving entries in hitHealth that permanently prevent hits.
        if (!ProjectilePreChecks(healthAndShieldManager, collision))
            return;

        if (!layerMask.ContainsLayer(collision.gameObject.layer))
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

        currentAmountOfHits++;

        if (amountOfHits.HasValue && currentAmountOfHits >= amountOfHits.Value)
        {
            deactivated = true;
            DestroyAttack();
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // Let base handle projectile destruction on enter (e.g., collision with other projectiles)
        base.OnTriggerEnter2D(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!canHitSameTargetMultipleTimes)
            return;

        if (deactivated)
            return;

        if (!layerMask.ContainsLayer(collision.gameObject.layer))
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


