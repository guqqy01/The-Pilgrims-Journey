using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public class TimeLineDraggableRect : DraggableRect, IComparable<TimeLineDraggableRect>
{
    public IDraggableRectTimeItem ListOverTimeItem { private set; get; }

    public float Time
    {
        get
        {
            return ListOverTimeItem.StartTime;
        }
        set
        {
            ListOverTimeItem.StartTime = value;
        }
    }

    public TimeLineDraggableRect(IDraggableRectTimeItem listOverTimeItem, Rect rect, bool constrainedX, bool constrainedY, Color innerColor, Color outlineColor, bool ignoreOtherDraggableRects) : base(rect, constrainedX, constrainedY, innerColor, outlineColor, ignoreOtherDraggableRects)
    {
        ListOverTimeItem = listOverTimeItem;
    }

    public void SetRectValues(Rect draggableAreaRect, float viewAmount, float viewStartTime)
    {
        if (ListOverTimeItem == null)
            return;


        if (ListOverTimeItem is IDraggableRectTimeLengthItem lengthItem)
        {
            switch (lengthItem.Axis)
            {
                case DraggableRectAxis.Horizontal:
                    rect.width = Mathf.Lerp(0f, draggableAreaRect.width, Mathf.InverseLerp(0f, viewAmount, lengthItem.Length));
                    break;
                case DraggableRectAxis.Vertical:
                    rect.height = Mathf.Lerp(0f, draggableAreaRect.height, Mathf.InverseLerp(0f, viewAmount, lengthItem.Length));
                    break;
            }
        }

        float xPos = (draggableAreaRect.width / viewAmount) * (Time - viewStartTime);
        float yPos = (draggableAreaRect.height / viewAmount) * (Time - viewStartTime);
        switch (ListOverTimeItem.Position)
        {
            case IDraggableRectTimeItem.TimePosition.Start:
                switch (ListOverTimeItem.Axis)
                {
                    case DraggableRectAxis.Horizontal:
                        rect.position = new Vector2(xPos, rect.position.y);
                        break;
                    case DraggableRectAxis.Vertical:
                        rect.position = new Vector2(rect.position.x, yPos);
                        break;
                }
                break;
            case IDraggableRectTimeItem.TimePosition.Center:
                switch (ListOverTimeItem.Axis)
                {
                    case DraggableRectAxis.Horizontal:
                        rect.center = new Vector2(xPos, rect.center.y);
                        break;
                    case DraggableRectAxis.Vertical:
                        rect.center = new Vector2(rect.center.x, yPos);
                        break;
                }
                break;
            case IDraggableRectTimeItem.TimePosition.End:
                switch (ListOverTimeItem.Axis)
                {
                    case DraggableRectAxis.Horizontal:
                        rect.position = new Vector2(xPos + rect.width, rect.position.y);
                        break;
                    case DraggableRectAxis.Vertical:
                        rect.position = new Vector2(rect.position.x, yPos + rect.height);
                        break;
                }
                break;
        }
    }

    public void SetObjectValues(Rect draggableAreaRect, float viewAmount, float viewStartTime)
    {
        if (ListOverTimeItem == null)
            return;

        if (ListOverTimeItem is IDraggableRectTimeLengthItem lengthItem)
        {
            switch (lengthItem.Axis)
            {
                case DraggableRectAxis.Horizontal:
                    lengthItem.Length = Mathf.Lerp(0f, viewAmount + viewStartTime, Mathf.InverseLerp(0f, draggableAreaRect.width, rect.width));
                    break;
                case DraggableRectAxis.Vertical:
                    lengthItem.Length = Mathf.Lerp(0f, viewAmount + viewStartTime, Mathf.InverseLerp(0f, draggableAreaRect.height, rect.height));
                    break;
            }
        }

        switch (ListOverTimeItem.Position)
        {
            case IDraggableRectTimeItem.TimePosition.Start:
                switch (ListOverTimeItem.Axis)
                {
                    case DraggableRectAxis.Horizontal:
                        Time = viewAmount * rect.xMin / draggableAreaRect.width + viewStartTime;
                        break;
                    case DraggableRectAxis.Vertical:
                        Time = viewAmount * rect.yMin / draggableAreaRect.height + viewStartTime;
                        break;
                }
                break;
            case IDraggableRectTimeItem.TimePosition.Center:
                switch (ListOverTimeItem.Axis)
                {
                    case DraggableRectAxis.Horizontal:
                        Time = viewAmount * rect.center.x / draggableAreaRect.width + viewStartTime;
                        break;
                    case DraggableRectAxis.Vertical:
                        Time = viewAmount * rect.center.y / draggableAreaRect.height + viewStartTime;
                        break;
                }
                break;
            case IDraggableRectTimeItem.TimePosition.End:
                switch (ListOverTimeItem.Axis)
                {
                    case DraggableRectAxis.Horizontal:
                        Time = viewAmount * rect.xMax / draggableAreaRect.width + viewStartTime;
                        break;
                    case DraggableRectAxis.Vertical:
                        Time = viewAmount * rect.yMax / draggableAreaRect.width + viewStartTime;
                        break;
                }
                break;
        }
    }

    public int CompareTo(TimeLineDraggableRect other)
    {
        return ListOverTimeItem.CompareTo(other.ListOverTimeItem);
    }
}