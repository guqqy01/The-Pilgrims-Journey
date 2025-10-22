using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    public class PriestSpecial : MonoBehaviour
    {
        [SerializeField]
        private float range;

        [SerializeField]
        private float healCooldown;

        private float timeToNextHeal;

        [SerializeField]
        private LayerMask unitLayerMask;

        private Health targetHealth;
        private Health ourHealth;

        private void Awake()
        {
            ourHealth = GetComponent<Health>();
        }

        private void Update()
        {
            if (timeToNextHeal > Time.time)
                return;

            if (targetHealth == null || targetHealth.CurrentHealth == targetHealth.MaxHealth)
                targetHealth = FindTarget();

            if(targetHealth != null)
            {
                targetHealth.ModifyHealth(1);

                timeToNextHeal = Time.time + healCooldown;
            }
        }

        private Health FindTarget()
        {
            Collider2D[] unitColliders = Physics2D.OverlapCircleAll(transform.position, range, unitLayerMask);

            if (unitColliders.Length <= 1)
                return null;

            List<Health> healths = new List<Health>();

            foreach(Collider2D collider in unitColliders)
            {
                if(collider.TryGetComponent(out Health health))
                {
                    if (health == ourHealth)
                        continue;

                    if (health.CurrentHealth >= health.MaxHealth)
                        continue;

                    healths.Add(health);
                }
            }

            if(healths.Count == 0)
                return null;

            return healths.GetRandom();
        }
    }
}

