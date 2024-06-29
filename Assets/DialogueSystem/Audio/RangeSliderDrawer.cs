using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RangeSlider))]
public class RangeSliderDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty minValueProp = property.FindPropertyRelative("MinLimit");
        SerializedProperty maxValueProp = property.FindPropertyRelative("MaxLimit");
        SerializedProperty vector2Prop = property.FindPropertyRelative("MinMaxValues");

        float max = vector2Prop.vector2Value.x > vector2Prop.vector2Value.y ? vector2Prop.vector2Value.x : vector2Prop.vector2Value.y;
        float min = vector2Prop.vector2Value.x < vector2Prop.vector2Value.y ? vector2Prop.vector2Value.x : vector2Prop.vector2Value.y;

        EditorGUI.MinMaxSlider(position, label, ref min, ref max, minValueProp.floatValue, maxValueProp.floatValue);

        if(vector2Prop.vector2Value.x > vector2Prop.vector2Value.y)
        {
            vector2Prop.vector2Value = new Vector2(max, min);
        }
        else
        {
            vector2Prop.vector2Value = new Vector2(min, max);
        }
    }
}