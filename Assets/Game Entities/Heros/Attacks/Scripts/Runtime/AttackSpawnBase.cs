using System;
using System.Collections.Generic;
using UnityEngine;

namespace Attacks
{
    public abstract class AttackSpawnBase : MonoBehaviour
    {
        protected readonly Dictionary<HealthAndShieldManager, float> timeToNextDmg = new Dictionary<HealthAndShieldManager, float>();

        protected event Action OnSpawn;
        protected event Action<OnHitInfo> OnHit;

        [SerializeField]
        protected List<ManagedAudioSource> spawnAudioSources;
        [SerializeField]
        protected List<AudioClipAndSettings> hitAudioSources;

        // Pool support: if set, DestroyAttack will call this instead of Destroy(gameObject)
        private Action returnToPoolCallback;

        protected virtual void Awake()
        {
            OnSpawn += HandleAudioOnSpawn;
            OnHit += HandleAudioOnHit;
        }

        private void HandleAudioOnSpawn()
        {
            if (spawnAudioSources == null)
                return;

            for (int i = 0; i < spawnAudioSources.Count; i++)
            {
                var src = spawnAudioSources[i];
                if (src == null) continue;
                src.Play();
            }
        }

        private void HandleAudioOnHit(OnHitInfo hitInfo)
        {
            if (hitAudioSources == null || hitAudioSources.Count == 0)
                return;

            AudioClipAndSettings toPlay = null;
            for (int i = 0; i < hitAudioSources.Count; i++)
            {
                if (hitAudioSources[i] != null)
                {
                    toPlay = hitAudioSources[i];
                    break;
                }
            }

            if (toPlay != null && AudioManager.Instance != null)
                AudioManager.Instance.PlayClipOnObject(toPlay, gameObject);
        }

        /// <summary>
        /// Allow derived classes to raise the OnSpawn event.
        /// </summary>
        protected void RaiseOnSpawn()
        {
            OnSpawn?.Invoke();
        }

        /// <summary>
        /// Allow derived classes to raise the OnHit event.
        /// </summary>
        protected void RaiseOnHit(OnHitInfo info)
        {
            OnHit?.Invoke(info);
        }

        /// <summary>
        /// Called by pools when this instance is retrieved from the pool. Override to reset instance state.
        /// </summary>
        public virtual void OnPoolGet() { }

        /// <summary>
        /// Called by pools when this instance is released back into the pool. Override to cleanup state.
        /// Default implementation clears any target cooldowns and unsubscribes death handlers to avoid stale state.
        /// </summary>
        public virtual void OnPoolRelease()
        {
            RemoveAllOnTargetDieSubscription();

            timeToNextDmg.Clear();
        }

        /// <summary>
        /// Set a callback for returning this instance to a pool. When set, DestroyAttack will call this callback instead of destroying the GameObject.
        /// </summary>
        /// <param name="callback"></param>
        public void SetReturnToPoolCallback(Action callback)
        {
            returnToPoolCallback = callback;
        }

        public virtual void DestroyAttack()
        {
            if (returnToPoolCallback != null)
            {
                // Prevent re-entrancy and double-release: clear the callback before invoking it so subsequent
                // calls to DestroyAttack won't fall through to Destroy(gameObject) if OnRelease also clears the callback.
                var callback = returnToPoolCallback;
                returnToPoolCallback = null;

                try
                {
                    callback();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

                return;
            }

            // If the GameObject is already inactive it was likely returned to a pool earlier; avoid destroying a pooled instance.
            if (!gameObject.activeInHierarchy)
                return;

            Destroy(gameObject);
        }

        protected void OnEnemyDie(Health health)
        {
            if (health == null)
                return;

            if (health.TryGetComponent(out HealthAndShieldManager healthAndShieldManager))
            {
                timeToNextDmg.Remove(healthAndShieldManager);
            }

            health.onDie -= OnEnemyDie;
        }

        private void RemoveAllOnTargetDieSubscription()
        {
            foreach (var kvp in timeToNextDmg)
            {
                if (kvp.Key != null && kvp.Key.Health != null)
                    kvp.Key.Health.onDie -= OnEnemyDie;
            }
        }

        protected virtual void OnDestroy()
        {
            RemoveAllOnTargetDieSubscription();

            OnSpawn -= HandleAudioOnSpawn;
            OnHit -= HandleAudioOnHit;

            OnSpawn = null;
            OnHit = null;

            timeToNextDmg.Clear();
        }
    }
}
