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
        public void OnGameEnd();
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

    public interface IOnCompleteLocalMapLevel : IGameManagerCallInterface
    {
        public void OnCompleteLocalMapLevel();
    }

    public interface IOnClear : IGameManagerCallInterface
    {
        public void Clear();
    }

    public interface IOnSetup : IGameManagerCallInterface
    {
        public void SetupNewGame();
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

    public static class InterfaceFinder
    {
        public static List<T> FindInterfaces<T>() where T : IGameManagerCallInterface
        {
            List<T> returnList = new List<T>();
            foreach (var mono in UnityEngine.Object.FindObjectsByType(typeof(MonoBehaviour), FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (mono is IGameManagerCallInterface gameManagerCallInterface && gameManagerCallInterface is T obj)
                {
                    returnList.Add(obj);
                }
            }

            return returnList;
        }
    }

    public static class InterfaceCaller
    {
        public static void CallInterfaces<T>(Action<T> action) where T : IGameManagerCallInterface
        {
            List<T> interfaces = InterfaceFinder.FindInterfaces<T>();
            foreach (T interfaceInstance in interfaces)
            {
                action(interfaceInstance);
            }
        }
    }
}

