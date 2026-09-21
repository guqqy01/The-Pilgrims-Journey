#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System;


[CustomPropertyDrawer(typeof(TypeReference<>))]
public class TypeReferenceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var scriptProp = property.FindPropertyRelative("script");
        Type constraint = GetConstraintType(fieldInfo.FieldType) ?? typeof(UnityEngine.Object);

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();

        var newScript = EditorGUI.ObjectField(position, label, scriptProp.objectReferenceValue, typeof(MonoScript), false) as MonoScript;

        if (EditorGUI.EndChangeCheck())
        {
            if (newScript == null || IsValid(newScript, constraint))
                scriptProp.objectReferenceValue = newScript;
            else
                Debug.LogWarning($"'{newScript.GetClass()?.Name}' does not inherit from {constraint.Name}.");
        }

        EditorGUI.EndProperty();
    }

    private static bool IsValid(MonoScript script, Type constraint)
    {
        var type = script.GetClass();
        return type != null && !type.IsAbstract && constraint.IsAssignableFrom(type);
    }

    // Walks past arrays/lists to find the closed TypeReference<T> and pull out T.
    private static Type GetConstraintType(Type fieldType)
    {
        if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(TypeReference<>))
            return fieldType.GetGenericArguments()[0];

        if (fieldType.IsArray)
            return GetConstraintType(fieldType.GetElementType());

        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(fieldType) && fieldType.IsGenericType)
            return GetConstraintType(fieldType.GetGenericArguments()[0]);

        return null;
    }
}
#endif