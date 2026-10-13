using System.Collections.Generic;
using UnityEngine;
namespace GameManagerCalls
{
    /// <summary>
    /// Put on the scene root (SceneRoot requires it). Registers every MonoBehaviour
    /// under this object, including inactive ones, with the game call registry,
    /// and unregisters them when the scene root is destroyed (e.g. scene unload).
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1000)] // run before other Awakes so children can be called early
    public class SceneGameCallRegistrar : MonoBehaviour
    {
        private readonly List<MonoBehaviour> registered = new List<MonoBehaviour>();

        private void Awake()
        {
            RegisterHierarchy(gameObject);
        }

        private void OnDestroy()
        {
            foreach (var mono in registered)
                InterfaceRegistrar.Unregister(mono);

            registered.Clear();
        }

        /// <summary>
        /// Call this for objects spawned at runtime under (or alongside) this scene root.
        /// Safe to call more than once for the same object.
        /// </summary>
        public void RegisterHierarchy(GameObject root)
        {
            foreach (var mono in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mono == null) continue; // missing script
                if (registered.Contains(mono)) continue;

                registered.Add(mono);
                InterfaceRegistrar.Register(mono);
            }
        }
    }
}

