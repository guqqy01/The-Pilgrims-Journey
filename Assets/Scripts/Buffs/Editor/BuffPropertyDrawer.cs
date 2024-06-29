using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using System.Linq;
using System.Collections;
using System.Text.RegularExpressions;
using System;
using UnityEngine.TextCore.Text;

[CustomPropertyDrawer(typeof(Buff))]
public class BuffPropertyDrawer : PropertyDrawer
{
    private const float baseValueLabelWidth = 63f;
    private const float symbolValueWidth = 17f;
    private const float openingBracketWidth = 10f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 2f * EditorGUIUtility.singleLineHeight + 2f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty statProperty =  property.FindPropertyRelative("stat");

        Rect statRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };

        Rect formulaRect = new Rect(position) { yMin = position.yMin + EditorGUIUtility.singleLineHeight + 2f, height = EditorGUIUtility.singleLineHeight };

        EditorGUI.PropertyField(statRect, statProperty);

        PropertyInLineDrawer.DrawPropertiesInText(formulaRect, property, "(^BaseBuff^ + Base Value) * ^Multiplier^ + ^AdditionalBuff^", new GUIContent("Total Buff"), "^"[0]);
    }
}
