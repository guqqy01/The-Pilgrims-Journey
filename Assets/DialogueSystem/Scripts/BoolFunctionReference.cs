using System;
using UnityEngine;

[Serializable]
public class BoolFunctionReference
{
    [SerializeField] private UnityEngine.Object target;
    [SerializeField] private string methodName;
    public UnityEngine.Object Target => target;
    public string MethodName => methodName;

    public bool Invoke()
    {
        if (target == null || string.IsNullOrEmpty(methodName))
            return false;

        var method = target.GetType().GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

        if (method != null && method.ReturnType == typeof(bool) && method.GetParameters().Length == 0)
        {
            return (bool)method.Invoke(target, null);
        }

        Debug.LogWarning($"Method '{methodName}' on '{target.name}' not found or invalid signature.");
        return false;
    }
}
