using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEditor.Presets;
using NUnit.Framework;
using JetBrains.Annotations;
using UnityEngine.UI;

namespace LocalMap.Editor
{
    [Serializable]
    public class LocalMapGeneratorSettingsEditorWindow : EditorWindow
    {
        [SerializeField]
        private LocalMapGenerationSettings _generationSettings;

        private SerializedObject _serializedObject;
        private VerticalSeparator verticalSeparator;

        public Rect WindowRect => EnsureVerticalSeparator().topRect;
        public Rect PreviewWindowRect => EnsureVerticalSeparator().bottomRect;
        public Rect GeneratorSettingsRect => new Rect(WindowRect) { yMin = WindowRect.yMin + 2, height = EditorGUIUtility.singleLineHeight };

        private VerticalSeparator EnsureVerticalSeparator()
        {
            if (verticalSeparator == null)
                verticalSeparator = new VerticalSeparator(new Rect { width = position.width, height = position.height });

            return verticalSeparator;
        }

        #region Init And Setup

        [MenuItem("Window/Custom Windows/Map Generator Settings")]
        public static void Init()
        {
            LocalMapGeneratorSettingsEditorWindow window = (LocalMapGeneratorSettingsEditorWindow)EditorWindow.GetWindow(typeof(LocalMapGeneratorSettingsEditorWindow));
        }

        public static void Init(LocalMapGenerationSettings settings)
        {
            LocalMapGeneratorSettingsEditorWindow window = (LocalMapGeneratorSettingsEditorWindow)EditorWindow.GetWindow(typeof(LocalMapGeneratorSettingsEditorWindow));

            window._generationSettings = settings;
        }

        private void SetupSerializedObject()
        {
            if (_generationSettings == null) return;

            _serializedObject = new SerializedObject(_generationSettings);
            _mainToolbarWindow = null;
        }

        #endregion

        private void OnGUI()
        {
            _generationSettings = EditorGUI.ObjectField(GeneratorSettingsRect, new GUIContent("Generator Settings"), _generationSettings, typeof(LocalMapGenerationSettings), false) as LocalMapGenerationSettings;

            if (_generationSettings != null && (_serializedObject == null || _serializedObject.targetObject != _generationSettings))
            {
                SetupSerializedObject();
            }

            if (verticalSeparator != null && verticalSeparator.DoSeperator(new Rect() { width = position.width, height = position.height }))
            {
                Repaint();
            }


            if(_serializedObject != null)
            {
                SetupEditorMenuToolbarWindow();
                Rect rect = new Rect(WindowRect) { yMin = GeneratorSettingsRect.yMax + 2 };
                _mainToolbarWindow.DrawToolbarWindow(rect);
            }

            ShowPreviewWindow();
        }

        private EditorMenuToolbarWindow _mainToolbarWindow;
        private TerrainNoiseParentTab _terrainNoiseParentTab;
        
        private LocalMapEditorWindowOtherTab _otherSettingsTab;

        private void SetupEditorMenuToolbarWindow()
        {
            if (_mainToolbarWindow != null)
                return;

            if (_serializedObject == null)
                return;

            BaseAndPathTilesTab baseTilesTab = new BaseAndPathTilesTab(_serializedObject);
            MapObjectsTab mapObjectsTab = new MapObjectsTab(_serializedObject);
            _terrainNoiseParentTab = new TerrainNoiseParentTab(_serializedObject);
            _terrainNoiseParentTab.OnTabChange += Repaint;

            if(_otherSettingsTab == null)
            {
                _otherSettingsTab = new LocalMapEditorWindowOtherTab(_serializedObject);
            }
            else
            {
                _otherSettingsTab.EnsureSerializedObject(_serializedObject);
            }

            _mainToolbarWindow = new EditorMenuToolbarWindow(baseTilesTab, mapObjectsTab, _terrainNoiseParentTab, _otherSettingsTab);
        }

        private void OnDestroy()
        {
            if(_terrainNoiseParentTab != null)
            {
                _terrainNoiseParentTab.OnTabChange -= Repaint;
                _terrainNoiseParentTab.Dispose();
            }
        }

        #region Preview Window

        private PreviewRenderUtility _previewRenderUtility;
        [SerializeField]
        private LocalMapGenerator _previewLocalMapGenerator;

        [SerializeField]
        private MapGenerationPreviewSettings previewSettings;

        [SerializeField]
        private int selectedTerrain;

        private void OnEnable()
        {
            _previewRenderUtility = new PreviewRenderUtility();
            _previewRenderUtility.cameraFieldOfView = 90f;
            _previewRenderUtility.camera.transform.position = new Vector3(19.5f, 8.5f, -10);
            _previewRenderUtility.camera.transform.rotation = Quaternion.identity;

            GameObject obj = new GameObject();
            obj.hideFlags = HideFlags.HideAndDontSave;
            _previewLocalMapGenerator = obj.AddComponent<LocalMapGenerator>();

            _previewRenderUtility.AddSingleGO(obj);

            string presetPath = "Assets/LocalMap/LocalMapGenerator.preset";
            Preset preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);

            if (preset != null && preset.CanBeAppliedTo(_previewLocalMapGenerator))
            {
                preset.ApplyTo(_previewLocalMapGenerator);
            }

            GeneratePreviewMap();
        }

        private void OnDisable()
        {
            _previewLocalMapGenerator.Clear();

            _previewRenderUtility.Cleanup();
        }

        private void ShowPreviewWindow()
        {
            Rect labelRect = new Rect(PreviewWindowRect) { height = EditorGUIUtility.singleLineHeight };

            Rect previewRect = new Rect(PreviewWindowRect) { yMin = labelRect.yMax };

            GUI.Label(labelRect, "Map Generator Preview", EditorStyles.boldLabel);

            if (Event.current.type == EventType.Repaint)
            {
                _previewRenderUtility.BeginPreview(previewRect, GUIStyle.none);

                _previewRenderUtility.camera.Render();
                Texture resultRender = _previewRenderUtility.EndPreview();
                GUI.DrawTexture(previewRect, resultRender, ScaleMode.StretchToFill, false);
            }

            Rect generateMapButton = new Rect(previewRect) { xMin = previewRect.xMax - 100f, width = 90f, yMin = previewRect.yMax - 40f, height = 30f};

            Rect enumRect = new Rect(previewRect) { xMin = previewRect.xMin + 10f, width = 90f, yMin = previewRect.yMin + 10f, height = EditorGUIUtility.singleLineHeight };

            if(_generationSettings == null)
            {
                EditorGUI.LabelField(previewRect, new GUIContent("Select a generation settings object above"), new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter});

                return;
            }

            string[] terrainNoiseNames = new string[_generationSettings.terrainNoiseLists.Count];

            for (int i = 0; i < terrainNoiseNames.Length; i++)
            {
                terrainNoiseNames[i] = _generationSettings.terrainNoiseLists[i].name;
            }

            previewSettings = (MapGenerationPreviewSettings)EditorGUI.EnumFlagsField(enumRect, previewSettings);

            if (previewSettings.HasFlag(MapGenerationPreviewSettings.TerrainNoise) && terrainNoiseNames.Length > 0)
            {
                Rect terrainRect = new Rect(enumRect) { position = enumRect.position + new Vector2(0, enumRect.height + 2f) };

                selectedTerrain = EditorGUI.MaskField(terrainRect, selectedTerrain, terrainNoiseNames);
            }

            if (GUI.Button(generateMapButton, "Generate Map"))
            {
                GeneratePreviewMap();
            }
        }

        private void GeneratePreviewMap()
        {
            if (_previewLocalMapGenerator == null || _generationSettings == null)
                return;
            
            if(_otherSettingsTab == null || _otherSettingsTab.PreviewSettings == null)
            {
                Debug.LogWarning("Other tab is null, cannot generate preview map");
                return;
            }

            if(_previewRenderUtility == null)
            {
                Debug.LogWarning("Preview Render Utility is null, cannot generate preview map");
                return;
            }


            _previewLocalMapGenerator.ShowMapPreview(
                _previewRenderUtility.camera, 
                _generationSettings, 
                previewSettings, 
                GetSelectedIndices(selectedTerrain, _generationSettings.terrainNoiseLists.Count),
                _otherSettingsTab.PreviewSettings.StartWall,
                _otherSettingsTab.PreviewSettings.ToDirections,
                _otherSettingsTab.PreviewSettings.OtherFromDirections,
                _otherSettingsTab.PreviewSettings.NullableSeed);
            
        }

        List<int> GetSelectedIndices(int selectedMask, int optionCount)
        {
            var indices = new List<int>();
            for (int i = 0; i < optionCount; i++)
            {
                if ((selectedMask & (1 << i)) != 0)
                    indices.Add(i);
            }
            return indices;
        }

        #endregion
    }

}
