using System;

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
