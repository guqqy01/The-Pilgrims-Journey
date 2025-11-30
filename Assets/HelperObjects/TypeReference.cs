using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[Serializable]
public struct TypeReference
{
    [SerializeField] private MonoScript script;
    [SerializeField, HideInInspector] private string assemblyQualifiedName;

    public Type Type
    {
        get
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName))
            {
                if (script == null)
                    return null;

                SetupAsseblyQualifiedName();
            }

            return Type.GetType(assemblyQualifiedName);
        }
    }

#if UNITY_EDITOR
    public void OnValidate()
    {
        SetupAsseblyQualifiedName();
    }
#endif

    private void SetupAsseblyQualifiedName()
    {
        if (script != null)
        {
            var type = script.GetClass();
            assemblyQualifiedName = type?.AssemblyQualifiedName;
        }
    }
}

