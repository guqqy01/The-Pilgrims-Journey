using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System;
[Serializable]
public class EditorMenuToolbarWindow
{
    protected List<IEditorMenuToolbarTab> _tabs = new List<IEditorMenuToolbarTab>();

    public float ToolbarHeight = 2f * EditorGUIUtility.singleLineHeight;

    private List<Vector2> _scrollPositions = new List<Vector2>();

    private int _mainToolbarIndex = 0;

    private const float _scrollHandleWidth = 15f;

    #region Init

    public EditorMenuToolbarWindow(){}

    public EditorMenuToolbarWindow(List<IEditorMenuToolbarTab> tabs)
    {
        _tabs.Clear();
        _tabs.AddRange(tabs);
    }

    public EditorMenuToolbarWindow(List<IEditorMenuToolbarTab> tabs, float toolbarHeight)
    {
        ToolbarHeight = toolbarHeight;

        _tabs.Clear();
        _tabs.AddRange(tabs);
    }

    public EditorMenuToolbarWindow(float toolbarHeight, params IEditorMenuToolbarTab[] tabs)
    {
        ToolbarHeight = toolbarHeight;

        _tabs.Clear();
        _tabs.AddRange(tabs);
    }

    public EditorMenuToolbarWindow(params IEditorMenuToolbarTab[] tabs)
    {
        _tabs.Clear();
        _tabs.AddRange(tabs);
    }

    #endregion

    public void DrawToolbarWindow(Rect rect)
    {
        if (_tabs == null)
            return;

        Rect toolbarRect = new Rect(rect) { height = ToolbarHeight };
        Rect windowRect = new Rect(rect) { yMin = toolbarRect .yMax};

        string[] tabNames = _tabs.Select(x => x.Name).ToArray();

        _mainToolbarIndex = GUI.Toolbar(toolbarRect, _mainToolbarIndex, tabNames);

        IEditorMenuToolbarTab toolbarTab = _tabs[_mainToolbarIndex];

        float tabHeight = toolbarTab.GetHeight(windowRect);

        float scrollMinus = tabHeight > windowRect.height ? _scrollHandleWidth : 0f;

        Rect viewRect = new Rect(windowRect) { height = tabHeight, xMax = windowRect.xMax - scrollMinus };

        if (_scrollPositions == null)
            _scrollPositions = new List<Vector2>();

        _scrollPositions.MatchAmount(_tabs);
        _scrollPositions[_mainToolbarIndex] = GUI.BeginScrollView(windowRect, _scrollPositions[_mainToolbarIndex], viewRect);

        toolbarTab.DrawTab(viewRect);

        GUI.EndScrollView();
    }
}
