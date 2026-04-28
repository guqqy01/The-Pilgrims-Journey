using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

public interface IDraggableRectTimeItem : IComparable<IDraggableRectTimeItem>
{
    public float StartTime { get; set; }
    public DraggableRectAxis Axis { get; }
    public enum TimePosition 
    {
        Start,
        Center,
        End,
    }

    public TimePosition Position { get; }
}


public class DraggableTimeItemFloat : IDraggableRectTimeItem
{
    
    public float StartTime
    {
        get;
        set;
    }

    public DraggableRectAxis Axis { get; set; }

    public IDraggableRectTimeItem.TimePosition Position { get; set; }

    public DraggableTimeItemFloat(float time, DraggableRectAxis draggableRectAxis, IDraggableRectTimeItem.TimePosition position)
    {
        StartTime = time;
        Axis = draggableRectAxis;
        Position = position;
    }

    public int CompareTo(IDraggableRectTimeItem other)
    {
        return StartTime.CompareTo(other.StartTime);
    }
}

public interface IDraggableRectTimeLengthItem : IDraggableRectTimeItem
{
    public float Length { get; set; }
}

public enum DraggableRectAxis
{
    Horizontal,
    Vertical,
}