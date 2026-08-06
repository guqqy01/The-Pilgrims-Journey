using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heros.Attacks
{
    public abstract class AttackSpawn<TInitContext, TStatsContext> : AttackSpawnBase
        where TInitContext : AttackInitContext
        where TStatsContext : AttackStatsContext
    {
        [SerializeField]
        protected Collider2D hitCollider;

        [SerializeField]
        protected LayerMask hitLayerMask;

        protected int damage;
        protected float knockback;
        protected float hitSpeed;
        protected bool destroyProjectiles;
        protected int currentAmountOfHits;
        protected int? amountOfHits;
        protected bool projectilesCountAsHits;

        protected abstract void FixedUpdateMovement();

        protected abstract void InheritInit(TInitContext initContext);
        protected abstract void InheritSetupStats(TStatsContext statsContext);

        protected virtual void FixedUpdate()
        {
            FixedUpdateMovement();
        }

        public void Init(TInitContext initContext, TStatsContext statsContext)
        {
            InheritInit(initContext);
            SetupStats(statsContext);

            RaiseOnSpawn();
        }

        public void SetupStats(TStatsContext context)
        {
            damage = context.attackStats.Damage;
            hitSpeed = context.attackStats.HitRefreshSpeed;
            destroyProjectiles = context.attackStats.DestroyProjectiles;
            knockback = context.attackStats.Knockback;
            amountOfHits = context.attackStats.AmountOfHits;

            projectilesCountAsHits = context.attackStats.ProjectilesCountAsHits;

            InheritSetupStats(context);
        }

        /// <summary>
        /// Shared pre-checks used by different attack types. Returns true when it's valid to apply a hit and outputs whether a cooldown entry already existed.
        /// </summary>
        protected bool TryBeginHit(Collider2D collision, out HealthAndShieldManager healthAndShieldManager, out bool existed)
        {
            existed = false;
            healthAndShieldManager = null;

            if (!hitLayerMask.ContainsLayer(collision.gameObject.layer))
                return false;

            if (!collision.gameObject.TryGetComponentInParent(out healthAndShieldManager))
                return false;

            existed = timeToNextDmg.TryGetValue(healthAndShieldManager, out float nextTime);

            if (existed && nextTime > Time.time)
                return false;

            if (amountOfHits.HasValue && currentAmountOfHits >= amountOfHits.Value)
                return false;

            return true;
        }

        /// <summary>
        /// Update or add the hit cooldown entry for a target and subscribe to its death.
        /// </summary>
        protected void UpdateHitCooldown(HealthAndShieldManager healthAndShieldManager)
        {
            float timeToNextAttack = hitSpeed != 0 ? Time.time + 1f / hitSpeed : float.MaxValue;

            if (timeToNextDmg.ContainsKey(healthAndShieldManager))
            {
                timeToNextDmg[healthAndShieldManager] = timeToNextAttack;
            }
            else
            {
                timeToNextDmg.Add(healthAndShieldManager, timeToNextAttack);
                healthAndShieldManager.Health.onDie += OnEnemyDie;
            }
        }

        protected virtual void HandleColliderHits(Collider2D collision)
        {
            if (!TryBeginHit(collision, out HealthAndShieldManager healthAndShieldManager, out bool existed))
                return;

            Vector2 hitDirection = ((Vector2)(healthAndShieldManager.transform.position - transform.position)).normalized;
            OnHitInfo onHitInfo = healthAndShieldManager.Damage(damage, hitDirection, knockback);

            RaiseOnHit(onHitInfo);

            UpdateHitCooldown(healthAndShieldManager);

            HandleAmountOfHits();
        }

        protected virtual void OnTriggerStay2D(Collider2D collision)
        {
            HandleColliderHits(collision);
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (HandleProjectiles(collision))
            {
                HandleAmountOfHits();
            }
        }

        protected void HandleAmountOfHits()
        {
            if (amountOfHits == null)
                return;

            currentAmountOfHits++;

            if (currentAmountOfHits >= amountOfHits)
            {
                DestroyAttack();
            }
        }

        /// <returns>If hit a projectile and projectiles count towards hit counter</returns>
        protected bool HandleProjectiles(Collider2D collision)
        {
            if (!hitLayerMask.ContainsLayer(collision.gameObject.layer))
                return false;

            if (!destroyProjectiles)
                return false;

            if (!collision.gameObject.TryGetComponent(out global::Projectile projectile))
                return false;

            // Respect pooling: call ReleaseProjectile so pooled projectiles are returned to their pool
            projectile.DestroyAttack();

            return projectilesCountAsHits;
        }
    }
}
