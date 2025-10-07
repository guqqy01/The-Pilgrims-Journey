using System;
using UnityEngine;

[Serializable]
public class RangeSlider
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