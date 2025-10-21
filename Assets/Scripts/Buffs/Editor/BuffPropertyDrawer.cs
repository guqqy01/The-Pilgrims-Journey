using UnityEngine;
using UnityEditor;
using System;

/*[CustomPropertyDrawer(typeof(Buff))]
public class BuffPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 4f * EditorGUIUtility.singleLineHeight + 6f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty statProperty = property.FindPropertyRelative("stat");
        SerializedProperty baseBuffProp = property.FindPropertyRelative("baseBuff");
        SerializedProperty multiplierProp = property.FindPropertyRelative("Multiplier");
        SerializedProperty additionalBuffProp = property.FindPropertyRelative("additionalBuff");

        Rect statRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
        Rect formulaRect = new Rect(position) { yMin = statRect.yMax + 2f, height = 2f * EditorGUIUtility.singleLineHeight + 2f };
        Rect goldValuesRect = new Rect(position) { yMin = formulaRect.yMax + 2f, height = EditorGUIUtility.singleLineHeight };

        // Draw stat field
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(statRect, statProperty);
        if (EditorGUI.EndChangeCheck())
        {
            bool isIntStat = (UnitStatsEnum)statProperty.enumValueIndex is UnitStatsEnum.Health or UnitStatsEnum.Shield;
            baseBuffProp.FindPropertyRelative("isInt").boolValue = isIntStat;
            additionalBuffProp.FindPropertyRelative("isInt").boolValue = isIntStat;
        }

        // Formula preview
        PropertyInLineDrawer.DrawPropertiesInText(
            formulaRect,
            property,
            "(^baseBuff^ + Base Value) * ^Multiplier^ + ^additionalBuff^",
            new GUIContent("Total Buff"),
            '^'
        );

        // Gold values
        goldValuesRect = EditorGUI.PrefixLabel(goldValuesRect, new GUIContent("Buff gold values"));
        float width = (goldValuesRect.width - 4f) / 3f;

        try
        {
            bool isInt = baseBuffProp.FindPropertyRelative("isInt").boolValue;

            IntOrFloatObject baseBuffValue = isInt
                ? new IntOrFloatObject(baseBuffProp.FindPropertyRelative("intValue").intValue)
                : new IntOrFloatObject(baseBuffProp.FindPropertyRelative("floatValue").floatValue);

            IntOrFloatObject additionalBuffValue = isInt
                ? new IntOrFloatObject(additionalBuffProp.FindPropertyRelative("intValue").intValue)
                : new IntOrFloatObject(additionalBuffProp.FindPropertyRelative("floatValue").floatValue);

            Buff buff = new Buff(
                multiplierProp.floatValue,
                baseBuffValue,
                additionalBuffValue
            );
        }
        catch (Exception e)
        {
            EditorGUI.HelpBox(goldValuesRect, $"Error calculating buff: {e.Message}", MessageType.Error);
            Debug.LogError(e);
        }
    }
}*/