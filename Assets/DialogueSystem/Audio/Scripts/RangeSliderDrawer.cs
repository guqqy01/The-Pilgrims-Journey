using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FloatRangeSlider))]
public class FloatRangeSliderDrawer : PropertyDrawer
{
    public const float limitRectWidth = 100f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty minValueProp = property.FindPropertyRelative("MinLimit");
        SerializedProperty maxValueProp = property.FindPropertyRelative("MaxLimit");
        SerializedProperty startProp = property.FindPropertyRelative("Start");
        SerializedProperty lengthProp = property.FindPropertyRelative("Length");

        float max = startProp.floatValue + lengthProp.floatValue;
        float min = startProp.floatValue;

        position = EditorGUI.PrefixLabel(position, label);

        Rect minLimitRect = new Rect(position) { width = limitRectWidth };
        Rect maxLimitRect = new Rect(position) { xMin = position.xMax - limitRectWidth };
        Rect sliderRect = new Rect(position) { xMin = minLimitRect.xMax + 2, xMax = maxLimitRect.xMin - 2 };

        EditorGUI.PropertyField(minLimitRect, minValueProp, GUIContent.none);
        EditorGUI.PropertyField(maxLimitRect, maxValueProp, GUIContent.none);

        EditorGUI.MinMaxSlider(position, GUIContent.none, ref min, ref max, minValueProp.floatValue, maxValueProp.floatValue);

        startProp.floatValue = min;
        lengthProp.floatValue = max - min;
    }
}

[CustomPropertyDrawer(typeof(IntRangeSlider))]
public class IntRangeSliderDrawer : PropertyDrawer
{
    public const float limitRectWidth = 100f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty minValueProp = property.FindPropertyRelative("MinLimit");
        SerializedProperty maxValueProp = property.FindPropertyRelative("MaxLimit");
        SerializedProperty startProp = property.FindPropertyRelative("Start");
        SerializedProperty lengthProp = property.FindPropertyRelative("Length");

        int max = startProp.intValue + lengthProp.intValue;
        int min = startProp.intValue;

        float refMax = (float)max;
        float refMin = (float)min;

        position = EditorGUI.PrefixLabel(position, label);

        Rect minLimitRect = new Rect(position) { width = limitRectWidth };
        Rect maxLimitRect = new Rect(position) { xMin = position.xMax - limitRectWidth };
        Rect sliderRect = new Rect(position) { xMin = minLimitRect.xMax + 2, xMax = maxLimitRect.xMin - 2 };

        EditorGUI.PropertyField(minLimitRect, minValueProp, GUIContent.none);
        EditorGUI.PropertyField(maxLimitRect, maxValueProp, GUIContent.none);
        EditorGUI.MinMaxSlider(sliderRect, GUIContent.none, ref refMin, ref refMax, minValueProp.intValue, maxValueProp.intValue);

        startProp.intValue = Mathf.RoundToInt(refMin);
        lengthProp.intValue = Mathf.RoundToInt(refMax) - Mathf.RoundToInt(refMin);
    }
}