using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(IntOrFloatObject))]
public class IntOrFloatObjectPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty isIntProperty = property.FindPropertyRelative("isInt");
        if (isIntProperty.boolValue)
        {
            SerializedProperty intValueProperty = property.FindPropertyRelative("intValue");
            intValueProperty.intValue = EditorGUI.IntField(position, label, intValueProperty.intValue);
        }
        else
        {
            SerializedProperty floatValueProperty = property.FindPropertyRelative("floatValue");
            floatValueProperty.floatValue = EditorGUI.FloatField(position, label, floatValueProperty.floatValue);
        }
    }
}
