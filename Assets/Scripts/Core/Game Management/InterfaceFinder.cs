using System.Collections.Generic;
using UnityEngine;
namespace GameManagerCalls
{
    public static class InterfaceFinder
    {
        // Find interfaces by scanning all MonoBehaviours. This is the fallback used when no registry entries exist.
        public static List<T> FindInterfaces<T>() where T : IGameManagerCallInterface
        {
            // If components have explicitly registered, return those to ensure deterministic order.
            if (InterfaceRegistry<T>.HasAny)
            {
                return new List<T>(InterfaceRegistry<T>.GetAll());
            }

            var results = new List<T>();

            // Use Unity's search to include inactive objects too. Keep this as a single place for discovery.
            foreach (var mono in UnityEngine.Object.FindObjectsByType(typeof(MonoBehaviour), FindObjectsInactive.Include))
            {
                if (mono is IGameManagerCallInterface g && g is T typed)
                    results.Add(typed);
            }

            return results;
        }
    }
}

