using UnityEditor;
using UnityEngine;

public class CustomEditorGUI
{
    public static void DrawRangeSlider(RangeSlider rangeSlider, Rect position, GUIContent label)
    {
        float max = rangeSlider.MaxValue;
        float min = rangeSlider.MinValue;

        EditorGUI.MinMaxSlider(position, label, ref min, ref max, rangeSlider.MinLimit, rangeSlider.MaxLimit);

        rangeSlider.MinValue = min;
        rangeSlider.MaxValue = max;
    }
}

