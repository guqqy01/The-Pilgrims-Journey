using System;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour, IToBeInitiatedFromRoot where T : Singleton<T>
{
    public static T Instance { protected set; get; }

    public static bool HasInstance { get { return Instance != null; } }
    private static event Action<T> OnInstanceReady;
    public virtual void InitiateFromRootAtAwake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as T;
        OnInstanceReady?.Invoke(Instance);
    }

    private void OnDestroy()
    {
        CleanUp();

        if (Instance == this)
            Instance = null;
    }

    protected virtual void CleanUp() { }

    public static void WhenReady(Action<T> onReady)
    {
        if (HasInstance) { onReady(Instance); return; }
        void Handler(T instance) { OnInstanceReady -= Handler; onReady(instance); }
        OnInstanceReady += Handler;
    }
}

public class SingletonHelper<T> where T : class
{
    public T Instance { get; private set; }
    public bool HasInstance => Instance != null;
    private event Action<T> OnReady;

    public bool TryClaim(T self, Action onDuplicate)
    {
        if (Instance != null && !ReferenceEquals(Instance, self))
        {
            onDuplicate?.Invoke();
            return false;
        }
        Instance = self;
        OnReady?.Invoke(self);
        return true;
    }

    public void Release(T self)
    {
        if (ReferenceEquals(Instance, self)) Instance = null;
    }

    public void WhenReady(Action<T> onReady)
    {
        if (HasInstance) { onReady(Instance); return; }
        void Handler(T instance) { OnReady -= Handler; onReady(instance); }
        OnReady += Handler;
    }
}