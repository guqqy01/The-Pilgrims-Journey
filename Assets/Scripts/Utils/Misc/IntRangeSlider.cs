using System;
using UnityEngine;
using Random = UnityEngine.Random;
[Serializable]
public class IntRangeSlider
{
    [SerializeField]
    private int minLimit = 0;

    [Tooltip("Lowest possible value for the range start.")]
    public int MinLimit
    {
        get => minLimit;
        set
        {
            minLimit = value;
            if (maxLimit < minLimit) maxLimit = minLimit;
            ClampToLimits();
        }
    }

    [SerializeField]
    private int maxLimit = 1;

    [Tooltip("Highest possible value for the range end.")]
    public int MaxLimit
    {
        get => maxLimit;
        set
        {
            maxLimit = value;
            if (minLimit > maxLimit) minLimit = maxLimit;
            ClampToLimits();
        }
    }

    [SerializeField]
    private int start = 0;

    [Tooltip("The starting point of the range.")]
    public int Start
    {
        get => start;
        set
        {
            int previousSpan = Length;
            start = Mathf.Clamp(value, MinLimit, MaxLimit);
            Length = Mathf.Clamp(previousSpan, 0, MaxLimit - start);
        }
    }

    [SerializeField]
    private int length = 1;

    [Tooltip("The length of the range from Start to End.")]
    public int Length
    {
        get => length;
        set
        {
            length = Mathf.Clamp(value, 0, MaxLimit - Start);
        }
    }

    /// <summary>
    /// The end point of the range (Start + Length).
    /// Automatically clamped within limits.
    /// </summary>
    public int End
    {
        get => Mathf.Clamp(Start + Length, MinLimit, MaxLimit);
        set => Length = Mathf.Clamp(value - Start, 0, MaxLimit - Start);
    }

    /// <summary>
    /// The span of the total limit range.
    /// </summary>
    public int LimitSpan => MaxLimit - MinLimit;

    /// <summary>
    /// Clamps Start and Length so the range stays within limits.
    /// </summary>
    public void ClampToLimits()
    {
        start = Mathf.Clamp(start, MinLimit, MaxLimit);
        length = Mathf.Clamp(length, 0, MaxLimit - start);
    }

    /// <summary>
    /// Moves the entire range by a delta without changing its length.
    /// </summary>
    public void Shift(int delta)
    {
        int maxStart = Mathf.Max(MinLimit, MaxLimit - Length);
        Start = Mathf.Clamp(start + delta, MinLimit, maxStart);
    }

    /// <summary>
    /// Sets both start and length at once while keeping everything clamped.
    /// </summary>
    public void Set(int newStart, int newLength)
    {
        start = Mathf.Clamp(newStart, MinLimit, MaxLimit);
        length = Mathf.Clamp(newLength, 0, MaxLimit - start);
    }

    public int RandomPoint()
    {
        return Random.Range(Start, End + 1);
    }
}