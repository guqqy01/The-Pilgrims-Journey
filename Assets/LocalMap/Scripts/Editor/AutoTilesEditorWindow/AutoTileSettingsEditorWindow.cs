using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEditorInternal;

namespace LocalMap.Editor
{
    [Serializable]
    public class AutoTileSettingsEditorWindow : EditorWindow
    {
        [SerializeField]
        private AutoTileSettings _autoTileSettings;

        private SerializedObject _serializedObject;

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

        private AutoTile CurrentlySelectedAutoTile
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
                _autoTileSettings.autoTileSetups.Add(new AutoTile());

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
            foreach (AutoTile autoTile in _autoTileSettings.autoTileSetups)
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

        private void SelectAutoTile(AutoTile autoTile)
        {
            CurrentlySelectedAutoTileIndex = _autoTileSettings.autoTileSetups.IndexOf(autoTile);
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
            Rect overBaseSpriteRect = new Rect(sideRect) { yMin = underTileTypeRect.yMax + spacing, height = SLH };
            Rect underBaseSpriteRect = new Rect(sideRect) { yMin = overBaseSpriteRect.yMax + spacing, height = SLH };
            Rect tileOrientationRect = new Rect(sideRect) { yMin = underBaseSpriteRect.yMax + spacing, height = SLH };
            Rect normalSpriteRect = new Rect(sideRect) { yMin = tileOrientationRect.yMax + spacing, height = SLH };
            Rect outlineSpriteRect = new Rect(sideRect) { yMin = normalSpriteRect.yMax + spacing, height = SLH };

            Rect drawOrderRect = new Rect(sideRect) { yMin = outlineSpriteRect.yMax + spacing };

            SerializedProperty selectedAutoTileProp = _serializedObject.FindProperty("autoTileSetups").GetArrayElementAtIndex(CurrentlySelectedAutoTileIndex);

            if (selectedAutoTileProp == null)
                return;

            SerializedProperty overBaseSprite = selectedAutoTileProp.FindPropertyRelative("OverBase");
            SerializedProperty underBaseSprite = selectedAutoTileProp.FindPropertyRelative("UnderBase");

            EditorGUI.BeginChangeCheck();

            CurrentlySelectedAutoTile.OverTileType = (TileType)EditorGUI.EnumPopup(overTileTypeRect, new GUIContent("Over Tile Type"), CurrentlySelectedAutoTile.OverTileType);
            CurrentlySelectedAutoTile.UnderTileType = (TileType)EditorGUI.EnumFlagsField(underTileTypeRect, new GUIContent("Under Tile Types"), CurrentlySelectedAutoTile.UnderTileType);

            EditorGUI.ObjectField(overBaseSpriteRect, overBaseSprite);
            EditorGUI.ObjectField(underBaseSpriteRect, underBaseSprite);
            DrawTileArrangementChoice(tileOrientationRect);
            DrawOutlineTileObject(outlineSpriteRect, selectedAutoTileProp);
            DrawNormalTileObject(normalSpriteRect, selectedAutoTileProp);
            DrawDrawOrderList(drawOrderRect);

            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
            }
        }

        private ReorderableList drawOrderReordableFoldoutList;

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

        private void DrawNormalTileObject(Rect rect, SerializedProperty selectedAutoTileProp)
        {
            SerializedProperty tileSpriteProp = null;

            switch (_selectedTileOrientation)
            {
                case TileOrientations.StraightTopCenter:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("TopCenter");
                    break;
                case TileOrientations.StraightCenterLeft:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("CenterLeft");
                    break;
                case TileOrientations.StraightCenterRight:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("CenterRight");
                    break;
                case TileOrientations.StraightBottomCenter:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("BottomCenter");
                    break;
                case TileOrientations.SmallCornerTopRight:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("TopRight");
                    break;
                case TileOrientations.SmallCornerTopLeft:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("TopLeft");
                    break;
                case TileOrientations.SmallCornerBottomLeft:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("BottomLeft");
                    break;
                case TileOrientations.SmallCornerBottomRight:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("BottomRight");
                    break;
                case TileOrientations.BigCornerTopAndLeft:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("TopAndLeft");
                    break;
                case TileOrientations.BigCornerTopAndRight:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("TopAndRight");
                    break;
                case TileOrientations.BigCornerBottomAndLeft:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("BottomAndLeft");
                    break;
                case TileOrientations.BigCornerBottomAndRight:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("BottomAndRight");
                    break;
            }

            EditorGUI.ObjectField(rect, tileSpriteProp);
        }

        private void DrawOutlineTileObject(Rect rect, SerializedProperty selectedAutoTileProp)
        {
            SerializedProperty tileSpriteProp = null;

            switch (_selectedTileOrientation)
            {
                case TileOrientations.StraightTopCenter:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("TopCenterOutline");
                    break;
                case TileOrientations.StraightCenterLeft:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("CenterLeftOutline");
                    break;
                case TileOrientations.StraightCenterRight:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("CenterRightOutline");
                    break;
                case TileOrientations.StraightBottomCenter:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("BottomCenterOutline");
                    break;
                case TileOrientations.SmallCornerTopRight:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("TopRightOutline");
                    break;
                case TileOrientations.SmallCornerTopLeft:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("TopLeftOutline");
                    break;
                case TileOrientations.SmallCornerBottomLeft:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("BottomLeftOutline");
                    break;
                case TileOrientations.SmallCornerBottomRight:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("BottomRightOutline");
                    break;
                case TileOrientations.BigCornerTopAndLeft:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("TopAndLeftOutline");
                    break;
                case TileOrientations.BigCornerTopAndRight:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("TopAndRightOutline");
                    break;
                case TileOrientations.BigCornerBottomAndLeft:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("BottomAndLeftOutline");
                    break;
                case TileOrientations.BigCornerBottomAndRight:
                    tileSpriteProp = selectedAutoTileProp.FindPropertyRelative("BottomAndRightOutline");
                    break;
            }

            EditorGUI.ObjectField(rect, tileSpriteProp);
        }

        private void DrawTileArrangementChoice(Rect rect)
        {
            GenericMenu menu = new GenericMenu();

            TileOrientations[] tileOrientations = Enum.GetNames(typeof(TileOrientations)).Select(x => (TileOrientations)Enum.Parse(typeof(TileOrientations), x)).ToArray();

            foreach (TileOrientations ori in tileOrientations)
            {
                menu.AddItem(new GUIContent(ori.ToString().AddSpacesBeforeCapsRegex()), _selectedTileOrientation == ori, () => _selectedTileOrientation = ori);
            }

            if (EditorGUI.DropdownButton(rect, new GUIContent(_selectedTileOrientation.ToString().AddSpacesBeforeCapsRegex()), FocusType.Passive))
            {
                menu.DropDown(rect);
            }
        }

        private void DrawGrid(Rect gridRect)
        {
            float cellSize = (gridRect.height - 2f * gridSpacing) / 3f;

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    float xPos = gridRect.xMin + x * (cellSize + gridSpacing);
                    float yPos = gridRect.yMin + y * (cellSize + gridSpacing);

                    Rect squareRect = new Rect(gridRect) { position = new Vector2(xPos, yPos), size = new Vector2(cellSize, cellSize) };

                    int gridY = 2 - y;

                    Grid3x3<bool?> grid = _selectedTileOrientation switch
                    {
                        TileOrientations.StraightTopCenter => AutoTile.StraightTopCenterGrid(),
                        TileOrientations.StraightCenterLeft => AutoTile.StraightCenterLeftGrid(),
                        TileOrientations.StraightCenterRight => AutoTile.StraightCenterRightGrid(),
                        TileOrientations.StraightBottomCenter => AutoTile.StraightBottomCenterGrid(),
                        TileOrientations.SmallCornerTopLeft => AutoTile.SmallCornerTopLeftGrid(),
                        TileOrientations.SmallCornerTopRight => AutoTile.SmallCornerTopRightGrid(),
                        TileOrientations.SmallCornerBottomLeft => AutoTile.SmallCornerBottomLeftGrid(),
                        TileOrientations.SmallCornerBottomRight => AutoTile.SmallCornerBottomRightGrid(),
                        TileOrientations.BigCornerTopAndLeft => AutoTile.BigCornerTopLeftGrid(),
                        TileOrientations.BigCornerTopAndRight => AutoTile.BigCornerTopRightGrid(),
                        TileOrientations.BigCornerBottomAndLeft => AutoTile.BigCornerBottomLeftGrid(),
                        TileOrientations.BigCornerBottomAndRight => AutoTile.BigCornerBottomRightGrid(),
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



