using System;
using UnityEngine;

[Serializable]
public class FloatRangeSlider
{
    public float MinLimit = 0f;
    public float MaxLimit = 1f;

    public Vector2 MinMaxValues = new Vector2(0f, 1f);

    public float ValueSpan 
    { 
        get 
        { 
            return MaxValue - MinValue;
        }
        set
        {
            float currentValueSpan = MaxValue - MinValue;
            if(value < currentValueSpan)
            {
                MaxValue = MinValue + value;
            }
            else if (value > currentValueSpan)
            {
                float amountToExpand = value - currentValueSpan;
                float maxLargeExpansion = MaxLimit - MaxValue;

                if(amountToExpand <= maxLargeExpansion)
                {
                    MaxValue = MinValue + value;
                }
                else
                {
                    float remainder = amountToExpand - maxLargeExpansion;
                    MaxValue = MaxLimit;
                    MinValue = MinValue - remainder;
                }
            }
        }
    }
    public float LimitSpan => MaxLimit - MinLimit;

    public float MaxValue
    {
        get
        {
            return  MinMaxValues.x > MinMaxValues.y ? MinMaxValues.x : MinMaxValues.y;
        }
        set
        {
            if (MinMaxValues.x > MinMaxValues.y) 
            {
                Mathf.Clamp(MinMaxValues.x = value, MinValue, MaxLimit);
            } 
            else 
            {
                Mathf.Clamp(MinMaxValues.y = value, MinValue, MaxLimit);
            }
        }
    }
    public float MinValue
    {
        get
        {
            return MinMaxValues.x < MinMaxValues.y ? MinMaxValues.x : MinMaxValues.y;
        }
        set
        {
            if (MinMaxValues.x < MinMaxValues.y)
            {
                Mathf.Clamp(MinMaxValues.x = value, MinLimit, MaxValue);
            }
            else
            {
                Mathf.Clamp(MinMaxValues.y = value, MinLimit, MaxValue);
            }
        }
    }
}

[Serializable]
public class IntRangeSlider
{
    public int MinLimit = 0;
    public int MaxLimit = 1;

    public Vector2Int MinMaxValues = new Vector2Int(0, 1);

    public int ValueSpan
    {
        get
        {
            return MaxValue - MinValue;
        }
        set
        {
            int currentValueSpan = MaxValue - MinValue;
            if (value < currentValueSpan)
            {
                MaxValue = MinValue + value;
            }
            else if (value > currentValueSpan)
            {
                int amountToExpand = value - currentValueSpan;
                int maxLargeExpansion = MaxLimit - MaxValue;

                if (amountToExpand <= maxLargeExpansion)
                {
                    MaxValue = MinValue + value;
                }
                else
                {
                    int remainder = amountToExpand - maxLargeExpansion;
                    MaxValue = MaxLimit;
                    MinValue = MinValue - remainder;
                }
            }
        }
    }
    public int LimitSpan => MaxLimit - MinLimit;

    public int MaxValue
    {
        get
        {
            return MinMaxValues.x > MinMaxValues.y ? MinMaxValues.x : MinMaxValues.y;
        }
        set
        {
            if (MinMaxValues.x > MinMaxValues.y)
            {
                Mathf.Clamp(MinMaxValues.x = value, MinValue, MaxLimit);
            }
            else
            {
                Mathf.Clamp(MinMaxValues.y = value, MinValue, MaxLimit);
            }
        }
    }
    public int MinValue
    {
        get
        {
            return MinMaxValues.x < MinMaxValues.y ? MinMaxValues.x : MinMaxValues.y;
        }
        set
        {
            if (MinMaxValues.x < MinMaxValues.y)
            {
                Mathf.Clamp(MinMaxValues.x = value, MinLimit, MaxValue);
            }
            else
            {
                Mathf.Clamp(MinMaxValues.y = value, MinLimit, MaxValue);
            }
        }
    }
}