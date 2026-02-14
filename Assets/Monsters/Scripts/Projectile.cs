using System;
using Units;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    private ProjectileStats projectileStats;

    private bool homing;

    private bool bounce;

    private Transform target;

    private Vector2 direction;

    private LayerMask layerMask;

    [SerializeField]
    private int amountOfHits = 0;

    private int currentAmountOfHits = 0;

    [SerializeField]
    private bool canHitSameTargetMultipleTimes = false;

    private bool deactivated = false;

    public delegate void OnHit(OnHitInfo hitInfo);
    public OnHit onHit;

    private readonly HashSet<HealthAndShieldManager> hitHealth = new HashSet<HealthAndShieldManager>();
     
    //private readonly Dictionary<HealthAndShieldManager, float> hitHealthAndRefreshTime = 

    private const float projectileLifetime = 20f;
    private float lifeTimeEnd;

    private HashSet<HealthAndShieldManager> ignoreExceptions;

    private void OnDestroy()
    {
        onHit = null;
    }

    public void Init(ProjectileStats projectileHitStats, bool homing, Vector2 direction, LayerMask layerMask, bool bounce = false, Transform target = null, params HealthAndShieldManager[] ignoreExceptions)
    {
        projectileStats = projectileHitStats;
        this.homing = homing;
        this.target = target;
        this.bounce = bounce;

        this.direction = direction;
        this.layerMask = layerMask;
        transform.right = direction;

        this.ignoreExceptions = ignoreExceptions.ToHashSet();
    }

    private void Awake()
    {
        lifeTimeEnd = Time.time + projectileLifetime;
    }

    private void Update()
    {
        if (homing && target != null)
        {
            direction = (target.position - transform.position).normalized;

            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.right, direction);

            transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            projectileStats.turningSpeed * Time.deltaTime
            );
        }

        transform.position += projectileStats.speed * Time.deltaTime * transform.right;

        if(Time.time > lifeTimeEnd)
        {
            Destroy(gameObject);
        }
    }

    protected virtual bool AttackActive(Collider2D collision)
    {
        return true;
    }

    private bool CollisionValid(Collider2D collision, out HealthAndShieldManager healthAndShieldManager)
    {
        healthAndShieldManager = null;

        if (deactivated)
            return false;

        if (!layerMask.ContainsLayer(collision.gameObject.layer))
            return false;

        if (!collision.TryGetComponentInParent(out healthAndShieldManager))
            return false;

        if (ignoreExceptions.Contains(healthAndShieldManager))
            return false;

        if (!hitHealth.Add(healthAndShieldManager))
            return false;

        if (!AttackActive(collision))
            return false;

        return true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!CollisionValid(collision, out HealthAndShieldManager healthAndShieldManager))
            return;

        Vector2 damageDirection = (healthAndShieldManager.transform.position - transform.position).normalized;

        OnHitInfo info = healthAndShieldManager.Damage(projectileStats.attackStats, damageDirection);

        HandleBounce(collision);

        onHit?.Invoke(info);

        currentAmountOfHits++;

        if (amountOfHits > 0 && currentAmountOfHits >= amountOfHits)
        {
            deactivated = true;
            Destroy(gameObject);
        }
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
}

public struct ProjectileStats
{
    public AttackStats attackStats;

    public float speed;
    public float turningSpeed;

    public ProjectileStats(AttackStats attackStats, float speed, float turningSpeed)
    {
        this.attackStats = attackStats;

        this.speed = speed;
        this.turningSpeed = turningSpeed;
    }

    public ProjectileStats(int damage, float criticalChance, int criticalDamageMultiplier, float knockbackMultiplier, float speed, float turningSpeed)
    {
        attackStats = new AttackStats(damage, criticalChance, criticalDamageMultiplier, knockbackMultiplier);

        this.speed = speed;
        this.turningSpeed = turningSpeed;
    }
}


public struct AttackStats
{
    public int damage;

    public float criticalChance;
    public int criticalDamageMultiplier;

    public float knockbackMultiplier;

    public AttackStats(int damage, float criticalChance, int criticalDamageMultiplier, float knockbackMultiplier)
    {
        this.damage = damage;
        this.criticalChance = criticalChance;
        this.criticalDamageMultiplier = criticalDamageMultiplier;
        this.knockbackMultiplier = knockbackMultiplier;
    }
}


