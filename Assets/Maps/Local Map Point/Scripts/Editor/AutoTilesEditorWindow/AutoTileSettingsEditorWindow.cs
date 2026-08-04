using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LocalMap.Editor
{
    [Serializable]
    public class AutoTileSettingsEditorWindow : EditorWindow
    {
        [SerializeField]
        private AutoTileSettings _autoTileSettings;

        private SerializedObject _serializedObject;
        private SerializedProperty CurrentlySelectedAutoTilesProp
        {
            get
            {
                return _serializedObject.FindProperty(nameof(_autoTileSettings.autoTileSetups)).GetArrayElementAtIndex(CurrentlySelectedAutoTileIndex);
            }
        }

        private SerializedProperty CurrentlySelectedAutoTileSpriteAndWalkabilityProperty
        {
            get
            {
                if (CurrentlySelectedAutoTilesProp == null)
                    return null;

                string propName = _selectedTileOrientation switch
                {
                    TileOrientations.StraightTopCenter => nameof(CurrentlySelectedAutoTile.TopCenterAutoTile),
                    TileOrientations.StraightCenterLeft => nameof(CurrentlySelectedAutoTile.CenterLeftAutoTile),
                    TileOrientations.StraightCenterRight => nameof(CurrentlySelectedAutoTile.CenterRightAutoTile),
                    TileOrientations.StraightBottomCenter => nameof(CurrentlySelectedAutoTile.BottomCenterAutoTile),
                    TileOrientations.SmallCornerTopRight => nameof(CurrentlySelectedAutoTile.TopRightAutoTile),
                    TileOrientations.SmallCornerTopLeft => nameof(CurrentlySelectedAutoTile.TopLeftAutoTile),
                    TileOrientations.SmallCornerBottomLeft => nameof(CurrentlySelectedAutoTile.BottomLeftAutoTile),
                    TileOrientations.SmallCornerBottomRight => nameof(CurrentlySelectedAutoTile.BottomRightAutoTile),
                    TileOrientations.BigCornerTopAndLeft => nameof(CurrentlySelectedAutoTile.TopAndLeftAutoTile),
                    TileOrientations.BigCornerTopAndRight => nameof(CurrentlySelectedAutoTile.TopAndRightAutoTile),
                    TileOrientations.BigCornerBottomAndLeft => nameof(CurrentlySelectedAutoTile.BottomAndLeftAutoTile),
                    TileOrientations.BigCornerBottomAndRight => nameof(CurrentlySelectedAutoTile.BottomAndRightAutoTile),
                    _ => null
                };

                if (propName == null) return null;

                return CurrentlySelectedAutoTilesProp.FindPropertyRelative(propName);
            }
        }

        private DraggableArea boundsRectDraggableArea;

        private ReorderableList drawOrderReordableFoldoutList;

        private int _currentlySelectedAutoTileIndex;

        private TileOrientations _selectedTileOrientation;

        private enum TileOrientations
        {
            StraightTopCenter = 0,
            StraightCenterLeft = 1,
            StraightCenterRight = 2,
            StraightBottomCenter = 3,
            SmallCornerTopRight = 4,
            SmallCornerTopLeft = 5,
            SmallCornerBottomLeft = 6,
            SmallCornerBottomRight = 7,
            BigCornerTopAndLeft = 8,
            BigCornerTopAndRight = 9,
            BigCornerBottomAndLeft = 10,
            BigCornerBottomAndRight = 11,
        }

        private int CurrentlySelectedAutoTileIndex
        {
            get
            {
                if (_autoTileSettings == null)
                    return default;

                if (_autoTileSettings.autoTileSetups == null || _autoTileSettings.autoTileSetups.Count <= 0)
                    return default;

                if (_currentlySelectedAutoTileIndex < 0 || _currentlySelectedAutoTileIndex > _autoTileSettings.autoTileSetups.Count - 1)
                    _currentlySelectedAutoTileIndex = Mathf.Clamp(_currentlySelectedAutoTileIndex, 0, _autoTileSettings.autoTileSetups.Count - 1);

                return _currentlySelectedAutoTileIndex;
            }
            set
            {
                if (_autoTileSettings == null)
                    return;

                if (_autoTileSettings.autoTileSetups == null || _autoTileSettings.autoTileSetups.Count <= 0)
                    return;

                _currentlySelectedAutoTileIndex = Mathf.Clamp(value, 0, _autoTileSettings.autoTileSetups.Count - 1);
            }
        }

        private AutoTiles CurrentlySelectedAutoTile
        {
            get
            {
                if (_autoTileSettings == null)
                    return null;

                if (_autoTileSettings.autoTileSetups == null || _autoTileSettings.autoTileSetups.Count <= 0)
                    return null;

                return _autoTileSettings.autoTileSetups[CurrentlySelectedAutoTileIndex];
            }
        }

        private AutoTileSpriteAndWalkability SelectedAutoTileSpriteAndWalkability
        {
            get
            {
                if(CurrentlySelectedAutoTile == null)
                    return null;

                return _selectedTileOrientation switch
                {
                    TileOrientations.StraightTopCenter => CurrentlySelectedAutoTile.TopCenterAutoTile,
                    TileOrientations.StraightCenterLeft => CurrentlySelectedAutoTile.CenterLeftAutoTile,
                    TileOrientations.StraightCenterRight => CurrentlySelectedAutoTile.CenterRightAutoTile,
                    TileOrientations.StraightBottomCenter => CurrentlySelectedAutoTile.BottomCenterAutoTile,
                    TileOrientations.SmallCornerTopRight => CurrentlySelectedAutoTile.TopRightAutoTile,
                    TileOrientations.SmallCornerTopLeft => CurrentlySelectedAutoTile.TopLeftAutoTile,
                    TileOrientations.SmallCornerBottomLeft => CurrentlySelectedAutoTile.BottomLeftAutoTile,
                    TileOrientations.SmallCornerBottomRight => CurrentlySelectedAutoTile.BottomRightAutoTile,
                    TileOrientations.BigCornerTopAndLeft => CurrentlySelectedAutoTile.TopAndLeftAutoTile,
                    TileOrientations.BigCornerTopAndRight => CurrentlySelectedAutoTile.TopAndRightAutoTile,
                    TileOrientations.BigCornerBottomAndLeft => CurrentlySelectedAutoTile.BottomAndLeftAutoTile,
                    TileOrientations.BigCornerBottomAndRight => CurrentlySelectedAutoTile.BottomAndRightAutoTile,
                    _ => null
                };
            }
        }

        private Rect WindowRect => new Rect() { width = position.width, height = position.height };
        private Rect AutoTileSettingsRect => new Rect(WindowRect) { yMin = WindowRect.yMin + spacing, height = EditorGUIUtility.singleLineHeight };
        private Rect AddAndSubtractRect => new Rect(WindowRect) { yMin = AutoTileSettingsRect.yMax + spacing, height = 2f * EditorGUIUtility.singleLineHeight };
        private Rect AddRect => new Rect(AddAndSubtractRect) { width = AddAndSubtractRect.width / 2f - 1f };
        private Rect SubtractRect => new Rect(AddAndSubtractRect) {xMin = AddRect.xMax + spacing };
        private Rect SelectAutoTileRect => new Rect(WindowRect) { yMin = AddAndSubtractRect.yMax + spacing, height = EditorGUIUtility.singleLineHeight };

        private const float spacing = 2f;
        private const float gridPadding = 10f;
        private const float gridSpacing = 3f;
        private float SLH => EditorGUIUtility.singleLineHeight;

        private Rect GetGridRect()
        {
            float yMin = SelectAutoTileRect.yMax + spacing + gridPadding;
            float yMax = WindowRect.yMax - gridPadding;
            float xMin = WindowRect.xMin + gridPadding;
            float xMax = WindowRect.xMax / 2f;

            float width = xMax - xMin;
            float height = yMax - yMin;

            float size = MathF.Min(width, height);

            Vector2 center = new Vector2((xMax + xMin) / 2f, (yMax + yMin) / 2f);
            return new Rect() { size = new Vector2(size, size), center = center , x = gridPadding};
        }

        [MenuItem("Window/Custom Windows/Auto Tile Settings")]
        public static void Init()
        {
            AutoTileSettingsEditorWindow window = (AutoTileSettingsEditorWindow)EditorWindow.GetWindow(typeof(AutoTileSettingsEditorWindow));
        }

        public static void Init(AutoTileSettings settings)
        {
            AutoTileSettingsEditorWindow window = (AutoTileSettingsEditorWindow)EditorWindow.GetWindow(typeof(AutoTileSettingsEditorWindow));

            window._autoTileSettings = settings;
        }

        private void SetupSerializedObject()
        {
            if (_autoTileSettings == null) return;

            _serializedObject = new SerializedObject(_autoTileSettings);
        }

        private void OnGUI()
        {
            _autoTileSettings = EditorGUI.ObjectField(AutoTileSettingsRect, new GUIContent("Auto Tile Settings"), _autoTileSettings, typeof(AutoTileSettings), false) as AutoTileSettings;

            if (_autoTileSettings != null && (_serializedObject == null || _serializedObject.targetObject != _autoTileSettings))
            {
                SetupSerializedObject();
            }

            if(_autoTileSettings == null)
            {
                EditorGUI.LabelField(WindowRect, new GUIContent("Select an auto tile settings object"), new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter});
                return;
            }

            AddAndSubtractAutoTiles();
            AutoTileSelectionDropDown();

            DrawGridWindow();
        }

        private void AddAndSubtractAutoTiles()
        {
            if (_autoTileSettings == null || _serializedObject == null)
                return;

            if(GUI.Button(AddRect, new GUIContent("Add auto tiles")))
            {
                _autoTileSettings.autoTileSetups.Add(new AutoTiles());

                _serializedObject.ApplyModifiedProperties();
                _serializedObject.Update();
            }
            if(GUI.Button(SubtractRect, new GUIContent("Remove selected auto tiles")))
            {
                _autoTileSettings.autoTileSetups.Remove(CurrentlySelectedAutoTile);

                _serializedObject.ApplyModifiedProperties();
                _serializedObject.Update();
            }
        }

        private void AutoTileSelectionDropDown()
        {
            if (_autoTileSettings == null)
                return;

            if (CurrentlySelectedAutoTile == null)
                return;

            GenericMenu menu = new GenericMenu();
            menu.allowDuplicateNames = true;
            foreach (AutoTiles autoTile in _autoTileSettings.autoTileSetups)
            {
                string underString = autoTile.UnderTileType.ToString() == "-1" ? "Everything" :
                    autoTile.UnderTileType == 0 ? "Nothing" :
                     autoTile.UnderTileType.ToString();
                string title = autoTile.OverTileType.ToString() + ", over: " + underString;
                menu.AddItem(new GUIContent(title), CurrentlySelectedAutoTile == autoTile, () => SelectAutoTile(autoTile));
            }

            string underStringTitle = (int)CurrentlySelectedAutoTile.UnderTileType == -1 ? "Everything" :
                    CurrentlySelectedAutoTile.UnderTileType == 0 ? "Nothing" :
                     CurrentlySelectedAutoTile.UnderTileType.ToString();
            string dropDownTitle = CurrentlySelectedAutoTile != null ? 
                CurrentlySelectedAutoTile.OverTileType.ToString() + ", over: " + underStringTitle : 
                "Null";

            if (EditorGUI.DropdownButton(SelectAutoTileRect, new GUIContent(dropDownTitle), FocusType.Passive))
            {
                menu.DropDown(SelectAutoTileRect);
            }
        }
        
        private void SelectAutoTile(AutoTiles autoTile)
        {
            CurrentlySelectedAutoTileIndex = _autoTileSettings.autoTileSetups.IndexOf(autoTile);
            boundsRectDraggableArea = null;
        }

        private void DrawGridWindow()
        {
            Rect gridRect = GetGridRect();

            DrawGrid(gridRect);
            DrawSideOfGrid(gridRect.xMax + gridPadding);
        }

        private void DrawSideOfGrid(float xMin)
        {
            Rect sideRect = new Rect(WindowRect) { xMin = xMin, yMin = SelectAutoTileRect.yMax + gridPadding };
            Rect overTileTypeRect = new Rect(sideRect) { height = SLH };
            Rect underTileTypeRect = new Rect(sideRect) { yMin = overTileTypeRect.yMax + spacing, height = SLH };

            Rect overBaseSpriteRect = new Rect(sideRect) { yMin = underTileTypeRect.yMax + spacing + SLH, height = SLH };
            Rect underBaseSpriteRect = new Rect(sideRect) { yMin = overBaseSpriteRect.yMax + spacing, height = SLH };

            Rect tileOrientationRect = new Rect(sideRect) { yMin = underBaseSpriteRect.yMax + spacing + SLH, height = SLH };

            Rect normalSpriteRect = new Rect(sideRect) { yMin = tileOrientationRect.yMax + spacing + SLH, height = SLH };
            Rect outlineSpriteRect = new Rect(sideRect) { yMin = normalSpriteRect.yMax + spacing, height = SLH };


            SerializedProperty selectedAutoTileProp = CurrentlySelectedAutoTilesProp;

            if (selectedAutoTileProp == null)
                return;

            SerializedProperty overBaseSprite = selectedAutoTileProp.FindPropertyRelative(nameof(CurrentlySelectedAutoTile.OverBase));
            SerializedProperty underBaseSprite = selectedAutoTileProp.FindPropertyRelative(nameof(CurrentlySelectedAutoTile.UnderBase));

            EditorGUI.BeginChangeCheck();

            CurrentlySelectedAutoTile.OverTileType = (TileType)EditorGUI.EnumPopup(overTileTypeRect, new GUIContent("Over Tile Type"), CurrentlySelectedAutoTile.OverTileType);
            CurrentlySelectedAutoTile.UnderTileType = (TileType)EditorGUI.EnumFlagsField(underTileTypeRect, new GUIContent("Under Tile Types"), CurrentlySelectedAutoTile.UnderTileType);

            EditorGUI.ObjectField(overBaseSpriteRect, overBaseSprite);
            EditorGUI.ObjectField(underBaseSpriteRect, underBaseSprite);
            EditorGUI.indentLevel++;
            DrawTileArrangementChoice(tileOrientationRect);

            DrawBaseTileObject(normalSpriteRect, selectedAutoTileProp);
            DrawOutlineTileObject(outlineSpriteRect, selectedAutoTileProp);

            float yMin = DrawBoundEditorReturnsYMin(sideRect, outlineSpriteRect.yMax + spacing + SLH);

            Rect drawOrderRect = new Rect(sideRect) { yMin = yMin + spacing};
            DrawDrawOrderList(drawOrderRect);
            EditorGUI.indentLevel--;
            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
            }
        }

        private float DrawBoundEditorReturnsYMin(Rect sideRect, float yMin)
        {
            AutoTileSpriteAndWalkability autoTile = SelectedAutoTileSpriteAndWalkability;
            SerializedProperty autoTileProp = CurrentlySelectedAutoTileSpriteAndWalkabilityProperty;

            if (autoTile == null || autoTileProp == null)
                return 0;
             
            SerializedProperty boundsProp = autoTileProp.FindPropertyRelative(nameof(SelectedAutoTileSpriteAndWalkability.walkabilityBounds));

            Rect boundsRect = new Rect(sideRect) { yMin = yMin, height = EditorGUI.GetPropertyHeight(boundsProp) };

            EditorGUI.PropertyField(boundsRect, boundsProp);

            return boundsRect.yMax;
        }

        private void DrawDrawOrderList(Rect listRect)
        {
            SerializedProperty drawOrderSerializedProperty = _serializedObject.FindProperty(nameof(_autoTileSettings.drawOrderForType));

            if(drawOrderSerializedProperty.arraySize != Enum.GetNames(typeof(TileType)).Length)
            {
                HandleAmountOfDrawOrderTileTypeInList(drawOrderSerializedProperty);
            }

            if (drawOrderReordableFoldoutList == null)
            {
                drawOrderReordableFoldoutList = new ReorderableList(_serializedObject, drawOrderSerializedProperty, true, true, false, false);
                drawOrderReordableFoldoutList.elementHeightCallback = index =>
                {
                    return EditorGUIUtility.singleLineHeight;
                };

                drawOrderReordableFoldoutList.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    SerializedProperty element = drawOrderSerializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.LabelField(rect, ((TileType)element.enumValueFlag).ToString());
                };
            }

            drawOrderReordableFoldoutList.DoList(listRect);
        }

        private void HandleAmountOfDrawOrderTileTypeInList(SerializedProperty drawOrderSerializedProperty)
        {
            drawOrderSerializedProperty.ClearArray();

            string[] tileTypeNames = Enum.GetNames(typeof(TileType));

            for (int i = 0; i < tileTypeNames.Length; i++)
            {
                drawOrderSerializedProperty.InsertArrayElementAtIndex(i);
                SerializedProperty element = drawOrderSerializedProperty.GetArrayElementAtIndex(i);

                element.enumValueFlag = (int)Enum.Parse<TileType>(tileTypeNames[i]);
            }
        }

        private void DrawBaseTileObject(Rect rect, SerializedProperty prop) => DrawTileSpriteObject(rect, prop, false);
        private void DrawOutlineTileObject(Rect rect, SerializedProperty prop) => DrawTileSpriteObject(rect, prop, true);

        private void DrawTileSpriteObject(Rect rect, SerializedProperty prop, bool outline)
        {
            string tileSpriteType = outline ? "OutlineSprite" : "BaseSprite";

            SerializedProperty autoTileProp = CurrentlySelectedAutoTileSpriteAndWalkabilityProperty;

            if (autoTileProp == null) return;

            SerializedProperty spriteProp = autoTileProp.FindPropertyRelative(tileSpriteType);

            EditorGUI.ObjectField(rect, spriteProp, new GUIContent(tileSpriteType.AddSpacesBeforeCapsRegex()));
        }

        private void DrawTileArrangementChoice(Rect rect)
        {
            GenericMenu menu = new GenericMenu();

            TileOrientations[] tileOrientations = Enum.GetNames(typeof(TileOrientations)).Select(x => (TileOrientations)Enum.Parse(typeof(TileOrientations), x)).ToArray();

            foreach (TileOrientations ori in tileOrientations)
            {
                menu.AddItem(new GUIContent(ori.ToString().AddSpacesBeforeCapsRegex()), _selectedTileOrientation == ori, () => SelectTileOrientation(ori));
            }

            rect = EditorGUI.PrefixLabel(rect, new GUIContent("Tile Orientation"));

            if (EditorGUI.DropdownButton(rect, new GUIContent(_selectedTileOrientation.ToString().AddSpacesBeforeCapsRegex()), FocusType.Passive))
            {
                menu.DropDown(rect);
            }
        }

        private void SelectTileOrientation(TileOrientations tileOrientation)
        {
            _selectedTileOrientation = tileOrientation;
            boundsRectDraggableArea = null;
        }

        private void DrawGrid(Rect gridRect)
        {
            float cellSize = (gridRect.height - 2f * gridSpacing) / 3f;

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    float cellXPos = gridRect.xMin + x * (cellSize + gridSpacing);
                    float cellYPos = gridRect.yMin + y * (cellSize + gridSpacing);

                    Rect squareRect = new Rect(gridRect) { position = new Vector2(cellXPos, cellYPos), size = new Vector2(cellSize, cellSize) };

                    int gridY = 2 - y;

                    Grid3x3<bool?> grid = _selectedTileOrientation switch
                    {
                        TileOrientations.StraightTopCenter => AutoTiles.StraightTopCenterGrid(),
                        TileOrientations.StraightCenterLeft => AutoTiles.StraightCenterLeftGrid(),
                        TileOrientations.StraightCenterRight => AutoTiles.StraightCenterRightGrid(),
                        TileOrientations.StraightBottomCenter => AutoTiles.StraightBottomCenterGrid(),
                        TileOrientations.SmallCornerTopLeft => AutoTiles.SmallCornerTopLeftGrid(),
                        TileOrientations.SmallCornerTopRight => AutoTiles.SmallCornerTopRightGrid(),
                        TileOrientations.SmallCornerBottomLeft => AutoTiles.SmallCornerBottomLeftGrid(),
                        TileOrientations.SmallCornerBottomRight => AutoTiles.SmallCornerBottomRightGrid(),
                        TileOrientations.BigCornerTopAndLeft => AutoTiles.BigCornerTopLeftGrid(),
                        TileOrientations.BigCornerTopAndRight => AutoTiles.BigCornerTopRightGrid(),
                        TileOrientations.BigCornerBottomAndLeft => AutoTiles.BigCornerBottomLeftGrid(),
                        TileOrientations.BigCornerBottomAndRight => AutoTiles.BigCornerBottomRightGrid(),
                        _ => new Grid3x3<bool?>()
                    };

                    bool overBase = grid[x, gridY] ?? false;

                    Grid3x3<bool> g = new Grid3x3<bool>();

                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            g[i, j] = grid[i, j].HasValue && grid[i, j].Value;
                        }
                    }

                    DrawCell(squareRect, overBase, g.ShiftContent(1 - x, y - 1));
                }
            }

            float centerCellXPos = gridRect.xMin + (cellSize + gridSpacing);
            float centerCellYPos = gridRect.yMin + (cellSize + gridSpacing);

            float topRightCellXPos = gridRect.xMin + 2 * (cellSize + gridSpacing) + spacing;
            float topRightCellYPos = gridRect.yMin + spacing;

            Rect drawWalkabilityBoundsButtonRect = new Rect(gridRect) 
            { 
                position = new Vector2(topRightCellXPos, topRightCellYPos),  
                size = new Vector2(cellSize - 2 * spacing, EditorGUIUtility.singleLineHeight) 
            };
            DrawWalkabilityDrawButton(drawWalkabilityBoundsButtonRect);

            Rect centerCellRect = new Rect(gridRect) { position = new Vector2(centerCellXPos, centerCellYPos), size = new Vector2(cellSize, cellSize) };
            DrawWalkableBounds(centerCellRect);
        }

        private bool drawWalkabilityBounds = false;

        private void DrawWalkabilityDrawButton(Rect rect)
        {
            drawWalkabilityBounds = GUI.Toggle(rect, drawWalkabilityBounds, new GUIContent("Draw bounds"), EditorStyles.toolbarButton);
        }

        private void DrawWalkableBounds(Rect centerCellRect)
        {
            if (!drawWalkabilityBounds)
                return;

            SerializedProperty autoTileProp = CurrentlySelectedAutoTileSpriteAndWalkabilityProperty;
            AutoTileSpriteAndWalkability autoTile = SelectedAutoTileSpriteAndWalkability;

            if (autoTile == null || autoTileProp == null)
                return;

            if (boundsRectDraggableArea == null)
            {
                boundsRectDraggableArea = new DraggableArea<AutoTileWalkability>(autoTile.walkabilityBounds)
                {
                    limitXPositionToRect = true,
                    limitYPositionToRect = true,
                    backgroundColorInner = Color.red.SetAlpha(0.3f),
                    backgroundColorOuter = Color.red,
                    defaultInnerColor = Color.green.SetAlpha(0.3f),
                    defaultOuterColor = Color.green,
                    defaultIgnoreOtherDraggableRects = true,
                };
            }

            _serializedObject.Update();

            boundsRectDraggableArea.AreaRect = centerCellRect;
            boundsRectDraggableArea.HandleDraggableRects(Event.current);

            _serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawCell(Rect rect, bool overTile, Grid3x3<bool> grid)
        {
            if(CurrentlySelectedAutoTile == null)
            {
                EditorGUI.DrawPreviewTexture(rect, Texture2D.redTexture);
                return;
            }

            if (overTile)
            {
                if(CurrentlySelectedAutoTile.OverBase != null)
                {
                    CurrentlySelectedAutoTile.OverBase.DrawSpriteInRect(rect);
                }
                else
                {
                    EditorGUI.DrawPreviewTexture(rect, Texture2D.grayTexture);
                }
            }
            else
            {
                if (CurrentlySelectedAutoTile.UnderBase != null)
                {
                    CurrentlySelectedAutoTile.UnderBase.DrawSpriteInRect(rect);
                }
                else
                {
                    EditorGUI.DrawPreviewTexture(rect, Texture2D.blackTexture);
                }

                List<Sprite> normalSprites = CurrentlySelectedAutoTile.GetNormalSprites(grid).Where(x => x != null).ToList();
                List<Sprite> outlineSprites = CurrentlySelectedAutoTile.GetOutlineSprites(grid).Where(x => x != null).ToList();

                foreach (Sprite sprite in normalSprites)
                {
                    sprite.DrawSpriteInRect(rect);
                }

                foreach (Sprite sprite in outlineSprites)
                {
                    sprite.DrawSpriteInRect(rect);
                }
            }
        }
        
    }

}



