using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace DialogueSystem
{
    public class DialogueMixerGroupEditorWindowDropDown : PopupWindowContent
    {
        private readonly AudioDialogueMixerGroup audioDialogueMixerGroup;
        private readonly EditorWindow parentWindow;

        private const float space = 5f;

        private const float width = 210f;
        private const float height = 52f;

        public DialogueMixerGroupEditorWindowDropDown(AudioDialogueMixerGroup audioDialogueMixerGroup, EditorWindow parentWindow)
        {
            this.audioDialogueMixerGroup = audioDialogueMixerGroup;
            this.parentWindow = parentWindow;
        }

        public override void OnGUI(Rect rect)
        {
            if (audioDialogueMixerGroup == null)
                return;

            float timeValue = audioDialogueMixerGroup.Time;
            float width = 200f;

            Rect dialogueItemTimeRect = new Rect() { yMin = rect.yMin + space, height = EditorGUIUtility.singleLineHeight, xMin = rect.xMin + space, width = width };
            Rect dialogueItemAudioMixerGroupRect = new Rect() { yMin = dialogueItemTimeRect.yMax + space, height = EditorGUIUtility.singleLineHeight, xMin = rect.xMin + space, width = width };

            float holder = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80f;

            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName("itemTime");
            audioDialogueMixerGroup.Time = EditorGUI.DelayedFloatField(dialogueItemTimeRect, new GUIContent("Time"), timeValue);
            GUI.SetNextControlName("itemAudioMixerGroup");

            audioDialogueMixerGroup.MixerGroup = (AudioMixerGroup)EditorGUI.ObjectField(dialogueItemAudioMixerGroupRect, audioDialogueMixerGroup.MixerGroup, typeof(AudioMixerGroup), false);

            if (EditorGUI.EndChangeCheck())
            {
                parentWindow.Repaint();
            }

            EditorGUIUtility.labelWidth = holder;

            Event e = Event.current;

            if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Escape || e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
            {
                editorWindow.Close();
            }
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(width, height);
        }
    }
}

