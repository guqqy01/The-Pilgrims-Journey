using UnityEngine;
using System.Collections.Generic;

public class MonsterCollisionAttack : MonoBehaviour
{
    public int damage = 3;
    public float knockback = 1f;
    public float attackCooldown = 0.5f;

    [SerializeField]
    private LayerMask layerMask;

    private readonly Dictionary<HealthAndShieldManager, float> hitCooldowns
        = new Dictionary<HealthAndShieldManager, float>();


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!layerMask.ContainsLayer(collision.gameObject.layer))
            return;

        if (!collision.TryGetComponentInParent(out HealthAndShieldManager health))
            return;


        if (!hitCooldowns.ContainsKey(health))
        {
            hitCooldowns.Add(health, 0f);
        }

        float nextAllowed = hitCooldowns[health];

        if (Time.time >= nextAllowed)
        {
            Vector3 dir = (collision.transform.position - transform.position).normalized;
            health.Damage(damage, false, dir, knockback);

            hitCooldowns[health] = Time.time + attackCooldown;
        }
    }
}
