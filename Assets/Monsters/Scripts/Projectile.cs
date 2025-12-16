using System;
using Units;
using UnityEngine;
using System.Collections.Generic;

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

    private readonly List<HealthAndShieldManager> hitHealth = new List<HealthAndShieldManager>();
     
    private const float projectileLifetime = 20f;
    private float lifeTimeEnd;

    public void Init(ProjectileStats projectileHitStats,bool homing, Vector2 direction, LayerMask layerMask, Transform target = null)
    {
        projectileStats = projectileHitStats;
        this.homing = homing;
        this.target = target;
        this.direction = direction;
        this.layerMask = layerMask;
        transform.right = direction;
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
        if (!hit && layerMask.ContainsLayer(collision.gameObject.layer))
        {
            if (collision.TryGetComponent(out HealthAndShieldManager healthAndShieldManager) && !hitHealth.Contains(healthAndShieldManager))
            {
                OnHitInfo info = healthAndShieldManager.Damage(projectileStats.attackStats, (healthAndShieldManager.transform.position - transform.position).normalized);

                onHit?.Invoke(info);

                if (oneHit)
                {
                    hit = true;
                    Destroy(gameObject);
                }
            }
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


