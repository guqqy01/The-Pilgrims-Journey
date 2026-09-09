using System;
using UnityEngine;

[Serializable]
public struct InterfaceReference<TInterface> where TInterface : class
{
    [SerializeField]
    private MonoBehaviour target;

    public TInterface Interface
    {
        get
        {
            if (target == null)
            {
                Debug.LogError($"Target is null for interface {typeof(TInterface).Name}");
                return null;
            }
            if (target is TInterface iface)
                return iface;
            Debug.LogError($"Target {target} does not implement interface {typeof(TInterface).Name}");
            return null;
        }
    }
}
