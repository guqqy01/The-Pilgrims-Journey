using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using Units;
using Unity.VisualScripting;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(Buff))]
public class BuffPropertyDrawer : PropertyDrawer
{
    private static readonly Dictionary<Type, Func<int, bool>> EnumMappings =
    new Dictionary<Type, Func<int, bool>> {
        { typeof(UnitStatsEnum), index => index == (int)UnitStatsEnum.Health || index == (int)UnitStatsEnum.Shield },
        { typeof(StatTypes), index => index == (int)StatTypes.Health || index == (int)StatTypes.Shield || index == (int)StatTypes.Damage || index == (int)StatTypes.NumberOfAttacks }
    };


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        SerializedProperty buffTypeProp = property.FindPropertyRelative(nameof(Buff.buffFlags));


        BuffType buffType = (BuffType)buffTypeProp.enumValueFlag;

        float height = EditorGUIUtility.singleLineHeight; // foldout line
        if(buffType.HasFlag(BuffType.Multiplier))
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("multiplier")) + 2f;
        if (buffType.HasFlag(BuffType.Base))
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("baseBuff")) + 2f;
        if (buffType.HasFlag(BuffType.Additional))
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("additionalBuff")) + 2f;
        return height;
    }


    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty multiplierProp = property.FindPropertyRelative("multiplier");
        SerializedProperty baseBuffProp = property.FindPropertyRelative("baseBuff");
        SerializedProperty additionalBuffProp = property.FindPropertyRelative("additionalBuff");
        
        SerializedProperty buffTypeProp = property.FindPropertyRelative(nameof(Buff.buffFlags));

        Rect foldoutRect = new Rect(position) { width = position.width / 2f , height = EditorGUIUtility.singleLineHeight};
        Rect enumRect = new Rect(position) { xMin = foldoutRect.xMax, height = EditorGUIUtility.singleLineHeight };

        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);

        BuffType buffType = (BuffType)EditorGUI.EnumFlagsField(enumRect, (BuffType)buffTypeProp.enumValueFlag);
        buffTypeProp.enumValueFlag = (int)buffType;

        AutoSetBuffType(position, property);

        EditorGUI.indentLevel++;

        float yMin = foldoutRect.yMax + 2f;

        if (property.isExpanded)
        {
            if (buffType.HasFlag(BuffType.Multiplier))
            {
                Rect multiplierRect = new Rect(position) { yMin = yMin, height = EditorGUIUtility.singleLineHeight };
                EditorGUI.PropertyField(multiplierRect, multiplierProp);
                yMin += EditorGUIUtility.singleLineHeight + 2f;
            }
            if (buffType.HasFlag(BuffType.Base))
            {
                Rect baseRect = new Rect(position) { yMin = yMin, height = EditorGUIUtility.singleLineHeight };
                EditorGUI.PropertyField(baseRect, baseBuffProp);
                yMin += EditorGUIUtility.singleLineHeight + 2f;
            }
            if (buffType.HasFlag(BuffType.Additional))
            {
                Rect additionalRect = new Rect(position) { yMin = yMin, height = EditorGUIUtility.singleLineHeight };
                EditorGUI.PropertyField(additionalRect, additionalBuffProp);
            }
        }

        EditorGUI.indentLevel--;
    }

    private void DrawManualDropdown(Rect position, SerializedProperty property)
    {
        SerializedProperty baseBuffProp = property.FindPropertyRelative("baseBuff");

        Rect dropDownRect = EditorGUI.PrefixLabel(new Rect(position) { height = EditorGUIUtility.singleLineHeight }, new GUIContent(" "));

        bool isInt = baseBuffProp.FindPropertyRelative("isInt").boolValue;

        if (EditorGUI.DropdownButton(dropDownRect, new GUIContent(isInt ? "Int" : "Float"), FocusType.Passive))
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Int"), isInt, () => ChangeBuffToInt(property));
            menu.AddItem(new GUIContent("Float"), !isInt, () => ChangeBuffToFloat(property));

            menu.ShowAsContext();
        }
    }

    private void AutoSetBuffType(Rect position, SerializedProperty property)
    {
        if (TryGetKeyProperty(property, out SerializedProperty keyProp))
        {
            if(TryGetEnumType(keyProp, out Type enumType))
            {
                if (EnumMappings.TryGetValue(enumType, out var isIntFunc))
                {
                    if (isIntFunc(keyProp.enumValueIndex))
                        ChangeBuffToInt(property);
                    else
                        ChangeBuffToFloat(property);
                    return;
                }
            }
        }
        else if(TryGetBuffTypeAttribute(property, out BuffTypeAttribute buffTypeAttribute))
        {
            if (buffTypeAttribute.IsInt)
                ChangeBuffToInt(property);
            else 
                ChangeBuffToFloat(property);
            return;
        }

        DrawManualDropdown(position, property);
    }

    private bool TryGetBuffTypeAttribute(SerializedProperty property, out BuffTypeAttribute buffTypeAttribute)
    {
        buffTypeAttribute = fieldInfo.GetCustomAttribute<BuffTypeAttribute>(false);

        return buffTypeAttribute != null;
    }

    private bool TryGetKeyProperty(SerializedProperty valueProp, out SerializedProperty keyProp)
    {
        keyProp = null;

        if (valueProp.propertyPath.EndsWith(".Value"))
        {
            string keyPath = valueProp.propertyPath.Replace(".Value", ".Key");
            keyProp = valueProp.serializedObject.FindProperty(keyPath);
            if (keyProp != null)
                return true;
        }

        return false;
    }

    private bool TryGetEnumType(SerializedProperty keyProp, out Type enumType)
    {
        enumType = null;

        if (keyProp == null || keyProp.propertyType != SerializedPropertyType.Enum)
            return false;

        enumType = keyProp.GetUnderlyingType();

        return enumType != null;
    }


    private void ChangeBuffToInt(SerializedProperty property)
    {
        SerializedProperty baseBuffProp = property.FindPropertyRelative("baseBuff");
        SerializedProperty addBuffProp = property.FindPropertyRelative("additionalBuff");

        if (baseBuffProp.FindPropertyRelative("isInt").boolValue && addBuffProp.FindPropertyRelative("isInt").boolValue)
            return;

        ConvertIntOrFloatToInt(baseBuffProp);
        ConvertIntOrFloatToInt(addBuffProp);

        property.serializedObject.ApplyModifiedProperties();
    }

    private void ChangeBuffToFloat(SerializedProperty property)
    {
        SerializedProperty baseBuffProp = property.FindPropertyRelative("baseBuff");
        SerializedProperty addBuffProp = property.FindPropertyRelative("additionalBuff");

        if (!baseBuffProp.FindPropertyRelative("isInt").boolValue && !addBuffProp.FindPropertyRelative("isInt").boolValue)
            return;

        ConvertIntOrFloatToFloat(baseBuffProp);
        ConvertIntOrFloatToFloat(addBuffProp);

        property.serializedObject.ApplyModifiedProperties();
    }

    private void ConvertIntOrFloatToInt(SerializedProperty prop)
    {
        var intValue = prop.FindPropertyRelative("intValue");
        var floatValue = prop.FindPropertyRelative("floatValue");
        var isIntProp = prop.FindPropertyRelative("isInt");

        intValue.intValue = Mathf.RoundToInt(floatValue.floatValue);
        isIntProp.boolValue = true;
    }

    private void ConvertIntOrFloatToFloat(SerializedProperty prop)
    {
        var intValue = prop.FindPropertyRelative("intValue");
        var floatValue = prop.FindPropertyRelative("floatValue");
        var isIntProp = prop.FindPropertyRelative("isInt");

        floatValue.floatValue = intValue.intValue;
        isIntProp.boolValue = false;
    }
}