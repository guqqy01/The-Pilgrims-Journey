using System;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour, IToBeInitiatedFromRoot where T : Singleton<T>
{
    public static T Instance { protected set; get; }

    public static bool HasInstance { get { return Instance != null; } }

    public virtual void InitiateFromRootAtAwake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as T;
    }

    private void OnDestroy()
    {
        CleanUp();

        if (Instance == this)
            Instance = null;
    }

    protected virtual void CleanUp() { }
}

public class SingletonHelper<T> where T : class
{
    public T Instance { get; private set; }
    public bool HasInstance => Instance != null;

    /// Call from InitiateFromRootAtAwake(). Returns false if `self` is a
    /// duplicate and onDuplicate has already handled it (e.g. destroyed it).
    public bool TryClaim(T self, Action onDuplicate)
    {
        if (Instance != null && !ReferenceEquals(Instance, self))
        {
            onDuplicate?.Invoke();
            return false;
        }
        Instance = self;
        return true;
    }

    /// Call from OnDestroy(). Only clears Instance if this really was the owner.
    public void Release(T self)
    {
        if (ReferenceEquals(Instance, self))
            Instance = null;
    }
}