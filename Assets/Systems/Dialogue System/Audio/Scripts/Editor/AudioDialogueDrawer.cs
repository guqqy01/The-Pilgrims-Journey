using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DialogueSystem
{
    [CustomPropertyDrawer(typeof(AudioDialogue))]
    public class AudioDialogueDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            if (GUI.Button(position, new GUIContent("Open Audio Dialogue Editor")))
            {
                AudioDialogueEditorWindow.Init(property);
            }
        }
    }
}