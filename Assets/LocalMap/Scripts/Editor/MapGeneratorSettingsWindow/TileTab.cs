using UnityEngine;
using UnityEditor;
using Codice.Client.Common.GameUI;
using LocalMap;
using System;
public class TileTab : IEditorMenuToolbarTab
{
    private const float itemHeight = 80f;

    public TileTab(string name, SerializedObject serializedObject, string property, string header)
    {
        Name = name;
        _serializedObject = serializedObject;

        SerializedProperty baseTilesProperty = serializedObject.FindProperty(property).FindPropertyRelative("weightedList");
        _tilesList = new ReorderableFoldoutList(serializedObject, baseTilesProperty, (index) => itemHeight, DrawTile, header);
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

    private void DrawTile(Rect rect, SerializedProperty baseTileProperty, int index)
    {
        float spriteLength = rect.height - 2;
        Rect spriteRect = new Rect(rect) { yMin = rect.yMin + 2, height = spriteLength, width = spriteLength };
        Rect spriteObjectPickerRect = new Rect(rect) { xMin = spriteRect.xMax, yMin = rect.yMin + 2f, height = EditorGUIUtility.singleLineHeight };
        Rect tileTypeRect = new Rect(rect) { xMin = spriteRect.xMax, yMin = spriteObjectPickerRect.yMax + 2f, height = EditorGUIUtility.singleLineHeight };
        Rect weightRect = new Rect(rect) { xMin = spriteRect.xMax, yMin = tileTypeRect.yMax + 2f, height = EditorGUIUtility.singleLineHeight };

        SerializedProperty tileTypeAndSpriteProperty = baseTileProperty.FindPropertyRelative("item");
        SerializedProperty weightCurveProperty = baseTileProperty.FindPropertyRelative("weightCurve");

        SerializedProperty tileTypeProperty = tileTypeAndSpriteProperty.FindPropertyRelative("TileType");
        SerializedProperty spriteProperty = tileTypeAndSpriteProperty.FindPropertyRelative("Sprite");

        Sprite sprite = spriteProperty.objectReferenceValue as Sprite;

        sprite.DrawSpriteInRect(spriteRect);

        EditorGUI.ObjectField(spriteObjectPickerRect, spriteProperty, new GUIContent("Tile"));

        tileTypeProperty.enumValueFlag = (int)(TileType)EditorGUI.EnumPopup(tileTypeRect, new GUIContent(tileTypeProperty.displayName), (TileType)tileTypeProperty.enumValueFlag);

        EditorGUI.PropertyField(weightRect, weightCurveProperty);
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
