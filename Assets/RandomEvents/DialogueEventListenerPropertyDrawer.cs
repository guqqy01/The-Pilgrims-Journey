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
            SerializedObject serializedObject = property.serializedObject;

            serializedObject.Update();

            SerializedProperty eventTypeProperty = property.FindPropertyRelative("eventType");

            SerializedProperty dialogueProperty = property.FindPropertyRelative("dialogue");

            SerializedProperty promptProperty = property.FindPropertyRelative("prompt");

            SerializedProperty promptResponseIndexProperty = property.FindPropertyRelative("promptResponseIndex");
            SerializedProperty promptParagraphIndexProperty = property.FindPropertyRelative("promptParagraphIndex");

            SerializedProperty EventProperty = property.FindPropertyRelative("Event");

            eventTypeProperty.enumValueIndex = (int)(DialogueEventType)EditorGUILayout.EnumPopup("Event Type", (DialogueEventType)eventTypeProperty.enumValueIndex);

            switch ((DialogueEventType)eventTypeProperty.enumValueIndex)
            {
                case DialogueEventType.StartDialogue:
                    EditorGUILayout.PropertyField(dialogueProperty);
                    EditorGUILayout.PropertyField(EventProperty);
                    break;
                case DialogueEventType.EndDialogue:
                    EditorGUILayout.PropertyField(dialogueProperty);
                    EditorGUILayout.PropertyField(EventProperty);
                    break;
                case DialogueEventType.PromptParagraph:
                    EditorGUILayout.PropertyField(promptProperty);

                    if (promptProperty.objectReferenceValue != null)
                    {
                        Prompt prompt = (Prompt)promptProperty.objectReferenceValue;
                        string[] paragraphs = prompt.paragraphs != null ? prompt.paragraphs.Select(x => x.paragraph).ToArray() : new string[0];
                        promptParagraphIndexProperty.intValue = EditorGUILayout.Popup("Response", promptParagraphIndexProperty.intValue, paragraphs);
                    }

                    EditorGUILayout.PropertyField(EventProperty);
                    break;
                case DialogueEventType.PromptResponse:
                    EditorGUILayout.PropertyField(promptProperty);

                    if (promptProperty.objectReferenceValue != null)
                    {
                        Prompt prompt = (Prompt)promptProperty.objectReferenceValue;
                        string[] responses = prompt.promptResponses != null ? prompt.promptResponses.Select(x => x.response).ToArray() : new string[0];
                        promptResponseIndexProperty.intValue = EditorGUILayout.Popup("Response", promptResponseIndexProperty.intValue, responses);
                    }

                    EditorGUILayout.PropertyField(EventProperty);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

}


