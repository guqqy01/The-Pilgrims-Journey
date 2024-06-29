using UnityEngine;
using UnityEditor;
using DialogueSystem;
using System.Collections.Generic;
using UnityEditorInternal;
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.TerrainTools;
using UnityEditor.Presets;
using UnityEditor.Rendering;

namespace Map
{
    public class MapGeneratorSettingsEditorWindow : EditorWindow
    {
        private LocalMapGenerationSettings _generationSettings;

        private SerializedObject _serializedObject;

        private ReordableFoldoutList _baseTilesList;
        private ReordableFoldoutList _pathTilesList;
        private ReordableFoldoutList _overlayMapObjectList;

        private ScrollableToolbar _scrollableToolbar;
        
        private readonly List<ReordableFoldoutList> _terrainNoiseItemLists = new List<ReordableFoldoutList>();
        private readonly List<ReordableFoldoutList> _terrainNoiseSpriteLists = new List<ReordableFoldoutList>();

        private VerticalSeperator verticalSeperator;

        public Rect WindowRect
        {
            get
            {
                if (verticalSeperator == null)
                {
                    Rect wholeRect = new Rect() { width = position.width, height = position.height };
                    verticalSeperator = new VerticalSeperator(wholeRect);
                }

                return verticalSeperator.topRect;
            }
        }
        public Rect GeneratorSettingsRect => new Rect(WindowRect) { yMin = WindowRect.yMin + 2, height = EditorGUIUtility.singleLineHeight };
        public Rect MainToolbarRect => new Rect(WindowRect) { yMin = GeneratorSettingsRect.yMax + 2, height = 2f * EditorGUIUtility.singleLineHeight };
        private Rect BaseTilesToolbarRect => new Rect(WindowRect) { yMin = MainToolbarRect.yMax, height = 2f * EditorGUIUtility.singleLineHeight };
        private Rect BaseTilePropertyRect => new Rect(WindowRect) { yMin = BaseTilesToolbarRect.yMax };
        private Rect ContentExcludingMainToolbarRect => new Rect(WindowRect) { yMin = MainToolbarRect.yMax };

        private Rect PreviewWindowRect
        {
            get
            {
                if (verticalSeperator == null)
                {
                    Rect wholeRect = new Rect() { width = position.width, height = position.height };
                    verticalSeperator = new VerticalSeperator(wholeRect);
                }

                return verticalSeperator.bottomRect;
            }
        }

        private int _mainToolbarIndex = 0;
        private int _baseTilesToolbarIndex = 0;

        private readonly List<string> _mainToolbarTitles = new List<string>()
        {
            "Base Tiles",
            "Map Objects",
            "Terrain Noise"
        };

        private readonly List<string> _baseTilesToolbarTitles = new List<string>()
        {
            "Map Tiles",
            "Path Tiles",
        };

        #region Init And Setup

        public static void Init(SerializedProperty audioDialogueProperty)
        {
            MapGeneratorSettingsEditorWindow window = (MapGeneratorSettingsEditorWindow)EditorWindow.GetWindow(typeof(MapGeneratorSettingsEditorWindow));

            window.Setup(audioDialogueProperty);
        }

        [MenuItem("Window/Custom Windows/Map Generator Settings")]
        public static void Init()
        {
            MapGeneratorSettingsEditorWindow window = (MapGeneratorSettingsEditorWindow)EditorWindow.GetWindow(typeof(MapGeneratorSettingsEditorWindow));
        }

        public void Setup(SerializedProperty MapGenerationSettingsSerializedProperty)
        {
            _generationSettings = (LocalMapGenerationSettings)MapGenerationSettingsSerializedProperty.GetTargetObjectOfProperty();
        }

        private void SetupLists()
        {
            if (_generationSettings == null) return;

            _serializedObject = new SerializedObject(_generationSettings);

            SerializedProperty baseTilesProperty = _serializedObject.FindProperty("baseTiles").FindPropertyRelative("weightedList");
            _baseTilesList = new ReordableFoldoutList(_serializedObject, baseTilesProperty, (index) => 64f, DrawBaseTile, "Map Tiles");

            SerializedProperty pathTilesProperty = _serializedObject.FindProperty("pathTiles").FindPropertyRelative("weightedList");
            _pathTilesList = new ReordableFoldoutList(_serializedObject, pathTilesProperty, (index) => 64f, DrawBaseTile, "Path Tiles");

            SerializedProperty mapObjectsProp = _serializedObject.FindProperty("mapObjects");
            _overlayMapObjectList = new ReordableFoldoutList(_serializedObject, mapObjectsProp, (index) => 128f, DrawOverlayMapObject, "Map Objects");
        }

        #endregion

        private void OnGUI()
        {
            _generationSettings = EditorGUI.ObjectField(GeneratorSettingsRect, new GUIContent("Generator Settings"), _generationSettings, typeof(LocalMapGenerationSettings), false) as LocalMapGenerationSettings;

            if (_generationSettings != null && (_serializedObject == null || _serializedObject.targetObject != _generationSettings))
            {
                SetupLists();
            }

            if (verticalSeperator.DoSeperator(new Rect() { width = position.width, height = position.height }))
            {
                Repaint();
            }

            _mainToolbarIndex = GUI.Toolbar(MainToolbarRect, _mainToolbarIndex, _mainToolbarTitles.ToArray());

            switch (_mainToolbarIndex)
            {
                case 0:
                    ShowBaseTileTab(); break;
                case 1:
                    ShowMapObjectsTab(); break;
                case 2:
                    ShowTerrainNoiseTab(); break;
            }

            

            ShowPreviewWindow();
        }

        #region Base Tiles Tab

        private void ShowBaseTileTab()
        {
            _baseTilesToolbarIndex = GUI.Toolbar(BaseTilesToolbarRect, _baseTilesToolbarIndex, _baseTilesToolbarTitles.ToArray());

            switch (_baseTilesToolbarIndex)
            {
                case 0:
                    ShowMapTilesSubTab(); break;
                case 1:
                    ShowPathTilesSubTab(); break;
            }

            void ShowMapTilesSubTab()
            {
                if (_generationSettings == null || _baseTilesList == null)
                    return;

                _baseTilesList.DoList(BaseTilePropertyRect);
            }

            void ShowPathTilesSubTab()
            {
                if (_generationSettings == null)
                    return;

                _pathTilesList.DoList(BaseTilePropertyRect);
            }
        }

        private void DrawBaseTile(Rect rect, SerializedProperty baseTileProperty, int index)
        {
            float spriteLength = rect.height - 2;
            Rect spriteRect = new Rect(rect) { yMin = rect.yMin + 2, height = spriteLength, width = spriteLength };
            Rect spriteObjectPickerRect = new Rect(rect) {xMin = spriteRect.xMax, yMin = rect.yMin + 2, height = EditorGUIUtility.singleLineHeight };
            Rect weightRect = new Rect(rect) { xMin = spriteRect.xMax, yMin = spriteObjectPickerRect.yMax + 2f, height = EditorGUIUtility.singleLineHeight };

            SerializedProperty spriteProperty = baseTileProperty.FindPropertyRelative("item");
            SerializedProperty weightCurveProperty = baseTileProperty.FindPropertyRelative("weightCurve");
            
            Sprite sprite = spriteProperty.objectReferenceValue as Sprite;

            Texture2D spriteTexture = null;

            if (sprite != null)
            {
                spriteTexture = AssetPreview.GetAssetPreview(sprite);
                if (spriteTexture != null)
                {
                    GUI.Label(spriteRect, spriteTexture);
                }
            }

            EditorGUI.ObjectField(spriteObjectPickerRect, spriteProperty, new GUIContent("Tile"));

            EditorGUI.PropertyField(weightRect, weightCurveProperty);
        }

        #endregion

        #region Map Objects Tab

        private void ShowMapObjectsTab()
        {
            if (_generationSettings == null || _overlayMapObjectList == null)
                return;

            _overlayMapObjectList.DoList(ContentExcludingMainToolbarRect);
        }

        private Texture2D GetOverlayPreview(GameObject go)
        {
            if (go == null) return null;
            Texture2D tex = AssetPreview.GetAssetPreview(go);
            if (tex == null)
                tex = AssetPreview.GetMiniThumbnail(go);
            return tex;
        }

        private void DrawOverlayMapObject(Rect rect, SerializedProperty element, int index)
        {
            rect.y += 2;
            rect.height -= 4;

            SerializedProperty nameProp = element.FindPropertyRelative("name");
            SerializedProperty overlayProp = element.FindPropertyRelative("overlay");
            SerializedProperty sizeProp = element.FindPropertyRelative("size");
            SerializedProperty curveProp = element.FindPropertyRelative("amountToSpawnCurve");

            float previewSize = rect.height - EditorGUIUtility.singleLineHeight;

            // --- Left: preview ---
            Rect previewRect = new Rect(rect.x, rect.y, previewSize, previewSize);


            Texture2D previewTex = null;
            if (overlayProp != null && overlayProp.objectReferenceValue != null)
            {
                GameObject go = overlayProp.objectReferenceValue as GameObject;
                if (go != null)
                {
                    previewTex = GetOverlayPreview(go);
                }
            }

            if (previewTex != null)
                GUI.Label(previewRect, previewTex);

            // --- Under preview: ObjectField for overlay ---
            Rect overlayFieldRect = new Rect(rect.x, rect.yMax - EditorGUIUtility.singleLineHeight, previewSize, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(overlayFieldRect, overlayProp, GUIContent.none);

            // --- Right side: other fields ---
            float rightX = previewRect.xMax + 6;
            float rightWidth = rect.width - previewSize - 6;

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = 2;

            Rect nameRect = new Rect(rightX, rect.y, rightWidth, lineHeight);
            Rect sizeRect = new Rect(rightX, nameRect.yMax + spacing, rightWidth, lineHeight * 2);
            Rect curveRect = new Rect(rightX, sizeRect.yMax + spacing, rightWidth, lineHeight);

            EditorGUI.PropertyField(nameRect, nameProp);
            EditorGUI.PropertyField(sizeRect, sizeProp);
            EditorGUI.PropertyField(curveRect, curveProp);
        }



        #endregion

        #region Terrain Noise Tab

        private void ShowTerrainNoiseTab()
        {
            if (_generationSettings == null || _baseTilesList == null)
                return;

            EditorGUI.indentLevel++;

            _serializedObject.Update();

            if (_scrollableToolbar == null)
                _scrollableToolbar = new ScrollableToolbar();

            Rect scrollableToolbarRect = new Rect(ContentExcludingMainToolbarRect) { height = 2f * EditorGUIUtility.singleLineHeight, xMax = ContentExcludingMainToolbarRect.xMax - 100f};

            Rect addButtonRect = new Rect(scrollableToolbarRect) { xMin = scrollableToolbarRect.xMax + 10f ,xMax = (scrollableToolbarRect.xMax + ContentExcludingMainToolbarRect.xMax) / 2f - 2f};
            Rect removeButtonRect = new Rect(scrollableToolbarRect) { xMin = addButtonRect.xMax , width = addButtonRect.width}; ;

            SerializedProperty terrainProperty = _serializedObject.FindProperty("terrainNoiseLists");

            if(GUI.Button(addButtonRect, new GUIContent("+")))
            {
                _serializedObject.Update();

                terrainProperty.InsertArrayElementAtIndex(terrainProperty.arraySize);

                _serializedObject.ApplyModifiedProperties();
            }

            if (GUI.Button(removeButtonRect, new GUIContent("-")))
            {
                _serializedObject.Update();

                _terrainNoiseSpriteLists.Clear();
                _terrainNoiseItemLists.Clear();

                terrainProperty.DeleteArrayElementAtIndex(_scrollableToolbar.SelectedIndex);

                if (_scrollableToolbar.SelectedIndex >= terrainProperty.arraySize)
                    _scrollableToolbar.SelectedIndex--;


                _serializedObject.ApplyModifiedProperties();
            }


            string[] terrainPropertyTitles = new string[terrainProperty.arraySize];

            for (int i = 0; i < terrainProperty.arraySize; i++)
            {
                terrainPropertyTitles[i] = terrainProperty.GetArrayElementAtIndex(i).displayName;
            }

            _scrollableToolbar.HandleScrollableToolbar(scrollableToolbarRect, terrainPropertyTitles);

            Rect terrainContentRect = new Rect(ContentExcludingMainToolbarRect) { yMin = scrollableToolbarRect.yMax };

            DrawTerrainNoiseElement(terrainContentRect, terrainProperty.GetArrayElementAtIndex(_scrollableToolbar.SelectedIndex), _scrollableToolbar.SelectedIndex);

            _serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel--;
        }

        private float GetTerrainNoiseSpriteElementHeight(int index)
        {
            if(_terrainNoiseSpriteLists.Count <= index)
                return 0;

            return _terrainNoiseSpriteLists[index].GetHeight();
        }

        private void DrawTerrainNoiseElement(Rect rect, SerializedProperty terrainNoiseProperty, int index)
        {
            SerializedProperty nameProperty = terrainNoiseProperty.FindPropertyRelative("name");
            SerializedProperty priorityProperty = terrainNoiseProperty.FindPropertyRelative("priority");
            SerializedProperty scaleProperty = terrainNoiseProperty.FindPropertyRelative("scale");
            SerializedProperty terrainNoiseItemsProperty = terrainNoiseProperty.FindPropertyRelative("items");

            Rect nameRect = new Rect(rect) { yMin = rect.yMin + 2f, height = EditorGUIUtility.singleLineHeight };
            Rect priorityRect = new Rect(rect) { yMin = nameRect.yMax + 2f, height = EditorGUIUtility.singleLineHeight };
            Rect scaleRect = new Rect(rect) { yMin = priorityRect.yMax + 2f, height = EditorGUIUtility.singleLineHeight * 2f + 2f };
            Rect terrainNoiseItemsRect = new Rect(rect) { yMin = scaleRect.yMax + 2f, xMax = rect.xMax};
            _serializedObject.Update();
            EditorGUI.PropertyField(nameRect, nameProperty);
            EditorGUI.PropertyField(priorityRect, priorityProperty);
            EditorGUI.PropertyField(scaleRect, scaleProperty);
            _serializedObject.ApplyModifiedProperties();
            SetupTerrainNoiseItemsList(terrainNoiseItemsProperty, index, 256f);

            _terrainNoiseItemLists[index].DoList(terrainNoiseItemsRect);
        }

        private void SetupTerrainNoiseItemsList(SerializedProperty terrainNoiseItemsProperty, int index, float height)
        {
            while(_terrainNoiseItemLists.Count <= index)
            {
                _terrainNoiseItemLists.Add(null);
            }
            if (_terrainNoiseItemLists.Count <= index)
            {
                ReordableFoldoutList list = new ReordableFoldoutList(_serializedObject, terrainNoiseItemsProperty, (index) => GetTerrainNoiseSpriteElementHeight(index), DrawTerrainNoiseItemElements, "Terrain Noise");
                _terrainNoiseItemLists.Add(list);
            }
            else if(_terrainNoiseItemLists[index] == null)
            {
                _terrainNoiseItemLists[index] = new ReordableFoldoutList(_serializedObject, terrainNoiseItemsProperty, (index) => GetTerrainNoiseSpriteElementHeight(index), DrawTerrainNoiseItemElements, "Terrain Noise");
            }
            else if (!_terrainNoiseItemLists[index]._listProperty.EqualContents(terrainNoiseItemsProperty))
            {
                _terrainNoiseItemLists[index] = new ReordableFoldoutList(_serializedObject, terrainNoiseItemsProperty, (index) => GetTerrainNoiseSpriteElementHeight(index), DrawTerrainNoiseItemElements, "Terrain Noise");
            }
        }

        private void DrawTerrainNoiseItemElements(Rect rect, SerializedProperty terrainNoiseItemProperty, int terrainNoiseIndex)
        {
            SerializedProperty perlinNoiseRangeSerializedProperty = terrainNoiseItemProperty.FindPropertyRelative("perlinNoiseRange");
            SerializedProperty spriteListSerializedProperty = terrainNoiseItemProperty.FindPropertyRelative("list").FindPropertyRelative("weightedList");

            Rect perloinNoiseRangeRect = new Rect(rect) { height = 2f * EditorGUIUtility.singleLineHeight };
            Rect spriteListRect = new Rect(rect) { yMin = perloinNoiseRangeRect.yMax + 4f };

            _serializedObject.Update();
            EditorGUI.PropertyField(rect, perlinNoiseRangeSerializedProperty);
            _serializedObject.ApplyModifiedProperties();

            SetupTerrainNoiseSpriteLists(spriteListSerializedProperty, terrainNoiseIndex, 64f);
            _terrainNoiseSpriteLists[terrainNoiseIndex].DoList(spriteListRect);
        }

        private void SetupTerrainNoiseSpriteLists(SerializedProperty terrainNoiseItemProperty, int terrainNoiseIndex, float height)
        {
            while(_terrainNoiseSpriteLists.Count <= terrainNoiseIndex)
            {
                _terrainNoiseSpriteLists.Add(null);
            }

            try
            {
                if (_terrainNoiseSpriteLists.Count <= terrainNoiseIndex)
                {
                    ReordableFoldoutList list = new ReordableFoldoutList(_serializedObject, terrainNoiseItemProperty, (index) => height, DrawBaseTile, "Terrain Sprites");
                    _terrainNoiseSpriteLists.Add(list);
                }
                else if (_terrainNoiseSpriteLists[terrainNoiseIndex] == null)
                {
                    _terrainNoiseSpriteLists[terrainNoiseIndex] = new ReordableFoldoutList(_serializedObject, terrainNoiseItemProperty, (index) => height, DrawBaseTile, "Terrain Sprites");
                }
                else if (_terrainNoiseSpriteLists[terrainNoiseIndex]._listProperty == null)
                {
                    _terrainNoiseSpriteLists[terrainNoiseIndex] = new ReordableFoldoutList(_serializedObject, terrainNoiseItemProperty, (index) => height, DrawBaseTile, "Terrain Sprites");
                }
                else if (!_terrainNoiseSpriteLists[terrainNoiseIndex]._listProperty.EqualContents(terrainNoiseItemProperty))
                {
                    _terrainNoiseSpriteLists[terrainNoiseIndex] = new ReordableFoldoutList(_serializedObject, terrainNoiseItemProperty, (index) => height, DrawBaseTile, "Terrain Sprites");
                }
            }
            catch(Exception e)
            {
                Debug.Log($"Terrain Noise Index: {terrainNoiseIndex}, Terrain Noise Item Property DisplayName: {terrainNoiseItemProperty.propertyPath}");

                throw e;
            }
            
        }

        #endregion

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

            if (previewSettings.HasFlag(MapGenerationPreviewSettings.TerrainNoise))
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
            if(_previewLocalMapGenerator != null && _generationSettings != null)

            _previewLocalMapGenerator.ShowMapPreview(_previewRenderUtility.camera, _generationSettings, previewSettings, GetSelectedIndices(selectedTerrain, _generationSettings.terrainNoiseLists.Count));
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
