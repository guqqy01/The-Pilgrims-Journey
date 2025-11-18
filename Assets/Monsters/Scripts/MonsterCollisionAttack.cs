using UnityEngine;
using System.Collections.Generic;

public class MonsterCollisionAttack : MonoBehaviour
{
    public int damage = 3;
    public float knockback = 1f;
    public float attackCooldown = 0.5f;

    [SerializeField]
    private LayerMask layerMask;

    private Dictionary<HealthAndShieldManager, float> hitCooldowns
        = new Dictionary<HealthAndShieldManager, float>();


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!layerMask.ContainsLayer(collision.gameObject.layer))
            return;

        if (!collision.TryGetComponent(out HealthAndShieldManager health))
            return;

        float nextAllowed = hitCooldowns.ContainsKey(health) ? hitCooldowns[health] : 0f;

        if (Time.time >= nextAllowed)
        {
            Vector3 dir = (collision.transform.position - transform.position).normalized;
            health.Damage(damage, dir, knockback);

            hitCooldowns[health] = Time.time + attackCooldown;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out HealthAndShieldManager health))
        {
            hitCooldowns.Remove(health);
        }
    }
}
