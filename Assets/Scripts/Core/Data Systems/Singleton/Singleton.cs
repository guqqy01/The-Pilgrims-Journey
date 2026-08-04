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

