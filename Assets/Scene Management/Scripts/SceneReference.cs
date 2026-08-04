#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;

[Serializable]
public class SceneReference
{
    public string sceneName;

#if UNITY_EDITOR
    [SerializeField]
    private SceneAsset sceneAsset;

    public void OnValidate()
    {
        if (sceneAsset != null && sceneAsset.name != sceneName)
        {
            sceneName = sceneAsset.name;
        }
    }
#endif

}

#if UNITY_EDITOR

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
        if(sceneAssetProp != null)
        {
            EditorGUI.PropertyField(sceneRect, sceneAssetProp, label);

            // Update sceneName automatically
            if (sceneAssetProp.objectReferenceValue != null)
            {
                sceneNameProp.stringValue = sceneAssetProp.objectReferenceValue.name;
            }
        }

        EditorGUI.EndProperty();
    }
}
#endif