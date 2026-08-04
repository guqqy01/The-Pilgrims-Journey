using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

[CustomPropertyDrawer(typeof(ParticleSystem.MinMaxCurve))]
public class MinMaxCurveDrawer : PropertyDrawer
{
    // Keep foldout state per property path
    private static readonly Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

    private readonly float LineHeight = EditorGUIUtility.singleLineHeight;
    private const float VSpace = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Cache sub-properties
        var modeProp = property.FindPropertyRelative("m_Mode");
        var curveMinProp = property.FindPropertyRelative("m_CurveMin");
        var curveMaxProp = property.FindPropertyRelative("m_CurveMax");
        var constMinProp = property.FindPropertyRelative("m_ConstantMin");
        var constMaxProp = property.FindPropertyRelative("m_ConstantMax");
        var curveMultiplierProp = property.FindPropertyRelative("m_CurveMultiplier");

        EditorGUI.indentLevel++;
        float y = position.y + VSpace;
        float x = position.x;
        float buttonWidth = 25f;
        float w = position.width - buttonWidth;

        var buttonRect = new Rect() {xMin = position.xMax - buttonWidth, width = buttonWidth , yMin = position.yMin + VSpace, height = LineHeight};

        var mode = (ParticleSystemCurveMode)modeProp.enumValueIndex;

        if (EditorGUI.DropdownButton(buttonRect, new GUIContent(" "), FocusType.Passive))
        {
            GenericMenu genericMenu = new GenericMenu();

            foreach (string enumStr in Enum.GetNames(typeof(ParticleSystemCurveMode)))
            {
                if (!Enum.TryParse(enumStr, false, out ParticleSystemCurveMode enumValue))
                    continue;

                genericMenu.AddItem(new GUIContent(enumStr), mode == enumValue, () => SetEnum(modeProp, enumValue));
            }

            genericMenu.ShowAsContext();
        }

        var mulitplierRect = new Rect();

        // Select layout based on chosen mode
        
        switch (mode)
        {
            case ParticleSystemCurveMode.Constant:
                DrawConstantField(ref x, ref y, w, constMaxProp, "Constant");
                break;

            case ParticleSystemCurveMode.TwoConstants:
                DrawTwoConstants(ref x, ref y, w, constMinProp, constMaxProp);
                break;

            case ParticleSystemCurveMode.Curve:
                mulitplierRect = new Rect(x, y, w, LineHeight);

                EditorGUI.PropertyField(mulitplierRect, curveMultiplierProp);

                y += LineHeight + VSpace;
                DrawCurveField(ref x, ref y, w, curveMaxProp, Color.green, "Curve");
                break;

            case ParticleSystemCurveMode.TwoCurves:
                mulitplierRect = new Rect(x, y, w, LineHeight);

                EditorGUI.PropertyField(mulitplierRect, curveMultiplierProp);

                y += LineHeight + VSpace;
                DrawCurveField(ref x, ref y, w, curveMinProp, Color.red, "Min Curve");
                DrawCurveField(ref x, ref y, w, curveMaxProp, Color.green, "Max Curve");
                break;
        }

        EditorGUI.indentLevel--;
    }

    private void SetEnum(SerializedProperty modeProp, ParticleSystemCurveMode enumValue)
    {
        modeProp.enumValueIndex = (int)enumValue;
        SerializedObject obj = modeProp.serializedObject;
        obj.ApplyModifiedProperties();
        obj.UpdateIfRequiredOrScript();
    }


    /*public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
       // Cache sub-properties
        var modeProp = property.FindPropertyRelative("m_Mode");
        var curveMinProp = property.FindPropertyRelative("m_CurveMin");
        var curveMaxProp = property.FindPropertyRelative("m_CurveMax");
        var constMinProp = property.FindPropertyRelative("m_ConstantMin");
        var constMaxProp = property.FindPropertyRelative("m_ConstantMax");
        var curveMultiplierProp = property.FindPropertyRelative("m_CurveMultiplier");

        // Ensure a foldout state exists
        if (!foldoutStates.ContainsKey(property.propertyPath))
            foldoutStates[property.propertyPath] = false;

        // Draw foldout header
        var foldoutRect = new Rect(position.x, position.y, position.width, LineHeight);
        foldoutStates[property.propertyPath] =
            EditorGUI.Foldout(foldoutRect, foldoutStates[property.propertyPath], label, true);

        if (!foldoutStates[property.propertyPath])
            return;

        EditorGUI.indentLevel++;
        float y = position.y + LineHeight + VSpace;
        float x = position.x;
        float w = position.width;

        // Draw mode popup
        var modeRect = new Rect(x, y, w, LineHeight);
        EditorGUI.PropertyField(modeRect, modeProp);
        y += LineHeight + VSpace;

        var mulitplierRect = new Rect();

        // Select layout based on chosen mode
        var mode = (ParticleSystemCurveMode)modeProp.enumValueIndex;
        switch (mode)
        {
            case ParticleSystemCurveMode.Constant:
                DrawConstantField(ref x, ref y, w, constMaxProp, "Constant");
                break;

            case ParticleSystemCurveMode.TwoConstants:
                DrawTwoConstants(ref x, ref y, w, constMinProp, constMaxProp);
                break;

            case ParticleSystemCurveMode.Curve:
                mulitplierRect = new Rect(x, y, w, LineHeight);

                EditorGUI.PropertyField(mulitplierRect, curveMultiplierProp);

                y += LineHeight + VSpace;
                DrawCurveField(ref x, ref y, w, curveMaxProp, Color.green, "Curve");
                break;

            case ParticleSystemCurveMode.TwoCurves:
                mulitplierRect = new Rect(x, y, w, LineHeight);

                EditorGUI.PropertyField(mulitplierRect, curveMultiplierProp);

                y += LineHeight + VSpace;
                DrawCurveField(ref x, ref y, w, curveMinProp, Color.red, "Min Curve");
                DrawCurveField(ref x, ref y, w, curveMaxProp, Color.green, "Max Curve");
                break;
        }

        EditorGUI.indentLevel--;
    }*/

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Always 1 line for foldout + 1 for mode
        float height = LineHeight + VSpace;

        var modeProp = property.FindPropertyRelative("m_Mode");
        var mode = (ParticleSystemCurveMode)modeProp.enumValueIndex;

        switch (mode)
        {
            case ParticleSystemCurveMode.Constant:
                height += VSpace;
                break;

            case ParticleSystemCurveMode.TwoConstants:
                height += VSpace;
                break;

            case ParticleSystemCurveMode.Curve:
                height += (LineHeight * 3) + 2f * VSpace;
                break;

            case ParticleSystemCurveMode.TwoCurves:
                height += ((LineHeight * 3) + VSpace) * 2 + VSpace;
                break;
        }

        return height;
    }

    // Helpers

    private void DrawConstantField(ref float x, ref float y, float width,
                                   SerializedProperty constProp, string label)
    {
        var r = new Rect(x, y, width, LineHeight);
        EditorGUI.PropertyField(r, constProp, new GUIContent(label));
        y += LineHeight + VSpace;
    }

    private void DrawTwoConstants(ref float x, ref float y, float width,
                                  SerializedProperty minProp, SerializedProperty maxProp)
    {
        float half = width * 0.5f - 2;
        var minR = new Rect(x, y, half, LineHeight);
        var maxR = new Rect(x + half + 4, y, half, LineHeight);

        EditorGUI.PropertyField(minR, minProp, new GUIContent("Min"));
        EditorGUI.PropertyField(maxR, maxProp, new GUIContent("Max"));
        y += LineHeight + VSpace;
    }

    private void DrawCurveField(ref float x, ref float y, float width,
                                SerializedProperty curveProp, Color color, string label)
    {
        var r = new Rect(x, y, width, LineHeight * 3);
        curveProp.animationCurveValue = EditorGUI.CurveField(r, new GUIContent(label), curveProp.animationCurveValue);
        y += (LineHeight * 3) + VSpace;
    }
}
