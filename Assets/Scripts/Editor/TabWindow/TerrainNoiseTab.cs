using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;
public class TerrainNoiseTab : IEditorMenuToolbarTab, IDisposable
{
    private readonly SerializedObject _serializedObject;
    private readonly SerializedProperty _terrainProperty;

    public string Name => "Terrain Noise";

    private ReordableFoldoutList _terrainNoiseItemList;
    private readonly List<ReordableFoldoutList> _terrainNoiseSpriteLists = new List<ReordableFoldoutList>();

    private const float FieldPadding = 2f;
    private const float SectionPadding = 4f;
    private const float HeaderFieldCount = 4f;
    private const float TilePreviewHeight = 64f;

    private bool _needsHeightRefresh = true;

    public Action OnRequestRepaint;

    public TerrainNoiseTab(SerializedObject serializedObject, SerializedProperty terrainProperty)
    {
        _serializedObject = serializedObject;
        _terrainProperty = terrainProperty;

        SerializedProperty itemsProperty = terrainProperty.FindPropertyRelative("items");

        _terrainNoiseItemList = new ReordableFoldoutList(
            _serializedObject,
            itemsProperty,
            (index) => GetTerrainNoiseSpriteElementHeight(index),
            DrawTerrainNoiseItemElements,
            "Terrain Noise");
    }

    public void DrawTab(Rect rect)
    {
        if (_terrainProperty == null)
            return;

        SerializedProperty nameProperty = _terrainProperty.FindPropertyRelative("name");
        SerializedProperty priorityProperty = _terrainProperty.FindPropertyRelative("priority");
        SerializedProperty scaleProperty = _terrainProperty.FindPropertyRelative("scale");
        SerializedProperty terrainNoiseItemsProperty = _terrainProperty.FindPropertyRelative("items");

        Rect nameRect = new Rect(rect) { yMin = rect.yMin + FieldPadding, height = EditorGUIUtility.singleLineHeight };
        Rect priorityRect = new Rect(rect) { yMin = nameRect.yMax + FieldPadding, height = EditorGUIUtility.singleLineHeight };
        Rect scaleRect = new Rect(rect) { yMin = priorityRect.yMax + FieldPadding, height = EditorGUIUtility.singleLineHeight * 2f + FieldPadding };
        Rect itemsRect = new Rect(rect) { yMin = scaleRect.yMax + FieldPadding, xMax = rect.xMax };

        _serializedObject.Update();
        EditorGUI.PropertyField(nameRect, nameProperty);
        EditorGUI.PropertyField(priorityRect, priorityProperty);
        EditorGUI.PropertyField(scaleRect, scaleProperty);
        _serializedObject.ApplyModifiedProperties();

        if (NeedsRebuild(_terrainNoiseItemList, terrainNoiseItemsProperty))
        {
            _terrainNoiseItemList = new ReordableFoldoutList(
                _serializedObject,
                terrainNoiseItemsProperty,
                (index) => GetTerrainNoiseSpriteElementHeight(index),
                DrawTerrainNoiseItemElements,
                "Terrain Noise");
        }

        _terrainNoiseItemList.DoList(itemsRect);

        if (_needsHeightRefresh)
        {
            _needsHeightRefresh = false;
            OnRequestRepaint?.Invoke();
        }
    }

    public float GetHeight(Rect rect)
    {
        return Mathf.Max(_terrainNoiseItemList.Height + HeaderFieldCount * EditorGUIUtility.singleLineHeight + FieldPadding * 5f, rect.height);
    }

    private float GetTerrainNoiseSpriteElementHeight(int index)
    {
        if (_terrainNoiseSpriteLists.Count <= index)
            return 0;

        return _terrainNoiseSpriteLists[index].Height + EditorGUIUtility.singleLineHeight * 3f + SectionPadding;
    }

    private void DrawTerrainNoiseItemElements(Rect rect, SerializedProperty terrainNoiseItemProperty, int terrainNoiseIndex)
    {
        SerializedProperty perlinNoiseRangeProp = terrainNoiseItemProperty.FindPropertyRelative("perlinNoiseRange");
        SerializedProperty spriteListProp = terrainNoiseItemProperty.FindPropertyRelative("list").FindPropertyRelative("weightedList");

        Rect perlinNoiseRangeRect = new Rect(rect) { height = 2f * EditorGUIUtility.singleLineHeight };
        Rect spriteListRect = new Rect(rect) { yMin = perlinNoiseRangeRect.yMax + SectionPadding };

        _serializedObject.Update();
        EditorGUI.PropertyField(perlinNoiseRangeRect, perlinNoiseRangeProp);
        _serializedObject.ApplyModifiedProperties();

        SetupTerrainNoiseSpriteLists(spriteListProp, terrainNoiseIndex, TilePreviewHeight);
        _terrainNoiseSpriteLists[terrainNoiseIndex].DoList(spriteListRect);
    }

    private bool NeedsRebuild(ReordableFoldoutList list, SerializedProperty property)
    {
        return list == null
        || list._listProperty == null
        || !list._listProperty.EqualContents(property);
    }

    private void SetupTerrainNoiseSpriteLists(SerializedProperty terrainNoiseItemProperty, int terrainNoiseIndex, float height)
    {
        while (_terrainNoiseSpriteLists.Count <= terrainNoiseIndex)
            _terrainNoiseSpriteLists.Add(null);

        if(NeedsRebuild(_terrainNoiseSpriteLists[terrainNoiseIndex], terrainNoiseItemProperty))
        {
            _terrainNoiseSpriteLists[terrainNoiseIndex] = new ReordableFoldoutList(
                _serializedObject,
                terrainNoiseItemProperty,
                (index) => height,
                DrawTile,
                "Terrain Sprites");
        }
    }

    private void DrawTile(Rect rect, SerializedProperty baseTileProperty, int index)
    {
        float spriteLength = rect.height - FieldPadding;
        Rect spriteRect = new Rect(rect) { yMin = rect.yMin + FieldPadding, height = spriteLength, width = spriteLength };
        Rect spriteObjectPickerRect = new Rect(rect) { xMin = spriteRect.xMax, yMin = rect.yMin + FieldPadding, height = EditorGUIUtility.singleLineHeight };
        Rect weightRect = new Rect(rect) { xMin = spriteRect.xMax, yMin = spriteObjectPickerRect.yMax + FieldPadding, height = EditorGUIUtility.singleLineHeight };

        SerializedProperty spriteProperty = baseTileProperty.FindPropertyRelative("item");
        SerializedProperty weightCurveProperty = baseTileProperty.FindPropertyRelative("weightCurve");

        Sprite sprite = spriteProperty.objectReferenceValue as Sprite;
        sprite.DrawSpriteInRect(spriteRect);

        EditorGUI.ObjectField(spriteObjectPickerRect, spriteProperty, new GUIContent("Tile"));
        EditorGUI.PropertyField(weightRect, weightCurveProperty);
    }

    public void Dispose()
    {
        OnRequestRepaint = null;
    }
}