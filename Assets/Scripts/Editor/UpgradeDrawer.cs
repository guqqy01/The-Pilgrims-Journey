using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Upgrade))]
public class UpgradeDrawer : PropertyDrawer
{
    public const float inbetweenWidth = 20f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty statProp = property.FindPropertyRelative("stat");

        float height = EditorGUIUtility.singleLineHeight;

        if (statProp.isExpanded)
            height += EditorGUIUtility.singleLineHeight + 2;

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Rect titleRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight};

        SerializedProperty statProp = property.FindPropertyRelative("stat");
        SerializedProperty typeProp = property.FindPropertyRelative("buffType");

        SerializedProperty intValuesProp = property.FindPropertyRelative("intValues");
        SerializedProperty floatValuesProp = property.FindPropertyRelative("floatValues");

        statProp.isExpanded = EditorGUI.Foldout(titleRect, statProp.isExpanded, "");

        string statName = statProp.enumDisplayNames[statProp.enumValueIndex];
        string buffTypeName = typeProp.enumDisplayNames[typeProp.enumValueIndex];

        titleRect = EditorGUI.PrefixLabel(titleRect, new GUIContent(statName + " " + buffTypeName));

        Rect statPosition = new Rect(titleRect) { xMax = titleRect.center.x - 1};
        Rect typePosition = new Rect(titleRect) { xMin = titleRect.center.x + 1};

        if (statProp.isExpanded)
        {
            EditorGUI.PropertyField(statPosition, statProp, GUIContent.none);
            EditorGUI.PropertyField(typePosition, typeProp, GUIContent.none);
        }
        
        Rect valuesRect = new Rect(position) {yMin = position.yMin + EditorGUIUtility.singleLineHeight + 2, height = EditorGUIUtility.singleLineHeight };

        if (statProp.isExpanded)
        {
            EditorGUI.indentLevel++;
            valuesRect = EditorGUI.PrefixLabel(valuesRect, new GUIContent("Values"));
            EditorGUI.indentLevel--;
        }

        float xMin = valuesRect.xMin;

        float width = (valuesRect.width - (inbetweenWidth * 4)) / 5;

        Rect value1 = new Rect(valuesRect) { xMin = xMin, width = width };
        xMin += width;
        Rect inBetween1 = new Rect(valuesRect) { xMin = xMin, width = inbetweenWidth};
        xMin += inbetweenWidth;
        Rect value2 = new Rect(valuesRect) { xMin = xMin, width = width };
        xMin += width;
        Rect inBetween2 = new Rect(valuesRect) { xMin = xMin, width = inbetweenWidth };
        xMin += inbetweenWidth;
        Rect value3 = new Rect(valuesRect) { xMin = xMin, width = width };
        xMin += width;
        Rect inBetween3 = new Rect(valuesRect) { xMin = xMin, width = inbetweenWidth };
        xMin += inbetweenWidth;
        Rect value4 = new Rect(valuesRect) { xMin = xMin, width = width };
        xMin += width;
        Rect inBetween4 = new Rect(valuesRect) { xMin = xMin, width = inbetweenWidth };
        xMin += inbetweenWidth;
        Rect value5 = new Rect(valuesRect) { xMin = xMin, width = width };

        if (statProp.isExpanded)
        {
            if((statName == UnitStatsEnum.Health.ToString() && buffTypeName != BuffType.Multiplier.ToString()) ||
                statName == UnitStatsEnum.Shield.ToString() && buffTypeName != BuffType.Multiplier.ToString())
            {
                intValuesProp.arraySize = 5;

                intValuesProp.GetArrayElementAtIndex(0).intValue = EditorGUI.IntField(value1, intValuesProp.GetArrayElementAtIndex(0).intValue);
                EditorGUI.LabelField(inBetween1, "->", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });
                intValuesProp.GetArrayElementAtIndex(1).intValue = EditorGUI.IntField(value2, intValuesProp.GetArrayElementAtIndex(1).intValue);
                EditorGUI.LabelField(inBetween2, "->", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });
                intValuesProp.GetArrayElementAtIndex(2).intValue = EditorGUI.IntField(value3, intValuesProp.GetArrayElementAtIndex(2).intValue);
                EditorGUI.LabelField(inBetween3, "->", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });
                intValuesProp.GetArrayElementAtIndex(3).intValue = EditorGUI.IntField(value4, intValuesProp.GetArrayElementAtIndex(3).intValue);
                EditorGUI.LabelField(inBetween4, "->", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });
                intValuesProp.GetArrayElementAtIndex(4).intValue = EditorGUI.IntField(value5, intValuesProp.GetArrayElementAtIndex(4).intValue);
            }
            else
            {
                floatValuesProp.arraySize = 5;

                floatValuesProp.GetArrayElementAtIndex(0).floatValue = EditorGUI.FloatField(value1, floatValuesProp.GetArrayElementAtIndex(0).floatValue);
                EditorGUI.LabelField(inBetween1, "->", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });
                floatValuesProp.GetArrayElementAtIndex(1).floatValue = EditorGUI.FloatField(value2, floatValuesProp.GetArrayElementAtIndex(1).floatValue);
                EditorGUI.LabelField(inBetween2, "->", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });
                floatValuesProp.GetArrayElementAtIndex(2).floatValue = EditorGUI.FloatField(value3, floatValuesProp.GetArrayElementAtIndex(2).floatValue);
                EditorGUI.LabelField(inBetween3, "->", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });
                floatValuesProp.GetArrayElementAtIndex(3).floatValue = EditorGUI.FloatField(value4, floatValuesProp.GetArrayElementAtIndex(3).floatValue);
                EditorGUI.LabelField(inBetween4, "->", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });
                floatValuesProp.GetArrayElementAtIndex(4).floatValue = EditorGUI.FloatField(value5, floatValuesProp.GetArrayElementAtIndex(4).floatValue);
            }
        }
    }
}
