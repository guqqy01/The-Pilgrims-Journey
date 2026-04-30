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
