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

[Serializable]
public struct TypeReference<TBase> : IEquatable<TypeReference<TBase>>, ISerializationCallbackReceiver
{
#if UNITY_EDITOR
    [SerializeField] private MonoScript script;
#endif
    [SerializeField, HideInInspector] private string assemblyQualifiedName;

    public bool Equals(TypeReference<TBase> other) => assemblyQualifiedName == other.assemblyQualifiedName;
    public override bool Equals(object obj) => obj is TypeReference<TBase> other && Equals(other);
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

                SetupAssemblyQualifiedName();
            }
#endif
            return Type.GetType(assemblyQualifiedName);
        }
    }

#if UNITY_EDITOR
    private void SetupAssemblyQualifiedName()
    {
        if (script == null)
        {
            assemblyQualifiedName = null;
            return;
        }

        var type = script.GetClass();

        if (type == null || type.IsAbstract || !typeof(TBase).IsAssignableFrom(type))
        {
            Debug.LogWarning($"[TypeReference<{typeof(TBase).Name}>] '{script.name}' does not derive from {typeof(TBase).Name}. Clearing reference.");
            script = null;
            assemblyQualifiedName = null;
            return;
        }

        assemblyQualifiedName = type.AssemblyQualifiedName;
    }
#endif

    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        SetupAssemblyQualifiedName();
#endif
    }

    public void OnAfterDeserialize() { }
}