using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DialogueSystem
{
    [CustomPropertyDrawer(typeof(DialogueEventListener))]
    public class DialogueEventListenerPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty eventTypeProperty = property.FindPropertyRelative("eventType");
            SerializedProperty dialogueProperty = property.FindPropertyRelative("dialogue");
            SerializedProperty promptProperty = property.FindPropertyRelative("prompt");
            SerializedProperty promptResponseIndexProperty = property.FindPropertyRelative("promptResponseIndex");
            SerializedProperty promptParagraphIndexProperty = property.FindPropertyRelative("promptParagraphIndex");
            SerializedProperty eventProperty = property.FindPropertyRelative("Event");

            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            eventTypeProperty.enumValueIndex = (int)(DialogueEventType)EditorGUILayout.EnumPopup(
                "Event Type",
                (DialogueEventType)eventTypeProperty.enumValueIndex
            );

            switch ((DialogueEventType)eventTypeProperty.enumValueIndex)
            {
                case DialogueEventType.StartDialogue:
                case DialogueEventType.EndDialogue:
                    EditorGUILayout.PropertyField(dialogueProperty);
                    EditorGUILayout.PropertyField(eventProperty);
                    break;

                case DialogueEventType.PromptParagraph:
                    EditorGUILayout.PropertyField(promptProperty);

                    if (promptProperty.objectReferenceValue is Prompt prompt
                        && prompt.paragraphs != null)
                    {
                        string[] paragraphs = prompt.paragraphs
                            .Select(x => x?.paragraph ?? "(null)")
                            .ToArray();

                        promptParagraphIndexProperty.intValue = EditorGUILayout.Popup(
                            "Paragraph",
                            promptParagraphIndexProperty.intValue,
                            paragraphs
                        );
                    }

                    EditorGUILayout.PropertyField(eventProperty);
                    break;

                case DialogueEventType.PromptResponse:
                    EditorGUILayout.PropertyField(promptProperty);

                    if (promptProperty.objectReferenceValue is Prompt prompt2
                        && prompt2.promptResponses != null)
                    {
                        string[] responses = prompt2.promptResponses
                            .Select(x => x?.response ?? "(null)")
                            .ToArray();

                        promptResponseIndexProperty.intValue = EditorGUILayout.Popup(
                            "Response",
                            promptResponseIndexProperty.intValue,
                            responses
                        );
                    }

                    EditorGUILayout.PropertyField(eventProperty);
                    break;
            }

            EditorGUI.EndProperty();
        }
    }
}
