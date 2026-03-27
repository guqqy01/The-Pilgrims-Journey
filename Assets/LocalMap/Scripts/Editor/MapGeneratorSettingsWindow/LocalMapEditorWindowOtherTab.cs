using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace LocalMap.Editor
{
    [Serializable]
    public class LocalMapEditorWindowOtherTab : EditorMenuToolbarWindow, IEditorMenuToolbarTab
    {
        private SerializedObject _serializedObject;

        public LocalMapEditorWindowPreviewGenerationSettings PreviewSettings;

        public string Name => "Other Settings";

        public LocalMapEditorWindowOtherTab(SerializedObject serializedObject)
        {
            _serializedObject = serializedObject;

            _tabs.Clear();

            PreviewSettings = new LocalMapEditorWindowPreviewGenerationSettings();

            _tabs.Add(PreviewSettings);
        }


        public void EnsureSerializedObject(SerializedObject serializedObject)
        {
            _serializedObject = serializedObject;

            _tabs ??= new List<IEditorMenuToolbarTab>();

            _tabs.Clear();

            _tabs.Add(PreviewSettings);
        }

        public void DrawTab(Rect rect)
        {
            DrawToolbarWindow(rect);
        }

        public float GetHeight(Rect rect)
        {
            return rect.height;
        }
    }
}
