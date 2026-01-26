using Heros.Attacks.Spot;
using System.Collections.Generic;
using UnityEngine;


namespace Heros.Attacks
{
    public abstract class HeroAttackSpawn<TInitContext, TStatsContext> : MonoBehaviour 
        where TInitContext : HeroAttackInitContext 
        where TStatsContext : HeroAttackStatsContext
    {
        [SerializeField]
        protected Collider2D hitCollider;

        [SerializeField]
        protected LayerMask hitLayerMask;

        protected readonly Dictionary<HealthAndShieldManager, float> timeToNextDmg = new Dictionary<HealthAndShieldManager, float>();

        protected readonly List<HealthAndShieldManager> toRemove = new List<HealthAndShieldManager>();

        protected int damage;
        protected float knockback;
        protected float hitSpeed;
        protected bool destroyProjectiles;
        protected int currentAmountOfHits;
        protected int? amountOfHits;
        protected bool projectilesCountAsHits;

        private void FixedUpdate()
        {
            FixedUpdateMovement();
        }

        private void OnEnemyDie(Health health)
        {
            if (health == null)
                return;

            if (health.TryGetComponent(out HealthAndShieldManager healthAndShieldManager))
            {
                timeToNextDmg.Remove(healthAndShieldManager);
            }

            health.onDie -= OnEnemyDie;
        }

        protected abstract void FixedUpdateMovement();

        protected abstract void InheritInit(TInitContext initContext);
        protected abstract void InheritSetupStats(TStatsContext statsContext);

        public void Init(TInitContext initContext, TStatsContext statsContext)
        {
            InheritInit(initContext);
            SetupStats(statsContext);
        }

        public void SetupStats(TStatsContext context)
        {
            damage = context.heroAttackStats.damage;
            hitSpeed = context.heroAttackStats.hitRefreshSpeed;
            destroyProjectiles = context.heroAttackStats.destroyProjectiles;
            knockback = context.heroAttackStats.knockback;
            amountOfHits = context.heroAttackStats.amountOfHits;

            projectilesCountAsHits = context.heroAttackStats.projectilesCountAsHits;

            InheritSetupStats(context);
        }

        private void HandleColliderHits(Collider2D collision)
        {
            if (!hitLayerMask.ContainsLayer(collision.gameObject.layer))
                return;

            if (!collision.gameObject.TryGetComponentInParent(out HealthAndShieldManager healthAndShieldManager))
                return;

            bool exists = timeToNextDmg.TryGetValue(healthAndShieldManager, out float nextTime);

            if (exists && nextTime > Time.time)
                return;

            if (amountOfHits < currentAmountOfHits)
                return;

            Vector2 hitDirection = ((Vector2)(healthAndShieldManager.transform.position - transform.position)).normalized;
            healthAndShieldManager.Damage(damage, hitDirection, knockback);

            float timeToNextAttack = hitSpeed != 0 ? Time.time + 1f / hitSpeed : float.MaxValue;

            if (exists)
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

        public virtual void DestroyAttack()
        {
            Destroy(gameObject);
        }

        protected virtual void OnDestroy()
        {
            foreach (var kvp in timeToNextDmg)
            {
                if (kvp.Key != null)
                    kvp.Key.Health.onDie -= OnEnemyDie;
            }

            timeToNextDmg.Clear();
        }
    }
}