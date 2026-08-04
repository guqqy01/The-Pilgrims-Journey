using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

namespace DialogueSystem
{
    public class SectionalAudioClipEditDropDownWindow : PopupWindowContent
    {
        private readonly AudioDialogueEditorWindow parentWindow;
        private readonly SectionalAudioClip sectionalAudioClip;

        private readonly SectionalAudioClipViewer sectionalAudioClipViewer;
        private const float space = 5f;

        private const float width = 210f;
        private float Height => (EditorGUIUtility.singleLineHeight + space) * 4 + space;

        public SectionalAudioClipEditDropDownWindow(SectionalAudioClipViewer sectionalAudioClipViewer, AudioDialogueEditorWindow parentWindow)
        {
            sectionalAudioClip = sectionalAudioClipViewer.sectionalAudioClip;
            this.sectionalAudioClipViewer = sectionalAudioClipViewer;
            this.parentWindow = parentWindow;
        }

        public override void OnGUI(Rect rect)
        {
            if (sectionalAudioClip == null)
                return;

            float timeValue = sectionalAudioClip.StartTime;
            float width = 200f;

            Rect AudioClipRect = new Rect() { yMin = rect.yMin + space, height = EditorGUIUtility.singleLineHeight, xMin = rect.xMin + space, width = width };
            Rect StartTimeRect = new Rect() { yMin = AudioClipRect.yMax + space, height = EditorGUIUtility.singleLineHeight, xMin = rect.xMin + space, width = width };
            Rect SliderRect = new Rect() { yMin = StartTimeRect.yMax + space, height = EditorGUIUtility.singleLineHeight, xMin = rect.xMin + space, width = width };
            Rect ButtonRect = new Rect() { yMin = SliderRect.yMax + space, height = EditorGUIUtility.singleLineHeight, xMin = rect.xMin + space, width = width };

            float holder = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80f;


            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName("audioClip");
            sectionalAudioClipViewer.AudioClip = (AudioClip)EditorGUI.ObjectField(AudioClipRect, sectionalAudioClip.audioClip, typeof(AudioClip), false);

            GUI.SetNextControlName("startTime");
            sectionalAudioClip.StartTime = EditorGUI.DelayedFloatField(StartTimeRect, new GUIContent("Time"), timeValue);

            CustomEditorGUI.DrawRangeSlider(sectionalAudioClip.slider, SliderRect, new GUIContent("Range"));

            if (GUI.Button(ButtonRect, new GUIContent("Edit")))
            {
                parentWindow.EditSectionalAudioClip(sectionalAudioClipViewer);
                editorWindow.Close();
            }

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
            return new Vector2(width, Height);
        }
    }
}

