using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class DraggableRect
{
    public Rect rect;

    public bool ConstrainedX { private set; get; }
    public bool ConstrainedY { private set; get; }

    public Color innerColor;
    public Color outlineColor;

    public bool Dragging { private set; get; }
    public Vector2 DragOffset { private set; get; }

    public bool IgnoreOtherDraggableRects {  private set; get; }

    public delegate void BeforeDraw(Rect rect);
    public BeforeDraw beforeDraw;
    public delegate void AfterDraw(Rect rect);
    public AfterDraw afterDraw;

    protected List<Rect> _ignoreDragRects = new List<Rect>();

    public DraggableRect(Rect rect, bool constrainedX, bool constrainedY, Color innerColor, Color outlineColor, bool ignoreOtherDraggableRects)
    {
        this.rect = rect;
        this.ConstrainedX = constrainedX;
        this.ConstrainedY = constrainedY;
        this.innerColor = innerColor;
        this.outlineColor = outlineColor;
        this.IgnoreOtherDraggableRects = ignoreOtherDraggableRects;
    }

    public bool HandleStartDrag(Event e)
    {
        if (e.type != EventType.MouseDown || e.button != 0)
            return false;

        if (!rect.Contains(e.mousePosition))
            return false;

        foreach (Rect ignoreRect in _ignoreDragRects)
        {
            if(ignoreRect.Contains(e.mousePosition)) 
                return false;
        }

        DragOffset = rect.center - e.mousePosition;
        Dragging = true;

        return true;
    }

    public void StopDragging()
    {
        Dragging = false;
    }

    public virtual void DrawRect(Event e)
    {
        beforeDraw?.Invoke(rect);

        Handles.color = Color.white;
        Handles.DrawSolidRectangleWithOutline(rect, innerColor, outlineColor);

        afterDraw?.Invoke(rect);
    }

    public virtual void BeforeDrag() { }
    public virtual void AfterDrag() { }
}
