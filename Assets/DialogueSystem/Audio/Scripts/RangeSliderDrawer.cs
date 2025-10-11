using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FloatRangeSlider))]
public class FloatRangeSliderDrawer : PropertyDrawer
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

        float limitRectWidth = 35f;

        position = EditorGUI.PrefixLabel(position, label);

        Rect minLimitRect = new Rect(position) { width = limitRectWidth };
        Rect maxLimitRect = new Rect(position) { xMin = position.xMax - limitRectWidth };
        Rect sliderRect = new Rect(position) { xMin = minLimitRect.xMax + 2, xMax = maxLimitRect.xMin - 2 };

        EditorGUI.PropertyField(minLimitRect, minValueProp, GUIContent.none);
        EditorGUI.PropertyField(maxLimitRect, maxValueProp, GUIContent.none);

        EditorGUI.MinMaxSlider(position, GUIContent.none, ref min, ref max, minValueProp.floatValue, maxValueProp.floatValue);

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

[CustomPropertyDrawer(typeof(IntRangeSlider))]
public class IntRangeSliderDrawer : PropertyDrawer
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

        int max = vector2Prop.vector2IntValue.x > vector2Prop.vector2IntValue.y ? vector2Prop.vector2IntValue.x : vector2Prop.vector2IntValue.y;
        int min = vector2Prop.vector2IntValue.x < vector2Prop.vector2IntValue.y ? vector2Prop.vector2IntValue.x : vector2Prop.vector2IntValue.y;

        float refMax = (float)max;
        float refMin = (float)min;

        float limitRectWidth = 35f;

        position = EditorGUI.PrefixLabel(position, label);

        Rect minLimitRect = new Rect(position) { width = limitRectWidth };
        Rect maxLimitRect = new Rect(position) { xMin = position.xMax - limitRectWidth };
        Rect sliderRect = new Rect(position) { xMin = minLimitRect.xMax + 2, xMax = maxLimitRect.xMin - 2 };

        EditorGUI.PropertyField(minLimitRect, minValueProp, GUIContent.none);
        EditorGUI.PropertyField(maxLimitRect, maxValueProp, GUIContent.none);
        EditorGUI.MinMaxSlider(sliderRect, GUIContent.none, ref refMin, ref refMax, minValueProp.intValue, maxValueProp.intValue);

        if (vector2Prop.vector2IntValue.x > vector2Prop.vector2IntValue.y)
        {
            vector2Prop.vector2IntValue = new Vector2Int(Mathf.RoundToInt(refMax), Mathf.RoundToInt(refMin));
        }
        else
        {
            vector2Prop.vector2IntValue = new Vector2Int(Mathf.RoundToInt(refMin), Mathf.RoundToInt(refMax));
        }
    }
}