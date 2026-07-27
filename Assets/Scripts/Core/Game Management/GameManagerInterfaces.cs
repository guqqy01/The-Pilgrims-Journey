using UnityEngine;
using System.Collections.Generic;
using System;
namespace GameManagerCalls
{
    public interface IGameManagerCallInterface 
    {
    }

    public interface IOnPlayLocalLevel : IGameManagerCallInterface
    {
        public void OnPlayLocalLevel();
    }

    public interface IOnCompletedLevel : IGameManagerCallInterface
    {
        public void OnLevelCompleted();
    }

    public interface IOnGameEnd : IGameManagerCallInterface
    {
        public void OnGameEnd(string message);
    }

    public interface IOnBeforeCreateLocalMap : IGameManagerCallInterface
    {
        public void OnBeforeCreateLocalMap();
    }

    public interface IOnCreateLocalMap : IGameManagerCallInterface
    {
        public void OnCreateLocalMap();
    }

    public interface IOnAfterCreateLocalMap : IGameManagerCallInterface
    {
        public void AfterCreateLocalMap();
    }

    public interface IOnCreateCurrentMapPoint : IGameManagerCallInterface
    {
        public void OnCreateCurrentMapPoint();
    }

    public interface IOnGenerateNewRandomEvent : IGameManagerCallInterface
    {
        public void OnGenerateNewRandomEvent();
    }

    public interface IOnCompleteLocalMapLevel : IGameManagerCallInterface
    {
        public void OnCompleteLocalMapLevel();
    }

    public interface IOnClear : IGameManagerCallInterface
    {
        public void Clear();
    }

    public interface IOnSetupNewGame : IGameManagerCallInterface
    {
        public void OnSetupNewGame();
    }

    public interface IOnLoadAfterMapPointSceneLoad : IGameManagerCallInterface
    {
        public void LoadAfterMapPointSceneLoad();
    }

    public interface IOnLoad : IGameManagerCallInterface
    {
        public void Load();
    }

    public interface IOnSave : IGameManagerCallInterface
    {
        public void Save();
    }

    public interface IOnDelete : IGameManagerCallInterface
    {
        public void Delete();
    }

    public interface IOnUnitPassed : IGameManagerCallInterface
    {
        public void OnUnitPassed(int totalPassed, int requiredToPass);
    }

    // Central registry that components can optionally register with for deterministic discovery.
    public static class InterfaceRegistry<T> where T : IGameManagerCallInterface
    {
        private static readonly List<T> registered = new List<T>();

        public static void Register(T item)
        {
            if (item == null) return;
            if (registered.Contains(item)) return;
            registered.Add(item);
        }

        public static void Unregister(T item)
        {
            if (item == null) return;
            registered.Remove(item);
        }

        public static IReadOnlyList<T> GetAll() => registered.AsReadOnly();

        public static bool HasAny => registered.Count > 0;

        public static void Clear() => registered.Clear();
    }

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
            foreach (var mono in UnityEngine.Object.FindObjectsByType(typeof(MonoBehaviour), FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (mono is IGameManagerCallInterface g && g is T typed)
                    results.Add(typed);
            }

            return results;
        }
    }

    public static class InterfaceCaller
    {
        // Call interfaces safely. Exceptions in one handler won't prevent others from running.
        public static void CallInterfaces<T>(Action<T> action) where T : IGameManagerCallInterface
        {
            if (action == null) return;

            List<T> interfaces = InterfaceFinder.FindInterfaces<T>();

            if (interfaces == null || interfaces.Count == 0)
            {
                // No listeners found - that's fine. Avoid spamming logs in normal flow.
                return;
            }

            foreach (T interfaceInstance in interfaces)
            {
                try
                {
                    action(interfaceInstance);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Exception while calling interface {typeof(T).Name} on {interfaceInstance}: {ex}");
                }
            }
        }
    }
}

