using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System;

public class ReorderableFoldoutList
{
    public readonly SerializedObject _serializedObject;
    public readonly SerializedProperty _listProperty;

    private ReorderableList _reorderableList;

    private readonly Dictionary<int, bool> _indexAndIfExpanded = new Dictionary<int, bool>();
    private readonly Action<Rect, SerializedProperty, int> _drawElementAction;
    private readonly Func<int, float> _elementHeightFunction;
    private readonly string _header = "";

    private Vector2 _scrollPosition;

    private bool _listExpanded = true;

    public float Height => _reorderableList.GetHeight();

    public ReorderableFoldoutList(SerializedObject serializedObject, SerializedProperty listProperty, Func<int, float> elementHeightFunction, Action<Rect, SerializedProperty, int> drawElementAction, string header = "")
    {
        _serializedObject = serializedObject;
        _listProperty = listProperty.Copy();

        _drawElementAction = drawElementAction;
        _elementHeightFunction = elementHeightFunction;
        _header = header;

        _reorderableList = CreateReorderableFoldoutList();

    }

    public float GetHeight()
    {
        return _reorderableList.GetHeight() + EditorGUIUtility.singleLineHeight * 3f + 4f;
    }

    public void DoList(Rect rect)
    {
        _serializedObject.Update();

        float listHeight = _reorderableList.GetHeight();
        Rect listRect = new Rect() { height = listHeight, width = rect.width };

        GUI.BeginGroup(rect);

        _reorderableList.DoList(listRect);

        GUI.EndGroup();

        _serializedObject.ApplyModifiedProperties();
    }

    private ReorderableList CreateReorderableFoldoutList()
    {
        var list = new ReorderableList(_serializedObject, _listProperty, true, true, true, true);

        list.elementHeightCallback = index =>
        {
            if (!_indexAndIfExpanded.ContainsKey(index))
                _indexAndIfExpanded.Add(index, true);

            if (_indexAndIfExpanded[index])
            {
                float height = _elementHeightFunction.Invoke(index);

                return height;
            }
            else
                return EditorGUIUtility.singleLineHeight;
        };

        if (_header == "")
            list.headerHeight = 0;
        else
            list.headerHeight = EditorGUIUtility.singleLineHeight;

        list.drawHeaderCallback = rect =>
        {
            if (string.IsNullOrEmpty(_header))
                return;

            bool previous = _listExpanded;

            _listExpanded = EditorGUI.Foldout(
                rect,
                _listExpanded,
                string.IsNullOrEmpty(_header) ? _listProperty.displayName : _header,
                true
            );

            if(previous != _listExpanded)
            {
                for (int i = 0; i < _listProperty.arraySize; i++)
                {
                    _indexAndIfExpanded[i] = _listExpanded;
                }

                _reorderableList = CreateReorderableFoldoutList();
            }
        };

        list.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            SerializedProperty element = _listProperty.GetArrayElementAtIndex(index);

            if (!_indexAndIfExpanded.ContainsKey(index))
                _indexAndIfExpanded.Add(index, true);

            Rect foldoutRect = new Rect(rect) { height = EditorGUIUtility.singleLineHeight };

            bool previous = _indexAndIfExpanded[index];

            _indexAndIfExpanded[index] = EditorGUI.Foldout(foldoutRect, _indexAndIfExpanded[index], new GUIContent(element.displayName));

            if(_indexAndIfExpanded[index] != previous)
            {
                bool allSame = true;

                for (int i = 0; i < _listProperty.arraySize; i++)
                {
                    if(_indexAndIfExpanded[i] != _indexAndIfExpanded[index])
                    {
                        allSame = false;
                        break;
                    }
                }

                if (allSame)
                {
                    _listExpanded = _indexAndIfExpanded[index];
                }
            }

            Rect contentRect = new Rect(rect) { yMin = rect.yMin + EditorGUIUtility.singleLineHeight, xMin = rect.xMin + 10f };

            if (_indexAndIfExpanded[index])
            {
                _drawElementAction.Invoke(contentRect, element, index);
            }
        };

        return list;
    }
}