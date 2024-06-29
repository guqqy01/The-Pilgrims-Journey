using UnityEngine;
using System.Collections.Generic;
using System;
namespace GameManagerCalls
{
    public interface IGameManagerCallInterface { }

    public interface ISetup : IGameManagerCallInterface
    {
        public void Setup();
    }

    public interface IOnLevelCompleted : IGameManagerCallInterface
    {
        public void OnLevelCompleted();
    }

    public interface IOnGameEnd : IGameManagerCallInterface
    {
        public void OnGameEnd();
    }

    public interface IUnitPassed : IGameManagerCallInterface
    {
        public void OnUnitPassed(int totalPassed, int requiredToPass);
    }

    public interface IClear : IGameManagerCallInterface
    {
        public void Clear();
    }

    public interface IAfterCreateLocalMap : IGameManagerCallInterface
    {
        public void AfterCreateLocalMap();
    }

    public interface IOnCreateLocalMap : IGameManagerCallInterface
    {
        public void OnCreateLocalMap();
    }

    public interface IOnBeforeCreateLocalMap : IGameManagerCallInterface
    {
        public void OnBeforeCreateLocalMap();
    }

    public static class InterfaceFinder
    {
        public static List<T> FindInterfaces<T>() where T : IGameManagerCallInterface
        {
            List<T> returnList = new List<T>();
            foreach (var mono in UnityEngine.Object.FindObjectsByType(typeof(MonoBehaviour), FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (mono is IGameManagerCallInterface gameManagerCallInterface)
                {
                    returnList.Add((T)gameManagerCallInterface);
                }
            }

            return returnList;
        }
    }
}

