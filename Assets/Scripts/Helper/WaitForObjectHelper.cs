using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Helper.Wait
{
    public static class WaitForObjectHelper
    {
        /// <summary>
        /// Waits until getObject() returns non-null, then invokes onReady.
        /// Runs on a hidden GameObject placed into the active scene.
        /// </summary>
        public static void WaitForObjectThenPerformAction(Func<UnityEngine.Object> getObject, Action onReady, float checkInterval = 0f)
        {
            // Create a single host GameObject
            var host = new GameObject("WaitForObjectHost");
            host.hideFlags = HideFlags.HideAndDontSave;

            // Ensure it's in the active scene (important in multi-scene setups)
            SceneManager.MoveGameObjectToScene(host, SceneManager.GetActiveScene());

            var waiter = host.AddComponent<WaitForObject>();
            waiter.Setup(getObject, onReady, checkInterval);

            // Start the coroutine on the MonoBehaviour so the polling actually runs
            waiter.StartWaiting();
        }
    }

    internal class WaitForObject : MonoBehaviour
    {
        private Func<UnityEngine.Object> _getObject;
        private Action _onReady;
        private float _checkInterval;

        public void Setup(Func<UnityEngine.Object> getObject, Action onReady, float checkInterval = 0f)
        {
            _getObject = getObject ?? throw new ArgumentNullException(nameof(getObject));
            _onReady = onReady;
            _checkInterval = checkInterval;
        }

        // Public kickoff so the helper controls when the coroutine starts
        public void StartWaiting()
        {
            StartCoroutine(WaitForObjectThenPerformActionCoroutine());
        }

        private IEnumerator WaitForObjectThenPerformActionCoroutine()
        {
            try
            {
                if (_checkInterval <= 0f)
                {
                    yield return new WaitUntil(() => _getObject() != null);
                }
                else
                {
                    while (_getObject() == null)
                    {
                        yield return new WaitForSeconds(_checkInterval);
                    }
                }

                _onReady?.Invoke();
            }
            finally
            {
                // Always destroy the host GameObject so we don't leak
                Destroy(gameObject);
            }
        }
    }
}

