using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Units;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Buff))]
public class BuffPropertyDrawer : PropertyDrawer
{
    private const float enumLengthWhenOneValue = 50f;

    private const float Spacing = 2f;
    private static readonly Dictionary<Type, Func<int, bool>> EnumMappings =
    new Dictionary<Type, Func<int, bool>>
{
    {
        typeof(StatTypes), value =>
            value == (int)StatTypes.Health ||
            value == (int)StatTypes.Shield ||
            value == (int)StatTypes.Damage ||
            value == (int)StatTypes.NumberOfAttacks
    },
    {
        typeof(UnitStatsEnum), value =>
            value == (int)UnitStatsEnum.Health ||
            value == (int)UnitStatsEnum.Shield
    }
};


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        float height = line; // header row

        SerializedProperty buffTypeProp = property.FindPropertyRelative(nameof(Buff.buffFlags));
        BuffType buffType = (BuffType)buffTypeProp.enumValueFlag;
        bool hasJustOneFlag = HasOneFlag(buffType);

        // Body is only drawn when expanded, or always for the single-flag inline layout
        if (!property.isExpanded && !hasJustOneFlag)
            return height;

        if (RequiresManualToggle(property))
            height += line + Spacing;

        if (hasJustOneFlag)
            return height; // value is drawn inline on the header row

        if (buffType.HasFlag(BuffType.Multiplier))
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(Buff.NameOfMultiplier)) + Spacing;
        if (buffType.HasFlag(BuffType.Base))
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(Buff.NameOfBase)) + Spacing;
        if (buffType.HasFlag(BuffType.Additional))
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(Buff.NameOfAdditional)) + Spacing;

        return height;
    }


    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;

        SerializedProperty multiplierProp = property.FindPropertyRelative(Buff.NameOfMultiplier);
        SerializedProperty baseBuffProp = property.FindPropertyRelative(Buff.NameOfBase);
        SerializedProperty additionalBuffProp = property.FindPropertyRelative(Buff.NameOfAdditional);
        SerializedProperty buffTypeProp = property.FindPropertyRelative(nameof(Buff.buffFlags));

        BuffType previousBuffType = (BuffType)buffTypeProp.enumValueFlag;
        bool hasJustOneFlag = HasOneFlag(previousBuffType);
        bool needsManualToggle = RequiresManualToggle(property);

        Rect foldoutRect = new Rect(position) { width = position.width / 2f, height = line };
        Rect ifOneValueRect = new Rect(position)
        {
            xMin = foldoutRect.xMax,
            xMax = hasJustOneFlag ? position.xMax - enumLengthWhenOneValue : foldoutRect.xMax,
            height = line
        };
        Rect enumRect = new Rect(position) { xMin = ifOneValueRect.xMax + (hasJustOneFlag ? 2f : 0f), height = line };

        // Draw the flags field FIRST so its control ID stays stable when the layout below changes.
        // Indent must be 0 here, otherwise the field shifts inside the narrow rect.
        int oldIndent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        EditorGUI.BeginChangeCheck();
        BuffType buffType = (BuffType)EditorGUI.EnumFlagsField(enumRect, previousBuffType);
        if (EditorGUI.EndChangeCheck())
            buffTypeProp.enumValueFlag = (int)buffType;

        EditorGUI.indentLevel = oldIndent;

        // Label / foldout after it
        if (hasJustOneFlag)
            EditorGUI.PrefixLabel(foldoutRect, label);
        else
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);

        if (!needsManualToggle)
            ApplyAutoIntMode(property);

        if (!property.isExpanded && !hasJustOneFlag)
            return;

        EditorGUI.indentLevel++;

        float y = foldoutRect.yMax + Spacing;

        // Int/Float toggle gets its own row below the header
        if (needsManualToggle)
        {
            Rect toggleRect = new Rect(position) { yMin = y, height = line };
            DrawManualDropdown(toggleRect, property);
            y += line + Spacing;
        }

        if (buffType.HasFlag(BuffType.Multiplier))
            y = DrawValue(position, y, ifOneValueRect, hasJustOneFlag, multiplierProp);
        if (buffType.HasFlag(BuffType.Base))
            y = DrawValue(position, y, ifOneValueRect, hasJustOneFlag, baseBuffProp);
        if (buffType.HasFlag(BuffType.Additional))
            y = DrawValue(position, y, ifOneValueRect, hasJustOneFlag, additionalBuffProp);

        EditorGUI.indentLevel--;
    }

    private bool TryGetAutoIsInt(SerializedProperty property, out bool isInt)
    {
        isInt = false;

        if (TryGetKeyProperty(property, out SerializedProperty keyProp))
        {
            if (keyProp.propertyType == SerializedPropertyType.Enum
                && TryGetKeyEnumType(out Type enumType)
                && EnumMappings.TryGetValue(enumType, out var isIntFunc))
            {
                isInt = isIntFunc(GetEnumValue(keyProp));
                return true;
            }
            return false; // dictionary key we don't have a rule for -> manual dropdown
        }

        if (TryGetBuffTypeAttribute(property, out BuffTypeAttribute attr))
        {
            if (string.IsNullOrEmpty(attr.MemberName))
            {
                isInt = attr.IsInt;
                return true;
            }

            return TryResolveBool(property, attr.MemberName, out isInt);
        }

        return false;
    }


    private const BindingFlags AnyInstance =
    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static bool TryResolveBool(SerializedProperty property, string memberName, out bool value)
    {
        value = false;

        object owner = GetParentObject(property);
        if (owner == null)
            return false;

        // Search the runtime type first so overrides in subclasses are found,
        // then walk up the hierarchy for private members declared in base classes.
        for (Type t = owner.GetType(); t != null; t = t.BaseType)
        {
            PropertyInfo prop = t.GetProperty(memberName, AnyInstance | BindingFlags.DeclaredOnly);
            if (prop != null && prop.PropertyType == typeof(bool))
            {
                value = (bool)prop.GetValue(owner);
                return true;
            }

            FieldInfo field = t.GetField(memberName, AnyInstance | BindingFlags.DeclaredOnly);
            if (field != null && field.FieldType == typeof(bool))
            {
                value = (bool)field.GetValue(owner);
                return true;
            }

            MethodInfo method = t.GetMethod(memberName, AnyInstance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
            if (method != null && method.ReturnType == typeof(bool))
            {
                value = (bool)method.Invoke(owner, null);
                return true;
            }
        }

        return false;
    }

    /// Walks the property path to find the object that owns the Buff field.
    private static object GetParentObject(SerializedProperty property)
    {
        object obj = property.serializedObject.targetObject;
        string[] parts = property.propertyPath.Replace(".Array.data[", "[").Split('.');

        for (int i = 0; i < parts.Length - 1; i++)
        {
            string part = parts[i];
            int bracket = part.IndexOf('[');

            if (bracket >= 0)
            {
                string name = part.Substring(0, bracket);
                int index = int.Parse(part.Substring(bracket + 1, part.Length - bracket - 2));
                obj = (GetFieldValue(obj, name) as IList)?[index];
            }
            else
            {
                obj = GetFieldValue(obj, part);
            }

            if (obj == null)
                return null;
        }

        return obj;
    }

    private static object GetFieldValue(object obj, string name)
    {
        for (Type t = obj?.GetType(); t != null; t = t.BaseType)
        {
            FieldInfo field = t.GetField(name, AnyInstance | BindingFlags.DeclaredOnly);
            if (field != null)
                return field.GetValue(obj);
        }
        return null;
    }
    private bool RequiresManualToggle(SerializedProperty property)
        => !TryGetAutoIsInt(property, out _);

    private void ApplyAutoIntMode(SerializedProperty property)
    {
        if (!TryGetAutoIsInt(property, out bool isInt))
            return;

        if (isInt) ChangeBuffToInt(property);
        else ChangeBuffToFloat(property);
    }

    /// The drawer's fieldInfo is the "Value" field of SerializedKeyValuePair<TKey, TValue>,
    /// so the key's enum type can be read from the sibling "Key" field or the generic arguments.
    private bool TryGetKeyEnumType(out Type enumType)
    {
        enumType = null;

        Type pairType = fieldInfo?.DeclaringType;
        if (pairType == null)
            return false;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        Type keyType = pairType.GetField("Key", flags)?.FieldType;

        if (keyType == null && pairType.IsGenericType)
            keyType = pairType.GetGenericArguments()[0];

        if (keyType == null || !keyType.IsEnum)
            return false;

        enumType = keyType;
        return true;
    }
    private static int GetEnumValue(SerializedProperty keyProp)
    => keyProp.intValue;

    private float DrawValue(Rect position, float y, Rect inlineRect, bool inline, SerializedProperty prop)
    {
        if (inline)
        {
            EditorGUI.PropertyField(inlineRect, prop, GUIContent.none);
            return y;
        }

        float h = EditorGUI.GetPropertyHeight(prop);
        EditorGUI.PropertyField(new Rect(position) { yMin = y, height = h }, prop);
        return y + h + Spacing;
    }

    private bool HasOneFlag(BuffType buffType)
    {
        bool multi = buffType.HasFlag(BuffType.Multiplier);
        bool bas = buffType.HasFlag(BuffType.Base);
        bool addit = buffType.HasFlag(BuffType.Additional);

        bool justMulti = multi && !bas && !addit;
        bool justBase = bas && !addit && !multi;
        bool justAdditional = addit && !bas && !multi;

        return justMulti || justBase || justAdditional;
    }

    private void DrawManualDropdown(Rect rect, SerializedProperty property)
    {
        SerializedProperty baseBuffProp = property.FindPropertyRelative("baseBuff");
        bool isInt = baseBuffProp.FindPropertyRelative("isInt").boolValue;

        Rect dropDownRect = EditorGUI.PrefixLabel(rect, new GUIContent("Value Type"));

        if (EditorGUI.DropdownButton(dropDownRect, new GUIContent(isInt ? "Int" : "Float"), FocusType.Passive))
        {
            GenericMenu menu = new GenericMenu();
            SerializedObject so = property.serializedObject;
            string path = property.propertyPath;

            menu.AddItem(new GUIContent("Int"), isInt, () =>
            {
                so.Update();
                ChangeBuffToInt(so.FindProperty(path));
            });
            menu.AddItem(new GUIContent("Float"), !isInt, () =>
            {
                so.Update();
                ChangeBuffToFloat(so.FindProperty(path));
            });
            menu.ShowAsContext();
        }
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
        var isIntProp = prop.FindPropertyRelative("isInt");
        if (isIntProp.boolValue) return;

        prop.FindPropertyRelative("intValue").intValue =
            Mathf.RoundToInt(prop.FindPropertyRelative("floatValue").floatValue);
        isIntProp.boolValue = true;
    }

    private void ConvertIntOrFloatToFloat(SerializedProperty prop)
    {
        var isIntProp = prop.FindPropertyRelative("isInt");
        if (!isIntProp.boolValue) return;

        prop.FindPropertyRelative("floatValue").floatValue =
            prop.FindPropertyRelative("intValue").intValue;
        isIntProp.boolValue = false;
    }
}