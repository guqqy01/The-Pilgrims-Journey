using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class PropertyInLineDrawer
{
    private static readonly TextGenerationSettings textGenerationSettings = new TextGenerationSettings
    {
        textAnchor = TextAnchor.UpperLeft,
        font = EditorStyles.label.font,
        fontSize = 14, // Set the font size
        color = Color.black,
        richText = false,
        lineSpacing = 1,
        generateOutOfBounds = true
    };


    public static void DrawPropertiesInText(Rect rect, SerializedProperty property, string text, GUIContent label, char propertySeperationCharacter)
    {
        rect = EditorGUI.PrefixLabel(rect, label);

        List<StringInfo> stringInfos = new List<StringInfo>();

        string currentStirng = text;

        int breaker = 1000;

        while (currentStirng.Length > 0 && breaker > 0)
        {
            int openCharIndex = currentStirng.IndexOf(propertySeperationCharacter);

            string labelString = currentStirng.Substring(0, openCharIndex);

            if (labelString.Length > 0)
                stringInfos.Add(new StringInfo(labelString, false, textGenerationSettings));

            string holderString = currentStirng.Substring(openCharIndex + 1);
            int endCharIndex = holderString.IndexOf(propertySeperationCharacter);

            string propertyString = holderString.Substring(0, endCharIndex);

            if (propertyString.Length > 0)
                stringInfos.Add(new StringInfo(propertyString, true, textGenerationSettings));

            currentStirng = holderString.Substring(endCharIndex + 1);

            breaker--;
        }

        float totalCharactersSize = 0f;
        float numberOfProperties = 0f;

        foreach (StringInfo info in stringInfos)
        {
            if (info.isProperty)
                numberOfProperties++;

            totalCharactersSize += info.stringTotalSize;
        }

        float propertyRectWidth = (rect.width - totalCharactersSize) / numberOfProperties;

        float xMin = rect.xMin;

        Rect upperRect = new Rect(rect) { yMin = rect.yMin, height = EditorGUIUtility.singleLineHeight };
        Rect lowerRect = new Rect(rect) { yMin = upperRect.yMax + 2f, height = EditorGUIUtility.singleLineHeight };

        for (int i = 0; i < stringInfos.Count; i++)
        {
            float width = stringInfos[i].isProperty ? propertyRectWidth : stringInfos[i].stringTotalSize;

            Rect upperPosition = new Rect(upperRect) { xMin = xMin, width = width };
            Rect lowerPosition = new Rect(lowerRect) { xMin = xMin, width = width };

            if (stringInfos[i].isProperty)
            {
                SerializedProperty relativeProperty = property.FindPropertyRelative(stringInfos[i].str);
                EditorGUI.LabelField(upperPosition, stringInfos[i].str);
                EditorGUI.PropertyField(lowerPosition, relativeProperty, GUIContent.none);
            }
            else
            {
                EditorGUI.LabelField(new Rect(rect) { xMin = xMin, width = width }, stringInfos[i].str);
            }

            xMin += width;
        }
    }

    private struct StringInfo
    {
        public readonly string str;
        public readonly bool isProperty;
        public readonly float stringTotalSize;


        public StringInfo(string str, bool isProperty, TextGenerationSettings textGenerationSettings)
        {
            this.str = str;
            this.isProperty = isProperty;
            stringTotalSize = 0;

            if (isProperty)
                return;

            TextGenerator textGenerator = new TextGenerator();
            textGenerator.Invalidate();
            if (textGenerator.Populate(str, textGenerationSettings))
            {
                foreach (UICharInfo characterInfo in textGenerator.characters)
                {
                    stringTotalSize += characterInfo.charWidth;
                }
                stringTotalSize += 2;
            }
        }
    }
}