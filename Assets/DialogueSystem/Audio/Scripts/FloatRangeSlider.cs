using System;
using UnityEngine;

[Serializable]
public class FloatRangeSlider
{
    [Tooltip("Lowest possible value for the range start.")]
    public float MinLimit = 0f;

    [Tooltip("Highest possible value for the range end.")]
    public float MaxLimit = 1f;

    [Tooltip("The starting point of the range.")]
    public float Start = 0f;

    [Tooltip("The length of the range from Start to End.")]
    public float Length = 1f;

    /// <summary>
    /// The end point of the range (Start + Length).
    /// Automatically clamped within limits.
    /// </summary>
    public float End
    {
        get => Mathf.Clamp(Start + Length, MinLimit, MaxLimit);
        set => Length = Mathf.Clamp(value - Start, 0f, MaxLimit - Start);
    }

    /// <summary>
    /// The span of the total limit range.
    /// </summary>
    public float LimitSpan => MaxLimit - MinLimit;

    /// <summary>
    /// Clamps Start and Length so the range stays within limits.
    /// </summary>
    public void ClampToLimits()
    {
        Start = Mathf.Clamp(Start, MinLimit, MaxLimit);
        Length = Mathf.Clamp(Length, 0f, MaxLimit - Start);
    }

    /// <summary>
    /// Moves the entire range by a delta without changing its length.
    /// </summary>
    public void Shift(float delta)
    {
        Start = Mathf.Clamp(Start + delta, MinLimit, MaxLimit - Length);
    }

    /// <summary>
    /// Sets both start and length at once while keeping everything clamped.
    /// </summary>
    public void Set(float start, float length)
    {
        Start = Mathf.Clamp(start, MinLimit, MaxLimit);
        Length = Mathf.Clamp(length, 0f, MaxLimit - Start);
    }
}

[Serializable]
public class IntRangeSlider
{
    [Tooltip("Lowest possible value for the range start.")]
    public int MinLimit = 0;

    [Tooltip("Highest possible value for the range end.")]
    public int MaxLimit = 10;

    [Tooltip("The starting point of the range.")]
    public int Start = 0;

    [Tooltip("The length of the range from Start to End.")]
    public int Length = 1;

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
        Start = Mathf.Clamp(Start, MinLimit, MaxLimit);
        Length = Mathf.Clamp(Length, 0, MaxLimit - Start);
    }

    /// <summary>
    /// Moves the entire range by a delta without changing its length.
    /// </summary>
    public void Shift(int delta)
    {
        Start = Mathf.Clamp(Start + delta, MinLimit, MaxLimit - Length);
    }

    /// <summary>
    /// Sets both start and length at once while keeping everything clamped.
    /// </summary>
    public void Set(int start, int length)
    {
        Start = Mathf.Clamp(start, MinLimit, MaxLimit);
        Length = Mathf.Clamp(length, 0, MaxLimit - Start);
    }
}