using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using DialogueSystem;
using NUnit.Framework;

public class TimeLineDraggableArea<T> : TimeLineDraggableArea where T : IDraggableRectTimeItem
{
    private List<T> list;

    public TimeLineDraggableArea(List<T> list, float timeScale, bool fillOtherAxis, float timeStart) : base(timeScale, fillOtherAxis, timeStart)
    {
        this.list = list;
    }

    public override void HandleDraggableRects(Event e)
    {
        HandleAmountOfDraggableRects(list);
        SetRectValues();
        base.HandleDraggableRects(e);
    }

    public void RemoveRightClicked()
    {
        if (rectRightClicked is TimeLineDraggableRect timeLineDraggableRect)
        {
            RemoveDraggableRect(list, timeLineDraggableRect);
        }
    }

    public void RemoveDraggableRect(TimeLineDraggableRect rectToDelete)
    {
        if (rectToDelete != null)
        {
            list.Remove((T)rectToDelete.ListOverTimeItem);

            Clear(2);
        }
    }
}

public class TimeLineDraggableArea : DraggableArea
{
    public float timeScale;
    public float timeStart;

    private bool drawTimeline;
    public bool DrawTimeline
    {
        get
        {
            return drawTimeline;
        }
        set
        {
            if (value)
            {
                beforeDraw += DrawTimeLine;
            }
            else
            {
                beforeDraw -= DrawTimeLine;
            }

            drawTimeline = value;
        }
    }

    public void DrawTimeLine(Rect rect)
    {
        TimelineDrawer.DrawTimeLine(rect, timeScale, timeStart);
    }

    public List<TimeLineDraggableRect> TimelineDraggableRects
    {
        get
        {
            List<TimeLineDraggableRect> returnList = new List<TimeLineDraggableRect>();
            foreach (var rect in draggableRects)
            {
                if(rect is TimeLineDraggableRect timeLineDraggableRect)
                {
                    returnList.Add(timeLineDraggableRect);
                }
            }
            return returnList;
        }
    }


    public TimeLineDraggableArea(float timeScale, bool fillOtherAxis, float timeStart)
    {
        this.timeScale = timeScale;
        this.timeStart = timeStart;
        this.fillOtherAxis = fillOtherAxis;

        protectedOnDraggingRect += SetObjectValues;
    }

    public void HandleDraggableRects<T>(Event e, List<T> List) where T : IDraggableRectTimeItem
    {
        HandleAmountOfDraggableRects(List);
        SetRectValues();
        base.HandleDraggableRects(e);
    }


    protected void HandleAmountOfDraggableRects<T>(List<T> List) where T : IDraggableRectTimeItem
    {
        //Remove draggable rects

        if (List == null)
            List = new List<T>();

        List<TimeLineDraggableRect> draggableRectsToRemove = new List<TimeLineDraggableRect>();

        foreach (TimeLineDraggableRect rect in TimelineDraggableRects)
        {
            if (rect.ListOverTimeItem == null)
            {
                Debug.Log("null");
                draggableRectsToRemove.Add(rect);
            }
        }

        for (int i = 0; i < draggableRectsToRemove.Count; i++)
        {
            draggableRects.Remove(draggableRectsToRemove[i]);
        }

        draggableRectsToRemove.Clear();

        for (int i = 0; i < TimelineDraggableRects.Count; i++)
        {
            bool contentContainsProperty = false;

            for (int j = 0; j < List.Count; j++)
            {
                if ((IDraggableRectTimeItem)List[j] == TimelineDraggableRects[i].ListOverTimeItem)
                {
                    contentContainsProperty = true;
                    break;
                }
            }

            if (!contentContainsProperty)
            {
                draggableRectsToRemove.Add(TimelineDraggableRects[i]);
            }
        }

        for (int i = 0; i < draggableRectsToRemove.Count; i++)
        {
            draggableRects.Remove(draggableRectsToRemove[i]);
        }

        //Add draggable rects

        for (int i = 0; i < List.Count; i++)
        {
            bool draggableRectsContainProperty = false;

            foreach (TimeLineDraggableRect rect in TimelineDraggableRects)
            {
                if (rect.ListOverTimeItem == (IDraggableRectTimeItem)List[i])
                {
                    draggableRectsContainProperty = true;
                    break;
                }
            }

            if (!draggableRectsContainProperty)
            {
                AddTimeLineDraggableRect(List[i]);
            }
        }
    }

    protected void AddTimeLineDraggableRect(IDraggableRectTimeItem item)
    {
        Rect newRect = new Rect() { width = defaultDraggableRectSizes.x, height = defaultDraggableRectSizes.y };
        newRect.position = areaRect.position;

        bool constrainX = false;
        bool constrainY = false;

        switch (item.Axis)
        {
            case DraggableRectAxis.Horizontal:
                constrainY = true;
                if (fillOtherAxis)
                {
                    newRect.height = areaRect.height;
                }
                break;
            case DraggableRectAxis.Vertical:
                constrainX = true;
                if (fillOtherAxis)
                {
                    newRect.width = areaRect.width;
                }
                break;
        }

        if (item is IDraggableRectTimeLengthItem lengthItem)
        {
            switch (lengthItem.Axis)
            {
                case DraggableRectAxis.Horizontal:
                    newRect.width = Mathf.Lerp(0, areaRect.width, Mathf.InverseLerp(0f, timeScale + timeStart, lengthItem.Length));
                    break;
                case DraggableRectAxis.Vertical:
                    newRect.height = Mathf.Lerp(0, areaRect.height, Mathf.InverseLerp(0f, timeScale + timeStart, lengthItem.Length));
                    break;
            }
        }

        switch (item.Position)
        {
            case IDraggableRectTimeItem.TimePosition.Start:
                switch (item.Axis)
                {
                    case DraggableRectAxis.Horizontal:
                        newRect.position = new Vector2(Mathf.Lerp(areaRect.xMin, areaRect.xMax, Mathf.InverseLerp(0f, timeScale + timeStart, item.StartTime)), newRect.position.y);
                        break;
                    case DraggableRectAxis.Vertical:
                        newRect.position = new Vector2(newRect.position.x, Mathf.Lerp(areaRect.yMin, areaRect.yMax, Mathf.InverseLerp(0f, timeScale + timeStart, item.StartTime)));
                        break;
                }
                break;
            case IDraggableRectTimeItem.TimePosition.Center:
                switch (item.Axis)
                {
                    case DraggableRectAxis.Horizontal:
                        newRect.center = new Vector2(Mathf.Lerp(areaRect.xMin, areaRect.xMax, Mathf.InverseLerp(0f, timeScale + timeStart, item.StartTime)), newRect.center.y);
                        break;
                    case DraggableRectAxis.Vertical:
                        newRect.center = new Vector2(newRect.center.x, Mathf.Lerp(areaRect.yMin, areaRect.yMax, Mathf.InverseLerp(0f, timeScale + timeStart, item.StartTime)));
                        break;
                }
                break;
            case IDraggableRectTimeItem.TimePosition.End:
                switch (item.Axis)
                {
                    case DraggableRectAxis.Horizontal:
                        newRect.position = new Vector2(Mathf.Lerp(areaRect.xMin, areaRect.xMax, Mathf.InverseLerp(0f, timeScale + timeStart, item.StartTime)) - newRect.width, newRect.position.y);
                        break;
                    case DraggableRectAxis.Vertical:
                        newRect.position = new Vector2(newRect.position.x, Mathf.Lerp(areaRect.yMin, areaRect.yMax, Mathf.InverseLerp(0f, timeScale + timeStart, item.StartTime)) - newRect.height);
                        break;
                }
                break;
        }

        AddTimelineDraggableRect(item, newRect, constrainX, constrainY);
    }

    public void RemoveRightClicked<T>(List<T> timelineList) where T : IDraggableRectTimeItem
    {
        if (rectRightClicked is TimeLineDraggableRect timeLineDraggableRect)
        {
            RemoveDraggableRect(timelineList, timeLineDraggableRect);
        }
    }

    protected void SetRectValues()
    {
        for (int i = 0; i < TimelineDraggableRects.Count; i++)
        {
            TimelineDraggableRects[i].SetRectValues(areaRect, timeScale, timeStart);
        }
    }

    protected void SetObjectValues()
    {
        for (int i = 0; i < TimelineDraggableRects.Count; i++)
        {
            TimelineDraggableRects[i].SetObjectValues(areaRect, timeScale, timeStart);
        }
    }

    protected virtual void AddTimelineDraggableRect<T>(T item, Rect rect, bool constrainX, bool constrainY) where T : IDraggableRectTimeItem
    {
        TimeLineDraggableRect draggableRect = new TimeLineDraggableRect(item, rect, constrainX, constrainY, defaultInnerColor, defaultOuterColor, defaultIgnoreOtherDraggableRects);
        Add(2, draggableRect);
    }

    public void RemoveDraggableRect<T>(List<T> timelineList, TimeLineDraggableRect rectToDelete) where T : IDraggableRectTimeItem
    {
        if (rectToDelete != null)
        {
            timelineList.Remove((T)rectToDelete.ListOverTimeItem);

            Clear(2);
        }
    }
}