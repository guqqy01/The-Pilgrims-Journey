using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEditor.Search;

[CustomPropertyDrawer(typeof(Singleton))]
public class SingletonPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty singletonProperty =  property.FindPropertyRelative("singleton");

        EditorGUI.BeginChangeCheck();

        Object holderObject = EditorGUILayout.ObjectField(label, singletonProperty.objectReferenceValue, typeof(MonoBehaviour), true);

        if (EditorGUI.EndChangeCheck())
        {
            if (holderObject is ISingletonToBeInitiatedFromRoot)
            {
                singletonProperty.objectReferenceValue = holderObject;
            }
            else if (holderObject is MonoBehaviour mono && mono.TryGetComponent(out ISingletonToBeInitiatedFromRoot singleton) && singleton is Object singletonObject)
            {
                singletonProperty.objectReferenceValue = singletonObject;
            }
            else
            {
                Debug.LogWarning($"Object: {holderObject}, does not inhert from or have a component that inherits from ISingletonToBeInitiatedFromRoot");
            }
        }

        if (GUILayout.Button("Find Singleton"))
        {
            SingletonPicker.Show(singletonProperty);
        }
    }
}
