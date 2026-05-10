using System;
using UnityEngine;

public interface IToBeInitiatedFromRoot
{
    public bool IsMonoBehaviour()
    {
        return this.GetType().IsSubclassOf(typeof(MonoBehaviour));
    }

    public void InitiateFromRootAtAwake();
}
