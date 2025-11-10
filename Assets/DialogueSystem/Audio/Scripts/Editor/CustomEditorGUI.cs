using UnityEditor;
using UnityEngine;

public class CustomEditorGUI
{
    public static void DrawRangeSlider(FloatRangeSlider rangeSlider, Rect position, GUIContent label)
    {
        float max = rangeSlider.End;
        float min = rangeSlider.Start;

        EditorGUI.MinMaxSlider(position, label, ref min, ref max, rangeSlider.MinLimit, rangeSlider.MaxLimit);

        rangeSlider.Start = min;
        rangeSlider.End = max;
    }
}

