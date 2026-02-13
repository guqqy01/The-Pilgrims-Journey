using System;
using Unity.VisualScripting;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct TypeReference
{
#if UNITY_EDITOR
    [SerializeField] private MonoScript script;
#endif
    [SerializeField, HideInInspector] private string assemblyQualifiedName;

    public Type Type
    {
        get
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(assemblyQualifiedName))
            {
                if (script == null)
                    return null;

                SetupAsseblyQualifiedName();
            }
#endif
            return Type.GetType(assemblyQualifiedName);
        }
    }

#if UNITY_EDITOR
    public void OnValidate()
    {
        SetupAsseblyQualifiedName();
    }


    private void SetupAsseblyQualifiedName()
    {
        if (script != null)
        {
            var type = script.GetClass();
            assemblyQualifiedName = type?.AssemblyQualifiedName;
        }
    }
#endif
}

