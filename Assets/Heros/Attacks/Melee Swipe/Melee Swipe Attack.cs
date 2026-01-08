using UnityEngine;
using System.Collections.Generic;
using Units;
using Monsters;

public class MeleeAttackSpawn : MonoBehaviour
{
    [SerializeField]
    private LayerMask hitLayerMask;

    private readonly List<GameObject> hitGameObjects = new List<GameObject>();

    int damage;
    float knockback;

    public void Init(int damage, float knockbackMultiplier = 0f)
    {
        this.damage = damage;
        knockback = knockbackMultiplier;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hitLayerMask.ContainsLayer(collision.gameObject.layer))
        {
            if (collision.gameObject.TryGetComponent(out Projectile projectile))
            {
                Destroy(projectile.gameObject);
                return;
            }
        }

        if (!hitGameObjects.Contains(collision.gameObject) && hitLayerMask.ContainsLayer(collision.gameObject.layer))
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
