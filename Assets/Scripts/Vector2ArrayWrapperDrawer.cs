using UnityEngine;
using UnityEditor;
[CustomPropertyDrawer(typeof(Vector2ListWrapper))]
public class Vector2ArrayWrapperDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 20;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position = EditorGUI.PrefixLabel(position, label);
        if (GUI.Button(position, new GUIContent("Open Tile Picker")))
        {
            TileShapeEditorWindow.Init(property);
        }
    }
}
