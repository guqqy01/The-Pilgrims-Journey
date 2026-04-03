using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;

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
            LocalMapEditorAutoTileSettings localMapEditorAutoTileSettings = new LocalMapEditorAutoTileSettings(serializedObject);

            _tabs.Add(PreviewSettings);
            _tabs.Add(localMapEditorAutoTileSettings);
        }


        public void EnsureSerializedObject(SerializedObject serializedObject)
        {
            _serializedObject = serializedObject;

            _tabs ??= new List<IEditorMenuToolbarTab>();

            _tabs.Clear();

            LocalMapEditorAutoTileSettings localMapEditorAutoTileSettings = new LocalMapEditorAutoTileSettings(serializedObject);

            _tabs.Add(PreviewSettings);
            _tabs.Add(localMapEditorAutoTileSettings);
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

    public class LocalMapEditorAutoTileSettings : IEditorMenuToolbarTab
    {
        public string Name => "Auto Tiles";

        private SerializedObject _serializedObject;
        private LocalMapGenerationSettings _localMapGenerationSettings;

        public LocalMapEditorAutoTileSettings(SerializedObject serializedObject)
        {
            _serializedObject = serializedObject;
            _localMapGenerationSettings = serializedObject.targetObject as LocalMapGenerationSettings;
        }

        public void DrawTab(Rect rect)
        {
            Rect autoTileRect = new Rect(rect) { height = EditorGUIUtility.singleLineHeight};

            EditorGUI.BeginChangeCheck();

            EditorGUI.ObjectField(autoTileRect, _serializedObject.FindProperty(nameof(_localMapGenerationSettings.autoBaseTileSettings)));

            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
            }
        }

        public float GetHeight(Rect rect)
        {
            return rect.height;
        }
    }
}
