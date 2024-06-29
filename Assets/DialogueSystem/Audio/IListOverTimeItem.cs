using System;

public interface IListOverTimeItem : IComparable<IListOverTimeItem>
{
    public float StartTime { get; set; } 
    public float Length { get; set; }

    public bool Overlap(float time, float length)
    {
        float timeEnd = time + length;
        return !(StartTime >= timeEnd) && !(time >= EndTime);
    }

    public bool Overlap(IListOverTimeItem other)
    {
        return !(StartTime >= other.EndTime) && !(other.StartTime >= EndTime);
    }

    public float EndTime => StartTime + Length;
}
