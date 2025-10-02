#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;

[Serializable]
public class SceneReference
{
    public SceneAsset sceneAsset;
    public string sceneName;

    public void OnValidate()
    {
        if (sceneAsset != null && sceneAsset.name != sceneName)
        {
            sceneName = sceneAsset.name;
        }
    }


}


[CustomPropertyDrawer(typeof(SceneReference))]
public class SceneReferenceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Find sub-properties
        SerializedProperty sceneAssetProp = property.FindPropertyRelative("sceneAsset");
        SerializedProperty sceneNameProp = property.FindPropertyRelative("sceneName");

        // Draw scene asset field
        var sceneRect = new Rect(position.x, position.y, position.width, position.height);
        EditorGUI.PropertyField(sceneRect, sceneAssetProp, label);

        // Update sceneName automatically
        if (sceneAssetProp.objectReferenceValue != null)
        {
            sceneNameProp.stringValue = sceneAssetProp.objectReferenceValue.name;
        }

        EditorGUI.EndProperty();
    }
}