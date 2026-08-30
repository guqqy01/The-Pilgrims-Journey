using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct TypeReference: IEquatable<TypeReference>, ISerializationCallbackReceiver
{
#if UNITY_EDITOR
    [SerializeField] private MonoScript script;
#endif
    [SerializeField, HideInInspector] private string assemblyQualifiedName;

    public bool Equals(TypeReference other) => assemblyQualifiedName == other.assemblyQualifiedName;
    public override bool Equals(object obj) => obj is TypeReference other && Equals(other);
    public override int GetHashCode() => assemblyQualifiedName?.GetHashCode() ?? 0;

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

    private void SetupAsseblyQualifiedName()
    {
        if (script != null)
        {
            var type = script.GetClass();
            assemblyQualifiedName = type?.AssemblyQualifiedName;
        }
    }
#endif

    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        SetupAsseblyQualifiedName();
#endif

    }

    public void OnAfterDeserialize()
    {

    }
}

