using UnityEngine;
using System.Collections.Generic;
using Units;
using Monsters;

public class MeleeAttackSpawn : MonoBehaviour
{
    [SerializeField]
    private LayerMask hitLayerMask;

    private readonly List<GameObject> hitGameObjects = new List<GameObject>();

    private int damage;
    private float knockback;

    private bool destroyProjectiles;

    public void Init(int damage, bool destroyProjectiles, float knockbackMultiplier = 0f)
    {
        this.damage = damage;
        this.destroyProjectiles = destroyProjectiles;
        knockback = knockbackMultiplier;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hitLayerMask.ContainsLayer(collision.gameObject.layer))
            return;

        if (destroyProjectiles && collision.gameObject.TryGetComponent(out Projectile projectile))
        {
            Destroy(projectile.gameObject);
            return;
        }

        if (!hitGameObjects.Contains(collision.gameObject))
        {
            if(collision.gameObject.TryGetComponentInParent(out HealthAndShieldManager healthAndShieldManager))
            {
                Vector2 hitDirection = collision.transform.position - transform.position;
                healthAndShieldManager.Damage(damage, hitDirection, knockback);
            }

            hitGameObjects.Add(collision.gameObject);
        }
    }

    private void DestroyAttack()
    {
        Destroy(gameObject);
    }
}
