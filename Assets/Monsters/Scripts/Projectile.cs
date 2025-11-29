using System;
using Units;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    private int damage;
    private float speed;

    private bool homing;

    private Transform target;

    private Vector2 direction;

    private LayerMask layerMask;

    [SerializeField]
    private bool oneHit = true;

    private bool hit = false;

    [SerializeField]
    private bool pointInTravelDirection;

    public delegate void OnHit(OnHitInfo hitInfo);
    public OnHit onHit;

    private readonly List<HealthAndShieldManager> hitHealth = new List<HealthAndShieldManager>();

    private const float projectileLifetime = 20f;
    private float lifeTimeEnd;

    private float knockbackMultiplier;

    public void Init(int damage, float speed, float knockbackMultiplier,bool homing, Transform target, Vector2 direction, LayerMask layerMask)
    {
        this.damage = damage;
        this.speed = speed;
        this.homing = homing;
        this.target = target;
        this.direction = direction;
        this.layerMask = layerMask;
        this.knockbackMultiplier = knockbackMultiplier;

        if (pointInTravelDirection)
        {
            direction = target.position - transform.position;

            transform.right = direction;
        }
    }

    private void Awake()
    {
        lifeTimeEnd = Time.time + projectileLifetime;
    }

    private void Update()
    {
        if (homing && target != null)
        {
            direction = target.position - transform.position;
                
            transform.right = direction;
        }

        transform.position += (Vector3)direction.normalized * speed * Time.deltaTime;

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
                
                OnHitInfo info = healthAndShieldManager.Damage(damage, (healthAndShieldManager.transform.position - transform.position).normalized, knockbackMultiplier);

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


