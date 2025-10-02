using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DialogueSystem
{
    [CustomEditor(typeof(PromptEventListener))]
    public class PromptEventListenerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty promptProperty = serializedObject.FindProperty("prompt");
            SerializedProperty promptResponseIndexProperty = serializedObject.FindProperty("promptResponseIndex");
            SerializedProperty eventProperty = serializedObject.FindProperty("Event");

            EditorGUILayout.PropertyField(promptProperty);

            if (promptProperty.objectReferenceValue != null)
            {
                Prompt prompt = (Prompt)promptProperty.objectReferenceValue;
                string[] responses = prompt.promptResponses != null
                    ? prompt.promptResponses.Select(x => x.response).ToArray()
                    : new string[0];

                promptResponseIndexProperty.intValue =
                    EditorGUILayout.Popup("Response", promptResponseIndexProperty.intValue, responses);
            }

            EditorGUILayout.PropertyField(eventProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}


