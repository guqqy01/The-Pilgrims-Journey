using Codice.Client.Common.GameUI;
using LocalMap;
using System;
using System.Security.Policy;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
public class TileTab : IEditorMenuToolbarTab
{
    private SerializedProperty _baseTileListProperty;

    public TileTab(string name, SerializedObject serializedObject, string property, string header)
    {
        Name = name;
        _serializedObject = serializedObject;

        _baseTileListProperty = serializedObject.FindProperty(property).FindPropertyRelative("weightedList");
        _tilesList = new ReorderableFoldoutList(serializedObject, _baseTileListProperty, (index) => HelpDrawLocalMapTileEditor.GetTileHeight(_baseTileListProperty, index), HelpDrawLocalMapTileEditor.DrawTile, header);
    }

    protected readonly SerializedObject _serializedObject;

    public string Name { get; set; }

    private ReorderableFoldoutList _tilesList;

    public virtual void DrawTab(Rect rect)
    {
        _tilesList.DoList(rect);
    }

    public virtual float GetHeight(Rect rect)
    {
        return _tilesList.Height;
    }
}

public static class HelpDrawLocalMapTileEditor
{
    public static float GetTileHeight(SerializedProperty baseTileWeightedListProperty, int index)
    {
        SerializedProperty baseTileProperty = baseTileWeightedListProperty.GetArrayElementAtIndex(index);

        SerializedProperty tileTypeAndSpriteProperty = baseTileProperty.FindPropertyRelative("item");
        SerializedProperty weightCurveProperty = baseTileProperty.FindPropertyRelative("weightCurve");

        return EditorGUI.GetPropertyHeight(tileTypeAndSpriteProperty) + EditorGUI.GetPropertyHeight(weightCurveProperty) + EditorGUIUtility.singleLineHeight + 2f;
    }

    public static void DrawTile(Rect rect, SerializedProperty baseTileProperty, int index)
    {
        SerializedProperty tileTypeAndSpriteProperty = baseTileProperty.FindPropertyRelative("item");
        SerializedProperty weightCurveProperty = baseTileProperty.FindPropertyRelative("weightCurve");

        Rect tileTypeAndSpriteRect = new Rect(rect) { height = EditorGUI.GetPropertyHeight(tileTypeAndSpriteProperty) };
        Rect weightRect = new Rect(rect) { yMin = tileTypeAndSpriteRect.yMax + 2f, height = EditorGUI.GetPropertyHeight(weightCurveProperty) };

        EditorGUI.PropertyField(tileTypeAndSpriteRect, tileTypeAndSpriteProperty);
        EditorGUI.PropertyField(weightRect, weightCurveProperty, new GUIContent("Weight"));
    }
}


[CustomPropertyDrawer(typeof(TileTypeAndVisual))]
public class TileTypeAndVisualPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float returnHeight = EditorGUIUtility.singleLineHeight * 2f + 2f;

        SerializedProperty tileVisualTypeProperty = property.FindPropertyRelative(nameof(TileTypeAndVisual.TileVisualType));

        switch ((TileVisualType)tileVisualTypeProperty.enumValueFlag)
        {
            case TileVisualType.SingleSprite:
                returnHeight += EditorGUIUtility.singleLineHeight * 2f + 4f;
                break;
            case TileVisualType.SpriteAnimation:
                SerializedProperty spriteAnimationProperty = property.FindPropertyRelative(nameof(TileTypeAndVisual.SpriteAnimation));
                returnHeight += EditorGUI.GetPropertyHeight(spriteAnimationProperty);
                break;
        }

        return returnHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty tileTypeProperty = property.FindPropertyRelative(nameof(TileTypeAndVisual.TileType));
        SerializedProperty tileVisualTypeProperty = property.FindPropertyRelative(nameof(TileTypeAndVisual.TileVisualType));

        Rect tileTypeRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
        Rect tileVisualTypeRect = new Rect(position) { yMin = tileTypeRect.yMax + 2f, height = EditorGUIUtility.singleLineHeight };
        tileTypeProperty.enumValueFlag = (int)(TileType)EditorGUI.EnumPopup(tileTypeRect, new GUIContent(tileTypeProperty.displayName), (TileType)tileTypeProperty.enumValueFlag);
        tileVisualTypeProperty.enumValueIndex = EditorGUI.Popup(tileVisualTypeRect, tileVisualTypeProperty.displayName, tileVisualTypeProperty.enumValueIndex, tileVisualTypeProperty.enumDisplayNames);

        Rect remainingRect = new Rect(position) { yMin = tileVisualTypeRect.yMax + 2f };

        switch ((TileVisualType)tileVisualTypeProperty.enumValueFlag)
        {
            case TileVisualType.SingleSprite:
                DrawSingleSpriteTileVisual(remainingRect, property);
                break;
            case TileVisualType.SpriteAnimation:
                DrawSpriteAnimationVisual(remainingRect, property);
                break;
        }
    }

    private void DrawSingleSpriteTileVisual(Rect rect, SerializedProperty property)
    {
        SerializedProperty spriteProperty = property.FindPropertyRelative(nameof(TileTypeAndVisual.Sprite));
        Sprite sprite = spriteProperty.objectReferenceValue as Sprite;

        float spriteLength = rect.height;
        Rect spriteRect = new Rect(rect) { height = spriteLength, width = spriteLength };
        Rect spriteObjectPickerRect = new Rect(rect) { xMin = spriteRect.xMax + 2f, yMin = rect.yMin + 2f, height = EditorGUIUtility.singleLineHeight };

        sprite.DrawSpriteInRect(spriteRect);
        EditorGUI.ObjectField(spriteObjectPickerRect, spriteProperty, new GUIContent("Tile"));
    }

    private void DrawSpriteAnimationVisual(Rect rect, SerializedProperty property)
    {
        SerializedProperty spriteAnimationProperty = property.FindPropertyRelative(nameof(TileTypeAndVisual.SpriteAnimation));
        EditorGUI.PropertyField(rect, spriteAnimationProperty, true);
    }
}

public class PathSettingsTileTab : TileTab
{
    public PathSettingsTileTab(string name, SerializedObject serializedObject, string property, string header) : base(name, serializedObject, property, header)
    {
    }

    public override void DrawTab(Rect rect)
    {
        SerializedProperty pathWidthRangeProp = _serializedObject.FindProperty("pathWidthRange");
        SerializedProperty pathNoiseScaleProp = _serializedObject.FindProperty("pathNoiseScale");
        SerializedProperty pathNoiseTilingProp = _serializedObject.FindProperty("pathNoiseTiling");
        SerializedProperty cullingAmountProp = _serializedObject.FindProperty("cullingAmount");

        Rect pathWidthRangeRect = new Rect(rect) { yMin = rect.yMin + 2f,  height = 2f * EditorGUIUtility.singleLineHeight + 2f};
        Rect pathNoiseScaleRect = new Rect(rect) { yMin = pathWidthRangeRect.yMax + 2f, height = 2f * EditorGUIUtility.singleLineHeight + 2f };
        Rect pathNoiseTilingRect = new Rect(rect) { yMin = pathNoiseScaleRect.yMax + 2f, height = 2f * EditorGUIUtility.singleLineHeight + 2f };
        Rect cullingAmountRect = new Rect(rect) { yMin = pathNoiseTilingRect.yMax + 2f, height = EditorGUIUtility.singleLineHeight };

        EditorGUI.BeginChangeCheck();

        EditorGUI.PropertyField(pathWidthRangeRect, pathWidthRangeProp, true);
        EditorGUI.PropertyField(pathNoiseScaleRect, pathNoiseScaleProp, true);
        EditorGUI.PropertyField(pathNoiseTilingRect, pathNoiseTilingProp, true);
        EditorGUI.PropertyField(cullingAmountRect, cullingAmountProp, true);

        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
        }

        Rect listRect = new Rect(rect) { yMin = cullingAmountRect.yMax + 2f };

        base.DrawTab(listRect);
    }

    public override float GetHeight(Rect rect)
    {
        return base.GetHeight(rect) + EditorGUIUtility.singleLineHeight * 7f + 2f * 8f;
    }
}
