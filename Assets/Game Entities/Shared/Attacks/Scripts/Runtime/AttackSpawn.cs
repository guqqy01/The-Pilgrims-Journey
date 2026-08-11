using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
namespace Attacks
{
    public abstract class AttackSpawn<TInit, TStats> : AttackSpawnBase
        where TInit : AttackInit
        where TStats : AttackStats
    {
        [SerializeField]
        protected Collider2D hitCollider;

        protected LayerMask hitLayerMask;

        protected TStats attackStats;
        protected int currentAmountOfHits;
        protected abstract void FixedUpdateMovement();
        protected abstract void InheritInit(TInit initContext);
        protected abstract void InheritSetupStats(TStats statsContext);

        protected virtual void FixedUpdate()
        {
            FixedUpdateMovement();
        }

        public override void Init<TInitContext, TStatsContext>(TInitContext initContext, TStatsContext statsContext)
        {
            hitLayerMask = initContext.HitMask;

            if(initContext is TInit init)
            {
                InheritInit(init);
            }
            else
            {
                Debug.LogWarning($"Init and context are not matching, TInit: {typeof(TInit)}, TInitContext: {typeof(TInitContext)}");
            }

            if(statsContext is TStats stats)
            {
                SetupStats(stats);
            }
            else
            {
                Debug.LogWarning($"Stats and context are not matching, TStats: {typeof(TStats)}, TStatsContext: {typeof(TStatsContext)}");
            }

            RaiseOnSpawn();
        }

        public void SetupStats(TStats context)
        {
            if (!context.HasExplicitStats)
                return;

            attackStats = context;

            InheritSetupStats(context);
        }

        /// <summary>
        /// Shared pre-checks used by different attack types. Returns true when it's valid to apply a hit and outputs whether a cooldown entry already existed.
        /// </summary>
        protected bool TryBeginHit(Collider2D collision, out HealthAndShieldManager healthAndShieldManager)
        {
            healthAndShieldManager = null;

            if (!hitLayerMask.ContainsLayer(collision.gameObject.layer))
                return false;

            if (!collision.gameObject.TryGetComponentInParent(out healthAndShieldManager))
                return false;

            timeToNextDmg.TryGetValue(healthAndShieldManager, out float nextTime);

            if (nextTime > Time.time)
                return false;

            if (attackStats.AmountOfHits.HasValue && currentAmountOfHits >= attackStats.AmountOfHits.Value)
                return false;

            return true;
        }

        /// <summary>
        /// Update or add the hit cooldown entry for a target and subscribe to its death.
        /// </summary>
        protected void UpdateHitCooldown(HealthAndShieldManager healthAndShieldManager)
        {
            float timeToNextAttack = attackStats.HitRefreshRate != 0 ? Time.time + 1f / attackStats.HitRefreshRate : float.MaxValue;

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
            if (!TryBeginHit(collision, out HealthAndShieldManager healthAndShieldManager))
                return;

            Vector2 hitDirection = ((Vector2)(healthAndShieldManager.transform.position - transform.position)).normalized;
            OnHitInfo onHitInfo = healthAndShieldManager.Damage(attackStats, hitDirection);

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
            if (!attackStats.AmountOfHits.HasValue)
                return;

            currentAmountOfHits++;

            if (currentAmountOfHits >= attackStats.AmountOfHits.Value)
            {
                DestroyAttack();
            }
        }

        /// <returns>If hit a projectile and projectiles count towards hit counter</returns>
        protected bool HandleProjectiles(Collider2D collision)
        {
            if (!hitLayerMask.ContainsLayer(collision.gameObject.layer))
                return false;

            if (!attackStats.DestroyProjectiles)
                return false;

            if (!collision.gameObject.TryGetComponent(out Projectiles.Projectile projectile))
                return false;

            projectile.DestroyAttack();

            return attackStats.ProjectilesCountAsHits;
        }
    }
}
