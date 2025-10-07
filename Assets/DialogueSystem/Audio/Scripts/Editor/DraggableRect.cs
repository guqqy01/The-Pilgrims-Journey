using UnityEngine;
using UnityEditor;
using Serilog;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEditor.AssetImporters;
using System;
using UnityEngine.SocialPlatforms;

public class DraggableRect
{
    public Rect rect;

    public bool constrainedX;
    public bool constrainedY;

    public Color innerColor;
    public Color outlineColor;

    public bool dragging;
    public Vector2 dragOffset;

    public bool ignoreOtherDraggableRects;

    public Dictionary<string, GenericMenu.MenuFunction> rightClickOptions = new Dictionary<string, GenericMenu.MenuFunction>();

    public delegate void BeforeDraw(Rect rect);
    public BeforeDraw beforeDraw;
    public delegate void AfterDraw(Rect rect);
    public AfterDraw afterDraw;

    public List<Rect> ignoreDragRects = new List<Rect>();

    public DraggableRect(Rect rect, bool constrainedX, bool constrainedY, Color innerColor, Color outlineColor, bool ignoreOtherDraggableRects)
    {
        this.rect = rect;
        this.constrainedX = constrainedX;
        this.constrainedY = constrainedY;
        this.innerColor = innerColor;
        this.outlineColor = outlineColor;
        this.ignoreOtherDraggableRects = ignoreOtherDraggableRects;
    }

    public bool HandleStartDrag(Event e)
    {
        if (e.type != EventType.MouseDown || e.button != 0)
            return false;

        if (!rect.Contains(e.mousePosition))
            return false;

        foreach (Rect ignoreRect in ignoreDragRects)
        {
            if(ignoreRect.Contains(e.mousePosition)) 
                return false;
        }

        dragOffset = rect.center - e.mousePosition;
        dragging = true;

        return true;
    }

    public virtual void DrawRect(Event e)
    {
        beforeDraw?.Invoke(rect);

        Handles.color = Color.white;
        Handles.DrawSolidRectangleWithOutline(rect, innerColor, outlineColor);

        if (e.type == EventType.MouseDown && e.button == 1 && rect.Contains(e.mousePosition) && rightClickOptions.Count > 0)
        {
            GenericMenu menu = new GenericMenu();
            foreach(KeyValuePair<string, GenericMenu.MenuFunction> kvp in rightClickOptions)
            {
                menu.AddItem(new GUIContent(kvp.Key), false, kvp.Value);
            }
            menu.ShowAsContext();
        }

        afterDraw?.Invoke(rect);
    }

    public virtual void BeforeDrag() { }
    public virtual void AfterDrag() { }
}
