using UnityEngine;
using UnityEditor;
using System;
using Object = UnityEngine.Object;
[CustomPropertyDrawer(typeof(InterfaceReference<>))]
public class InterfaceReferencePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty targetProp = property.FindPropertyRelative("target");

        // Get the TInterface type argument from the field's generic type
        Type interfaceType = fieldInfo.FieldType.GetGenericArguments()[0];

        Object beforeObj = targetProp.objectReferenceValue;

        EditorGUI.BeginProperty(position, label, property);

        Object obj = EditorGUI.ObjectField(position, label, beforeObj, typeof(Object), true);

        if (obj is GameObject go)
        {
            var comp = go.GetComponent(interfaceType);
            obj = comp;
        }

        if (obj == beforeObj)
        {
            EditorGUI.EndProperty();
            return;
        }

        if (obj == null)
        {
            targetProp.objectReferenceValue = null;
            EditorGUI.EndProperty();
            return;
        }

        // obj will be a MonoBehaviour (since that's the ObjectField type) — check it implements TInterface
        if (interfaceType.IsAssignableFrom(obj.GetType()))
        {
            targetProp.objectReferenceValue = obj;
        }
        else
        {
            Debug.LogWarning($"{obj.name} does not implement {interfaceType.Name}");
            // leave the field unchanged (don't assign the invalid object)
        }

        EditorGUI.EndProperty();
    }
}