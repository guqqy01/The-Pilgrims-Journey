using System;
using UnityEngine;


[AttributeUsage(AttributeTargets.Field)]
public class BuffTypeAttribute : PropertyAttribute
{
    public readonly bool IsInt;

    public BuffTypeAttribute(bool isInt)
    {
        IsInt = isInt;
    }
}
