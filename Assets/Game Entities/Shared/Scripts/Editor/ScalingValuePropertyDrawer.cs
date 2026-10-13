using UnityEngine;
using UnityEditor;
using Spawning;
[CustomPropertyDrawer(typeof(ScalingValue))]
public class ScalingValuePropertyDrawer : PropertyDrawer
{
    private readonly string nameOfBaseValue = nameof(ScalingValue.baseValue);
    private readonly string nameOfPerDifficulty = nameof(ScalingValue.perDifficulty);
    private readonly string nameOfClamp = nameof(ScalingValue.clamp);
    private readonly string nameOfMin = nameof(ScalingValue.min);
    private readonly string nameOfMax = nameof(ScalingValue.max);

    private const float spacing = 2f;
    private float SLH => EditorGUIUtility.singleLineHeight;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty baseValueProp = property.FindPropertyRelative(nameOfBaseValue);
        SerializedProperty perDifficultyProp = property.FindPropertyRelative(nameOfPerDifficulty);
        SerializedProperty clampProp = property.FindPropertyRelative(nameOfClamp);
        SerializedProperty minProp = property.FindPropertyRelative(nameOfMin);
        SerializedProperty maxProp = property.FindPropertyRelative(nameOfMax);

        Rect foldoutRect = new Rect(position) { height = SLH };
        Rect baseValueRect = new Rect(position) { yMin = foldoutRect.yMax + spacing, height = SLH };
        Rect perDifficultyRect = new Rect(position) { yMin = baseValueRect.yMax + spacing, height = SLH };
        Rect clampRect = new Rect(position) { yMin = perDifficultyRect.yMax + spacing, height = SLH };
        Rect minRect = new Rect(position) { yMin = clampRect.yMax + spacing, height = SLH };
        Rect maxRect = new Rect(position) { yMin = minRect.yMax + spacing, height = SLH };

        property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(foldoutRect, property.isExpanded, label);

        if(property.isExpanded)
        {
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(baseValueRect, baseValueProp, new GUIContent("Base Value"));
            EditorGUI.PropertyField(perDifficultyRect, perDifficultyProp, new GUIContent("Per Difficulty"));
            EditorGUI.PropertyField(clampRect, clampProp, new GUIContent("Clamp"));
            if(clampProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(minRect, minProp, new GUIContent("Min"));
                EditorGUI.PropertyField(maxRect, maxProp, new GUIContent("Max"));
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }
        EditorGUI.EndFoldoutHeaderGroup();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if(!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        SerializedProperty clampProp = property.FindPropertyRelative(nameOfClamp);
        if (clampProp.boolValue)
        {
            return SLH * 6 + 10;
        }
        return SLH * 4 + 6;
    }
}
