using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Heros.Attacks.Orbital
{
    public class OrbitalHeroAttackSpawn : MonoBehaviour
    {
        [SerializeField]
        private new Collider2D collider2D;

        [SerializeField]
        private LayerMask hitLayerMask;

        private Transform targetPositionTransform;

        private readonly Dictionary<HealthAndShieldManager, float> timeToNextDmg = new Dictionary<HealthAndShieldManager, float>();

        private readonly List<HealthAndShieldManager> toRemove = new List<HealthAndShieldManager>();

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

        public void Init(Transform targetPositionTransform, int damage, float attackSpeed, float orbitSpeed, bool destroyProjectiles, float knockbackMultiplier = 0f, int? amountOfHits = null, bool projectilesCountAsHits = false)
        {
            this.targetPositionTransform = targetPositionTransform;
            SetupStats(damage, attackSpeed, orbitSpeed, destroyProjectiles, knockbackMultiplier, amountOfHits, projectilesCountAsHits);
        }

        public void SetupStats(int damage, float attackSpeed, float orbitSpeed, bool destroyProjectiles, float knockbackMultiplier = 0f, int? amountOfHits = null, bool projectilesCountAsHits = false)
        {
            this.damage = damage;
            this.attackSpeed = attackSpeed;
            //Multiplied by 1.25 so it can catch up to the target if it needs to, otherwise the orbit speed will be controlled by the OrbitalHeroAttack through the targetTransform
            this.orbitSpeed = orbitSpeed * 1.25f;
            this.destroyProjectiles = destroyProjectiles;
            knockback = knockbackMultiplier;
            this.amountOfHits = amountOfHits;
            this.projectilesCountAsHits = projectilesCountAsHits;
        }

        private void FixedUpdate()
        {
            if (targetPositionTransform == null)
            {
                DestroyAttack();
                return;
            }

            Vector3 toVector = targetPositionTransform.position - transform.position;
            Vector3 normalizedDirection = toVector.normalized;

            float distanceMovedThisFrame = orbitSpeed * Time.fixedDeltaTime;

            if (toVector.sqrMagnitude <= distanceMovedThisFrame * distanceMovedThisFrame)
            {
                transform.position = targetPositionTransform.position;
            }
            else
            {
                transform.position += normalizedDirection * distanceMovedThisFrame;
            }
        }

        private void Update()
        {
            toRemove.Clear();

            foreach (var kvp in timeToNextDmg)
            {
                HealthAndShieldManager hit = kvp.Key;
                float time = kvp.Value;

                if(hit == null)
                    toRemove.Add(kvp.Key);
            }

            foreach(var hit in toRemove)
            {
                timeToNextDmg.Remove(hit);
            }
        }

        private void OnEnemyDie(Health health)
        {
            if (health == null)
                return;

            if(health.TryGetComponent(out HealthAndShieldManager healthAndShieldManager))
            {
                timeToNextDmg.Remove(healthAndShieldManager);
            }
            
            health.onDie -= OnEnemyDie;
        }

        private void HandleColliderHits(Collider2D collision)
        {
            if (!hitLayerMask.ContainsLayer(collision.gameObject.layer))
                return;

            if (!collision.gameObject.TryGetComponentInParent(out HealthAndShieldManager healthAndShieldManager))
                return;

            bool inDict = timeToNextDmg.ContainsKey(healthAndShieldManager);

            if (timeToNextDmg.TryGetValue(healthAndShieldManager, out float nextTime) && nextTime > Time.time)
                return;

            Vector2 hitDirection = healthAndShieldManager.transform.position - transform.position;
            healthAndShieldManager.Damage(damage, hitDirection, knockback);

            float timeToNextAttack = attackSpeed != 0 ? Time.time + 1f / attackSpeed : float.MaxValue;

            if (inDict)
            {
                timeToNextDmg[healthAndShieldManager] = timeToNextAttack;
            }
            else
            {
                timeToNextDmg.Add(healthAndShieldManager, timeToNextAttack);
                healthAndShieldManager.Health.onDie += OnEnemyDie;
            }

            HandleAmountOfHits();
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            HandleColliderHits(collision);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (HandleProjectiles(collision))
            {
                HandleAmountOfHits();
            }
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

        private bool HandleProjectiles(Collider2D collision)
        {
            if (!hitLayerMask.ContainsLayer(collision.gameObject.layer))
                return false;

            if (!destroyProjectiles)
                return false;

            if (!collision.gameObject.TryGetComponent(out Projectile projectile))
                return false;

            Destroy(projectile.gameObject);

            return projectilesCountAsHits;
        }

        public void DestroyAttack()
        {
            onDestroyed?.Invoke(this, targetPositionTransform);

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            foreach (var kvp in timeToNextDmg)
            {
                if (kvp.Key != null)
                    kvp.Key.Health.onDie -= OnEnemyDie;
            }

            timeToNextDmg.Clear();
            onDestroyed = null;
        }
    }
}

