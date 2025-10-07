using UnityEditor;
using System.Linq;
using UnityEngine;


namespace DialogueSystem
{
    [CustomPropertyDrawer(typeof(PromptResponseConditionSender))]
    public class PromptResponseConditionSenderPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            SerializedProperty promptProp = property.FindPropertyRelative("prompt");
            SerializedProperty responseIndexProp = property.FindPropertyRelative("responseIndex");
            SerializedProperty boolFunctionReferenceProp = property.FindPropertyRelative("boolFunctionReference");

            // Layout control
            float lineHeight = EditorGUIUtility.singleLineHeight + 2;
            Rect line = new Rect(position.x, position.y, position.width, lineHeight);

            // Draw foldout
            property.isExpanded = EditorGUI.Foldout(line, property.isExpanded, label, true);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                line.y += lineHeight;

                // Draw Prompt field
                EditorGUI.PropertyField(line, promptProp);
                line.y += lineHeight;

                Prompt prompt = promptProp.objectReferenceValue as Prompt;
                if (prompt != null && prompt.promptResponses != null)
                {
                    string[] responses = prompt.promptResponses
                        .Select(x => x?.response ?? "(null)")
                        .ToArray();

                    int currentIndex = responseIndexProp.intValue;
                    currentIndex = EditorGUI.Popup(line, "Response", currentIndex, responses);
                    responseIndexProp.intValue = currentIndex;
                    line.y += lineHeight;
                }

                // Draw BoolFunctionReference
                EditorGUI.PropertyField(line, boolFunctionReferenceProp);
                line.y += EditorGUI.GetPropertyHeight(boolFunctionReferenceProp);

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight + 4;

            SerializedProperty boolFunctionReferenceProp = property.FindPropertyRelative("boolFunctionReference");
            float baseHeight = EditorGUIUtility.singleLineHeight * 3 + 10; // prompt + popup + foldout
            float boolFuncHeight = EditorGUI.GetPropertyHeight(boolFunctionReferenceProp);
            return baseHeight + boolFuncHeight;
        }
    }
}
