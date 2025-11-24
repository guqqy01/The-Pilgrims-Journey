using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour, ISingletonToBeInitiatedFromRoot where T : Singleton<T>
{
    public static T Instance { protected set; get; }

    public virtual void InitiateSingleton()
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

