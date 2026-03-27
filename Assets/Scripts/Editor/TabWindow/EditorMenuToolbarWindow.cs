using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Newtonsoft.Json;

public class EditorMenuToolbarWindow
{
    public readonly List<IEditorMenuToolbarTab> Tabs = new List<IEditorMenuToolbarTab>();

    public float ToolbarHeight = 2f * EditorGUIUtility.singleLineHeight;

    private List<Vector2> _scrollPositions = new List<Vector2>();

    private int _mainToolbarIndex = 0;

    private const float _scrollHandleWidth = 15f;

    #region Init

    public EditorMenuToolbarWindow(){}

    public EditorMenuToolbarWindow(List<IEditorMenuToolbarTab> tabs)
    {
        Tabs.Clear();
        Tabs.AddRange(tabs);
    }

    public EditorMenuToolbarWindow(List<IEditorMenuToolbarTab> tabs, float toolbarHeight)
    {
        ToolbarHeight = toolbarHeight;

        Tabs.Clear();
        Tabs.AddRange(tabs);
    }

    public EditorMenuToolbarWindow(float toolbarHeight, params IEditorMenuToolbarTab[] tabs)
    {
        ToolbarHeight = toolbarHeight;

        Tabs.Clear();
        Tabs.AddRange(tabs);
    }

    public EditorMenuToolbarWindow(params IEditorMenuToolbarTab[] tabs)
    {
        Tabs.Clear();
        Tabs.AddRange(tabs);
    }

    #endregion

    public void DrawToolbarWindow(Rect rect)
    {
        Rect toolbarRect = new Rect(rect) { height = ToolbarHeight };
        Rect windowRect = new Rect(rect) { yMin = toolbarRect .yMax};

        string[] tabNames = Tabs.Select(x => x.Name).ToArray();

        _mainToolbarIndex = GUI.Toolbar(toolbarRect, _mainToolbarIndex, tabNames);

        IEditorMenuToolbarTab toolbarTab = Tabs[_mainToolbarIndex];

        float tabHeight = toolbarTab.GetHeight(windowRect);

        float scrollMinus = tabHeight > windowRect.height ? _scrollHandleWidth : 0f;

        Rect viewRect = new Rect(windowRect) { height = tabHeight, xMax = windowRect.xMax - scrollMinus };

        _scrollPositions.MatchAmount(Tabs);
        _scrollPositions[_mainToolbarIndex] = GUI.BeginScrollView(windowRect, _scrollPositions[_mainToolbarIndex], viewRect);

        toolbarTab.DrawTab(viewRect);

        GUI.EndScrollView();
    }
}
