using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

public class EditorMenuScrollableToolbarWindow : IDisposable
{
    protected readonly SerializedObject _serializedObject;
    private readonly string _collectionPropertyName;

    public readonly List<IEditorMenuToolbarTab> Tabs = new List<IEditorMenuToolbarTab>();

    public float ToolbarHeight = 2f * EditorGUIUtility.singleLineHeight;

    private readonly List<Vector2> _scrollPositions = new List<Vector2>();

    private const float _scrollHandleWidth = 15f;

    private ScrollableToolbar _scrollableToolbar;

    public Action OnTabChange;

    #region Init

    public EditorMenuScrollableToolbarWindow(SerializedObject serializedObject, string collectionPropertyName, List<IEditorMenuToolbarTab> tabs = null)
    {
        _collectionPropertyName = collectionPropertyName;
        _serializedObject = serializedObject;

        Tabs.Clear();
        if(tabs != null)
            Tabs.AddRange(tabs);
    }

    public EditorMenuScrollableToolbarWindow(SerializedObject serializedObject, string collectionPropertyName, float toolbarHeight, List<IEditorMenuToolbarTab> tabs)
    {
        _collectionPropertyName = collectionPropertyName;
        _serializedObject = serializedObject;

        ToolbarHeight = toolbarHeight;

        Tabs.Clear();
        if (tabs != null)
            Tabs.AddRange(tabs);
    }

    public EditorMenuScrollableToolbarWindow(SerializedObject serializedObject, string collectionPropertyName, float toolbarHeight, params IEditorMenuToolbarTab[] tabs)
    {
        _collectionPropertyName = collectionPropertyName;
        _serializedObject = serializedObject;

        ToolbarHeight = toolbarHeight;

        Tabs.Clear();
        if (tabs != null)
            Tabs.AddRange(tabs);
    }

    public EditorMenuScrollableToolbarWindow(SerializedObject serializedObject, string collectionPropertyName, params IEditorMenuToolbarTab[] tabs)
    {
        _collectionPropertyName = collectionPropertyName;
        _serializedObject = serializedObject;

        Tabs.Clear();
        if (tabs != null)
            Tabs.AddRange(tabs);
    }

    #endregion

    private void HandleScrollableToolbar(Rect toolbarRect, SerializedProperty collection)
    {
        if (_scrollableToolbar == null)
            _scrollableToolbar = new ScrollableToolbar();

        string[] titles = new string[collection.arraySize];

        for (int i = 0; i < collection.arraySize; i++)
        {
            titles[i] = collection.GetArrayElementAtIndex(i).displayName;
        }

        int previousIndex = _scrollableToolbar.SelectedIndex;
        _scrollableToolbar.HandleScrollableToolbar(toolbarRect, titles);

        if (_scrollableToolbar.SelectedIndex != previousIndex)
            OnTabChange?.Invoke();
    }

    private void DrawAddRemoveButtons(Rect rect, Rect toolbarRect, SerializedProperty collection)
    {
        Rect addButtonRect = new Rect(toolbarRect) { xMin = toolbarRect.xMax + 10f, xMax = (toolbarRect.xMax + rect.xMax) / 2f - 2f };
        Rect removeButtonRect = new Rect(toolbarRect) { xMin = addButtonRect.xMax, width = addButtonRect.width }; ;

        if (GUI.Button(addButtonRect, new GUIContent("+")))
        {
            _serializedObject.Update();

            collection.InsertArrayElementAtIndex(collection.arraySize);

            _serializedObject.ApplyModifiedProperties();
        }

        if (GUI.Button(removeButtonRect, new GUIContent("-")))
        {
            _serializedObject.Update();

            collection.DeleteArrayElementAtIndex(_scrollableToolbar.SelectedIndex);

            if (_scrollableToolbar.SelectedIndex >= collection.arraySize)
                _scrollableToolbar.SelectedIndex--;

            _serializedObject.ApplyModifiedProperties();
        }
    }

    private void DrawSelectedTab(Rect windowRect, int selectedIndex)
    {
        IEditorMenuToolbarTab toolbarTab = Tabs[selectedIndex];

        float tabHeight = toolbarTab.GetHeight(windowRect);
        float scrollMinus = tabHeight > windowRect.height ? _scrollHandleWidth : 0f;

        Rect viewRect = new Rect(windowRect) { height = tabHeight, xMax = windowRect.xMax - scrollMinus };

        _scrollPositions.MatchAmount(Tabs);
        _scrollPositions[selectedIndex] = GUI.BeginScrollView(windowRect, _scrollPositions[selectedIndex], viewRect);

        toolbarTab.DrawTab(viewRect);

        GUI.EndScrollView();
    }

    public void DrawToolbarWindow(Rect rect)
    {
        _serializedObject.Update();

        Rect toolbarRect = new Rect(rect) { height = ToolbarHeight, xMax = rect.xMax - 100f };
        Rect windowRect = new Rect(rect) { yMin = toolbarRect.yMax };

        SerializedProperty collection = _serializedObject.FindProperty(_collectionPropertyName);

        HandleScrollableToolbar(toolbarRect, collection);
        DrawAddRemoveButtons(rect, toolbarRect, collection);

        if (Tabs.Count <= 0) return;

        int selectedIndex = Mathf.Clamp(_scrollableToolbar.SelectedIndex, 0, Math.Max(collection.arraySize - 1, 0));

        DrawSelectedTab(windowRect, selectedIndex);
    }

    public void Dispose()
    {
        OnTabChange = null;
    }
}
