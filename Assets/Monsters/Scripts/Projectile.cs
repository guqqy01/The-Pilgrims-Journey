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

    private Transform target;

    private Vector2 direction;

    private LayerMask layerMask;

    [SerializeField]
    private bool oneHit = true;

    private bool hit = false;

    public delegate void OnHit(OnHitInfo hitInfo);
    public OnHit onHit;

    private readonly HashSet<HealthAndShieldManager> hitHealth = new HashSet<HealthAndShieldManager>();
     
    private const float projectileLifetime = 20f;
    private float lifeTimeEnd;

    private HashSet<HealthAndShieldManager> ignoreExceptions;

    public void Init(ProjectileStats projectileHitStats,bool homing, Vector2 direction, LayerMask layerMask, Transform target = null, params HealthAndShieldManager[] ignoreExceptions)
    {
        projectileStats = projectileHitStats;
        this.homing = homing;
        this.target = target;
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

        transform.position += transform.right * projectileStats.speed * Time.deltaTime;

        if(Time.time > lifeTimeEnd)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hit)
            return;

        if (!layerMask.ContainsLayer(collision.gameObject.layer))
            return;

        if (!collision.TryGetComponentInParent(out HealthAndShieldManager healthAndShieldManager))
            return;

        if (ignoreExceptions.Contains(healthAndShieldManager))
            return;

        if (!hitHealth.Add(healthAndShieldManager))
            return;

        Vector2 damageDirection = (healthAndShieldManager.transform.position - transform.position).normalized;

        OnHitInfo info = healthAndShieldManager.Damage(projectileStats.attackStats, damageDirection);

        onHit?.Invoke(info);

        if (oneHit)
        {
            hit = true;
            Destroy(gameObject);
        }
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


