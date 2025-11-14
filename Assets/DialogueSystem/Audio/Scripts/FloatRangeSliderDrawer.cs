using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(FloatRangeSlider))]
public class FloatRangeSliderDrawer : PropertyDrawer
{
    public const float limitRectWidth = 100f;
    public const float smallSpacing = 4f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // collapsed → 1 line
        // expanded → 3 lines (Start/End row + slider row + limits row)
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight * 2f + smallSpacing;

        return EditorGUIUtility.singleLineHeight * 4f + smallSpacing * 3;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty minLimitProp = property.FindPropertyRelative("minLimit");
        SerializedProperty maxLimitProp = property.FindPropertyRelative("maxLimit");
        SerializedProperty startProp = property.FindPropertyRelative("start");
        SerializedProperty lengthProp = property.FindPropertyRelative("length");

        if (minLimitProp == null || maxLimitProp == null || startProp == null || lengthProp == null)
        {
            EditorGUI.LabelField(position, label.text, "Missing fields");
            return;
        }

        //property.serializedObject.Update();
        //EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;

        float start = startProp.floatValue;
        float length = lengthProp.floatValue;
        float end = start + length;

        float minLimit = minLimitProp.floatValue;
        float maxLimit = maxLimitProp.floatValue;

        // -----------------------------
        // Foldout Header
        // -----------------------------
        Rect foldoutRect = new Rect(position.x, position.y, position.width, lineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, $"{label}         Start: {start}, End: {end}", true);

        float y = position.y + lineHeight + smallSpacing;

        // ============================================================
        //  COLLAPSED MODE → only slider row is drawn
        //  EXPANDED MODE → slider still drawn last
        // ============================================================

        //
        // 3) Slider Row
        //

        float minMaxLimitLabelWidth = 60f;

        Rect sliderRect = new Rect() { xMin = position.xMin + minMaxLimitLabelWidth, xMax = position.xMax - minMaxLimitLabelWidth, yMin = y, height = lineHeight };

        // Convert to floats for slider
        float fMin = start;
        float fMax = start + length;

        float sliderMin = minLimit;
        float sliderMax = Mathf.Max(maxLimit, sliderMin);

        Rect minLimitLabelRect = new Rect() { xMin = position.xMin, width = minMaxLimitLabelWidth, yMin = y, height = lineHeight };
        Rect maxLimitLabelRect = new Rect() { xMin = position.xMax - minMaxLimitLabelWidth, width = minMaxLimitLabelWidth, yMin = y, height = lineHeight };

        GUIStyle minStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight };
        GUIStyle maxStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft };

        EditorGUI.LabelField(minLimitLabelRect, new GUIContent(minLimit.ToString() + "|"), minStyle);
        EditorGUI.LabelField(maxLimitLabelRect, new GUIContent("|" + maxLimit.ToString()), maxStyle);

        EditorGUI.BeginChangeCheck();
        EditorGUI.MinMaxSlider(sliderRect, GUIContent.none, ref fMin, ref fMax, sliderMin, sliderMax);
        bool sliderChanged = EditorGUI.EndChangeCheck();

        if (sliderChanged)
        {
            float newSliderStart = fMin;
            float newSliderEnd = fMax;

            if (newSliderEnd < newSliderStart) newSliderEnd = newSliderStart;

            start = Mathf.Clamp(newSliderStart, minLimit, maxLimit);
            length = Mathf.Clamp(newSliderEnd - start, 0f, maxLimit - start);
            end = length + start;
        }

        y += lineHeight + smallSpacing;

        // ============================================================
        //   EXPANDED MODE UI
        // ============================================================
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            //
            // 1) Start / End row
            //
            Rect valuesRect = new Rect(position.x, y, position.width, lineHeight);

            float halfWidth = (valuesRect.width - smallSpacing) * 0.5f;

            Rect startRect = new Rect(valuesRect.x, valuesRect.y, halfWidth, valuesRect.height);
            Rect endRect = new Rect(valuesRect.x + halfWidth + smallSpacing, valuesRect.y, halfWidth, valuesRect.height);

            EditorGUI.BeginChangeCheck();

            start = EditorGUI.FloatField(startRect, "Start", start);

            if (EditorGUI.EndChangeCheck())
            {
                if (start > end)
                {
                    end = start;
                }

                end = Mathf.Clamp(end, minLimit, maxLimit);
                start = Mathf.Clamp(start, minLimit, maxLimit);
            }

            EditorGUI.BeginChangeCheck();

            end = EditorGUI.FloatField(endRect, "End", end);

            if (EditorGUI.EndChangeCheck())
            {
                if (end < start)
                {
                    start = end;
                }

                end = Mathf.Clamp(end, minLimit, maxLimit);
                start = Mathf.Clamp(start, minLimit, maxLimit);
            }

            y += lineHeight + smallSpacing;

            //
            // 2) MinLimit / MaxLimit row
            //
            Rect limitRow = new Rect(position.x, y, position.width, lineHeight);

            float halfLimitWidth = (limitRow.width - smallSpacing) * 0.5f;

            Rect minRect = new Rect(limitRow.x, limitRow.y, halfLimitWidth, limitRow.height);
            Rect maxRect = new Rect(limitRow.x + halfLimitWidth + smallSpacing, limitRow.y, halfLimitWidth, limitRow.height);

            EditorGUI.BeginChangeCheck();

            minLimit = EditorGUI.FloatField(minRect, "Min Limit", minLimit);

            if (EditorGUI.EndChangeCheck())
            {
                if (minLimit > maxLimit)
                    maxLimit = minLimit;

                end = Mathf.Clamp(end, minLimit, maxLimit);
                start = Mathf.Clamp(start, minLimit, maxLimit);
            }

            EditorGUI.BeginChangeCheck();

            maxLimit = EditorGUI.FloatField(maxRect, "Max Limit", maxLimit);

            if (EditorGUI.EndChangeCheck())
            {
                if (maxLimit < minLimit)
                    minLimit = maxLimit;

                end = Mathf.Clamp(end, minLimit, maxLimit);
                start = Mathf.Clamp(start, minLimit, maxLimit);
            }

            length = end - start;

            EditorGUI.indentLevel--;
        }

        // ============================================================
        //  APPLY VALUES
        // ============================================================
        minLimitProp.floatValue = minLimit;
        maxLimitProp.floatValue = maxLimit;
        startProp.floatValue = start;
        lengthProp.floatValue = length;

        //EditorGUI.EndProperty();
        //property.serializedObject.ApplyModifiedProperties();
    }
}
