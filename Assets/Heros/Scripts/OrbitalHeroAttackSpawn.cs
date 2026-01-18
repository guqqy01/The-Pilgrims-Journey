using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static Heros.OrbitalHeroAttackSpawn;

namespace Heros
{
    public class OrbitalHeroAttackSpawn : MonoBehaviour
    {
        [SerializeField]
        private LayerMask hitLayerMask;

        private readonly List<GameObject> hitGameObjects = new List<GameObject>();

        private Transform targetPositionTransform;

        private int damage;
        private float knockback;
        private float orbitSpeed;

        private float attackSpeed;

        private bool destroyProjectiles;

        private int currentAmountOfHits;

        private int? amountOfHits;

        private bool projectilesCountAsHits;

        public delegate void OnDestroyed(OrbitalHeroAttackSpawn orbitalDestroyed, Transform target);
        public OnDestroyed onDestroyed;

        public void Init(Transform targetPositionTransform, int damage, float orbitSpeed, bool destroyProjectiles, float knockbackMultiplier = 0f, int? amountOfHits = null, bool projectilesCountAsHits = false)
        {
            this.targetPositionTransform = targetPositionTransform;
            SetupStats(damage, orbitSpeed, destroyProjectiles, knockbackMultiplier, amountOfHits, projectilesCountAsHits);
        }

        public void SetupStats(int damage, float orbitSpeed, bool destroyProjectiles, float knockbackMultiplier = 0f, int? amountOfHits = null, bool projectilesCountAsHits = false)
        {
            this.damage = damage;
            //Multiplied by 1.25 so it can catch up to the target if it needs to, otherwise the orbit speed will be controlled by the OrbitalHeroAttack through the targetTransform
            this.orbitSpeed = orbitSpeed * 1.25f;
            this.destroyProjectiles = destroyProjectiles;
            knockback = knockbackMultiplier;
            this.amountOfHits = amountOfHits;
            this.projectilesCountAsHits = projectilesCountAsHits;
        }

        private void FixedUpdate()
        {
            Vector3 direction = (targetPositionTransform.position - transform.position).normalized;

            float distanceMovedThisFrame = orbitSpeed * Time.fixedDeltaTime;

            if(Vector3.Distance(targetPositionTransform.position, transform.position) <= distanceMovedThisFrame)
            {
                transform.position = targetPositionTransform.position;
            }
            else
            {
                transform.position = transform.position + direction * distanceMovedThisFrame;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!hitLayerMask.ContainsLayer(collision.gameObject.layer))
                return;

            HandleProjectiles(collision);

            HandleHits(collision);
        }

        private void HandleAmountOfHits()
        {
            if (amountOfHits == null)
                return;

            currentAmountOfHits++;

            if (currentAmountOfHits >= amountOfHits)
            {
                DestroyAttack();
            }
        }

        private void HandleHits(Collider2D collision)
        {
            if (!collision.gameObject.TryGetComponentInParent(out HealthAndShieldManager healthAndShieldManager))
                return;
            
            Vector2 hitDirection = collision.transform.position - transform.position;
            healthAndShieldManager.Damage(damage, hitDirection, knockback);
            
            hitGameObjects.Add(collision.gameObject);

            HandleAmountOfHits();
        }

        private void HandleProjectiles(Collider2D collision)
        {
            if (!destroyProjectiles)
                return;

            if (!collision.gameObject.TryGetComponent(out Projectile projectile))
                return;

            Destroy(projectile.gameObject);

            if (!projectilesCountAsHits)
                return;

            HandleAmountOfHits();
        }

        public void DestroyAttack()
        {
            onDestroyed?.Invoke(this, targetPositionTransform);

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            onDestroyed = null;
        }
    }
}

