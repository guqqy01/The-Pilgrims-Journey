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
        return 4f * EditorGUIUtility.singleLineHeight + 6f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty statProperty =  property.FindPropertyRelative("stat");
        SerializedProperty baseBuffprop = property.FindPropertyRelative("BaseBuff");
        SerializedProperty multiplierProp = property.FindPropertyRelative("Multiplier");
        SerializedProperty additionalBuffProp = property.FindPropertyRelative("AdditionalBuff");

        Rect statRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };

        Rect formulaRect = new Rect(position) { yMin = statRect.yMax + 2f, height = 2f * EditorGUIUtility.singleLineHeight + 2f };

        Rect GoldValuesRect = new Rect(position) { yMin = formulaRect.yMax + 2f, height = EditorGUIUtility.singleLineHeight };

        EditorGUI.BeginChangeCheck();

        EditorGUI.PropertyField(statRect, statProperty);

        if (EditorGUI.EndChangeCheck())
        {
            if ((UnitStatsEnum)statProperty.enumValueIndex == UnitStatsEnum.Health || (UnitStatsEnum)statProperty.enumValueIndex == UnitStatsEnum.Shield)
            {
                baseBuffprop.FindPropertyRelative("isInt").boolValue = true;
                additionalBuffProp.FindPropertyRelative("isInt").boolValue = true;
            }
            else
            {
                baseBuffprop.FindPropertyRelative("isInt").boolValue = false;
                additionalBuffProp.FindPropertyRelative("isInt").boolValue = false;
            }
        }

        PropertyInLineDrawer.DrawPropertiesInText(formulaRect, property, "(^BaseBuff^ + Base Value) * ^Multiplier^ + ^AdditionalBuff^", new GUIContent("Total Buff"), "^"[0]);

        GoldValuesRect = EditorGUI.PrefixLabel(GoldValuesRect, new GUIContent("Buff gold values"));

        float width = (GoldValuesRect.width - 4f) / 3f;

        bool isInt = baseBuffprop.FindPropertyRelative("isInt").boolValue;

        object baseBuffValue;
        object additionalBuffValue;

        if (isInt)
        {
            baseBuffValue = baseBuffprop.FindPropertyRelative("intValue").intValue;
            additionalBuffValue = additionalBuffProp.FindPropertyRelative("intValue").intValue;
        }
        else
        {
            baseBuffValue = baseBuffprop.FindPropertyRelative("floatValue").floatValue;
            additionalBuffValue = additionalBuffProp.FindPropertyRelative("floatValue").floatValue;
        }
        try
        {
            Buff buff = new Buff((UnitStatsEnum)property.FindPropertyRelative("stat").enumValueIndex, multiplierProp.floatValue, baseBuffValue, additionalBuffValue);

            int totalValue = BuffValueCalculator.GetValue(buff, out int baseGoldValue, out int multiplierGoldValue, out int additionalGoldValue);

            EditorGUI.LabelField(new Rect(GoldValuesRect) { width = width }, $"{baseGoldValue}g");
            EditorGUI.LabelField(new Rect(GoldValuesRect) { xMin = GoldValuesRect.xMin + width + 2f, width = width }, $"{multiplierGoldValue}g");
            EditorGUI.LabelField(new Rect(GoldValuesRect) { xMin = GoldValuesRect.xMin + 2f * width + 4f, width = width }, $"{additionalGoldValue}g");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        
    }
}
