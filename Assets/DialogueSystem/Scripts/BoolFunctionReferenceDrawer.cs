using System;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomPropertyDrawer(typeof(BoolFunctionReference))]
public class BoolFunctionReferenceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var targetProp = property.FindPropertyRelative("target");
        var methodProp = property.FindPropertyRelative("methodName");

        position = EditorGUI.PrefixLabel(position, label);

        EditorGUI.BeginProperty(position, label, property);

        var rect1 = new Rect(position.x, position.y, position.width * 0.5f - 2, position.height);
        var rect2 = new Rect(position.x + position.width * 0.5f + 2, position.y, position.width * 0.5f - 2, position.height);

        EditorGUI.PropertyField(rect1, targetProp, GUIContent.none);

        if (targetProp.objectReferenceValue != null)
        {
            var obj = targetProp.objectReferenceValue;
            var methods = obj.GetType()
                .GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                .Where(m => m.ReturnType == typeof(bool) && m.GetParameters().Length == 0)
                .Select(m => m.Name)
                .ToArray();

            int index = Mathf.Max(0, Array.IndexOf(methods, methodProp.stringValue));
            index = EditorGUI.Popup(rect2, index, methods);
            if (methods.Length > 0)
                methodProp.stringValue = methods[index];
        }
        else
        {
            EditorGUI.LabelField(rect2, "No Target");
        }

        EditorGUI.EndProperty();
    }
}
