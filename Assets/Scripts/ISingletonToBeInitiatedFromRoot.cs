using System;
using UnityEngine;

public interface ISingletonToBeInitiatedFromRoot
{
    public bool IsMonoBehaviour()
    {
        return this.GetType().IsSubclassOf(typeof(MonoBehaviour));
    }

    public void InitiateSingleton();
}
