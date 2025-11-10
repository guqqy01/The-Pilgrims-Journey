using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FloatRangeSlider))]
public class FloatRangeSliderDrawer : PropertyDrawer
{
    public const float limitRectWidth = 100f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty minValueProp = property.FindPropertyRelative("MinLimit");
        SerializedProperty maxValueProp = property.FindPropertyRelative("MaxLimit");
        SerializedProperty startProp = property.FindPropertyRelative("Start");
        SerializedProperty lengthProp = property.FindPropertyRelative("Length");

        float max = startProp.floatValue + lengthProp.floatValue;
        float min = startProp.floatValue;

        position = EditorGUI.PrefixLabel(position, label);

        Rect minLimitRect = new Rect(position) { width = limitRectWidth };
        Rect maxLimitRect = new Rect(position) { xMin = position.xMax - limitRectWidth };
        Rect sliderRect = new Rect(position) { xMin = minLimitRect.xMax + 2, xMax = maxLimitRect.xMin - 2 };

        EditorGUI.PropertyField(minLimitRect, minValueProp, GUIContent.none);
        EditorGUI.PropertyField(maxLimitRect, maxValueProp, GUIContent.none);

        EditorGUI.MinMaxSlider(position, GUIContent.none, ref min, ref max, minValueProp.floatValue, maxValueProp.floatValue);

        startProp.floatValue = min;
        lengthProp.floatValue = max - min;
    }
}


[CustomPropertyDrawer(typeof(IntRangeSlider))]
public class IntRangeSliderDrawer : PropertyDrawer
{
    public const float limitRectWidth = 100f;
    public const float smallSpacing = 4f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Two lines: values row + slider row
        return EditorGUIUtility.singleLineHeight * 2 + smallSpacing;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Backing field names
        SerializedProperty minLimitProp = property.FindPropertyRelative("minLimit");
        SerializedProperty maxLimitProp = property.FindPropertyRelative("maxLimit");
        SerializedProperty startProp = property.FindPropertyRelative("start");
        SerializedProperty lengthProp = property.FindPropertyRelative("length");

        if (minLimitProp == null || maxLimitProp == null || startProp == null || lengthProp == null)
        {
            EditorGUI.LabelField(position, label.text, "Missing backing fields");
            return;
        }

        property.serializedObject.Update();
        EditorGUI.BeginProperty(position, label, property);

        // Split into two rows
        Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect sliderRectFull = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + smallSpacing, position.width, EditorGUIUtility.singleLineHeight);

        
        // Draw prefix label on the first row
        Rect valuesRect = EditorGUI.PrefixLabel(labelRect, GUIUtility.GetControlID(FocusType.Passive), label);

        // Values row: Start and End int fields (normal IntFields)
        int start = startProp.intValue;
        int length = lengthProp.intValue;
        int end = start + length;

        // Layout for Start and End fields: two equal halves
        float halfWidth = (valuesRect.width - smallSpacing) * 0.5f;
        Rect startRect = new Rect(valuesRect.x, valuesRect.y, halfWidth, valuesRect.height);
        Rect endRect = new Rect(valuesRect.x + halfWidth + smallSpacing, valuesRect.y, halfWidth, valuesRect.height);

        EditorGUI.BeginChangeCheck();
        int newStart = EditorGUI.IntField(startRect, new GUIContent("Start"), start);
        int newEnd = EditorGUI.IntField(endRect, new GUIContent("End"), end);

        // Apply Start/End edits into start/length while preserving invariants
        if (EditorGUI.EndChangeCheck())
        {
            // If user changed both, accept them but clamp to limits later
            start = newStart;
            end = newEnd;

            // Ensure ordering
            if (end < start) end = start;

            // Temporarily set; will be clamped when we set serialized props below
            length = end - start;
        }

        // Slider row: Min / slider / Max
        Rect pos = sliderRectFull;
        Rect minLimitRect = new Rect(pos) { width = limitRectWidth };
        Rect maxLimitRect = new Rect(pos) { xMin = pos.xMax - limitRectWidth };
        Rect sliderRect = new Rect(pos) { xMin = minLimitRect.xMax + 4f, xMax = maxLimitRect.xMin - 4f };

        // Read current limits
        int minLimit = minLimitProp.intValue;
        int maxLimit = maxLimitProp.intValue;

        EditorGUI.BeginChangeCheck();
        // Draw simple int fields for limits
        minLimit = EditorGUI.IntField(minLimitRect, GUIContent.none, minLimit);
        maxLimit = EditorGUI.IntField(maxLimitRect, GUIContent.none, maxLimit);

        // Ensure min <= max
        if (maxLimit < minLimit)
        {
            // prefer to keep the field that the user last edited; fall back to clamping
            if (EditorGUI.EndChangeCheck())
            {
                // if min was just changed to be > max, push max up
                if (minLimitProp.intValue != minLimit)
                    maxLimit = minLimit;
                else
                    minLimit = maxLimit;
            }
            else
            {
                maxLimit = minLimit;
            }
        }

        // Convert current start/end for slider
        float fMin = (float)start;
        float fMax = (float)(start + length);

        // Clamp slider bounds to visible integer limits (avoid degenerate slider if equal)
        float sliderMin = (float)minLimit;
        float sliderMax = Mathf.Max((float)maxLimit, sliderMin);

        // Draw MinMaxSlider (float-based), show no labels (values shown above)
        EditorGUI.BeginChangeCheck();
        EditorGUI.MinMaxSlider(sliderRect, GUIContent.none, ref fMin, ref fMax, sliderMin, sliderMax);

        // If user interacted with slider, or changed limits/fields, update ints
        if (EditorGUI.EndChangeCheck() || minLimitProp.intValue != minLimit || maxLimitProp.intValue != maxLimit || newStart != startProp.intValue || (newStart + length) != (startProp.intValue + lengthProp.intValue))
        {
            // Round slider floats to ints
            int sliderStart = Mathf.RoundToInt(fMin);
            int sliderEnd = Mathf.RoundToInt(fMax);

            // Start/End priority: Changes to Start/End fields above override slider changes that conflict.
            // We already applied Start/End edits into start/length above when values row changed.
            // If the values row wasn't changed, accept slider values.
            bool valuesRowChanged = (newStart != startProp.intValue) || (newEnd != (startProp.intValue + lengthProp.intValue));

            if (valuesRowChanged)
            {
                // newStart and length were set earlier from the fields above
                start = Mathf.Clamp(newStart, minLimit, maxLimit);
                length = Mathf.Clamp(length, 0, maxLimit - start);
            }
            else
            {
                // Use slider values
                start = Mathf.Clamp(sliderStart, minLimit, maxLimit);
                sliderEnd = Mathf.Clamp(sliderEnd, minLimit, maxLimit);
                if (sliderEnd < start) sliderEnd = start;
                length = Mathf.Clamp(sliderEnd - start, 0, maxLimit - start);
            }

            // Apply limits and range to serialized properties
            minLimitProp.intValue = minLimit;
            maxLimitProp.intValue = maxLimit;
            startProp.intValue = start;
            lengthProp.intValue = length;
        }

        EditorGUI.EndProperty();
        property.serializedObject.ApplyModifiedProperties();
    }
}