using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(Buff))]
public class BuffPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 4f * EditorGUIUtility.singleLineHeight + 6f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty multiplierProp = property.FindPropertyRelative("Multiplier");

        SerializedProperty baseBuffProp = property.FindPropertyRelative("baseBuff");
        SerializedProperty additionalBuffProp = property.FindPropertyRelative("additionalBuff");

        Rect foldoutRect = new Rect(position) { width = position.width / 2f };

        Rect dropDownRect = EditorGUI.PrefixLabel(position, new GUIContent(" "));

        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);

        bool isInt = baseBuffProp.FindPropertyRelative("isInt").boolValue;

        if(EditorGUI.DropdownButton(dropDownRect, new GUIContent(isInt ? "Int" : "Float"), FocusType.Passive))
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Int"), isInt, () => ChangeBuffToInt(property));
            menu.AddItem(new GUIContent("Float"), !isInt, () => ChangeBuffToFloat(property));

            menu.ShowAsContext();
        }

        EditorGUI.indentLevel++;

        if (property.isExpanded)
        {
            EditorGUILayout.PropertyField(multiplierProp);
            EditorGUILayout.PropertyField(baseBuffProp);
            EditorGUILayout.PropertyField(additionalBuffProp);
        }

        EditorGUI.indentLevel--;
    }

    private void ChangeBuffToInt(SerializedProperty property)
    {
        SerializedProperty baseBuffProp = property.FindPropertyRelative("baseBuff");
        SerializedProperty addBuffProp = property.FindPropertyRelative("additionalBuff");

        ConvertIntOrFloatToInt(baseBuffProp);
        ConvertIntOrFloatToInt(addBuffProp);

        property.serializedObject.ApplyModifiedProperties();
    }

    private void ChangeBuffToFloat(SerializedProperty property)
    {
        SerializedProperty baseBuffProp = property.FindPropertyRelative("baseBuff");
        SerializedProperty addBuffProp = property.FindPropertyRelative("additionalBuff");

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