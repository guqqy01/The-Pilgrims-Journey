using System;
using UnityEngine;
[AttributeUsage(AttributeTargets.Field)]
public class BuffTypeAttribute : PropertyAttribute
{
    public readonly bool IsInt;
    public readonly string MemberName;

    public BuffTypeAttribute(bool isInt)
    {
        IsInt = isInt;
    }

    /// <param name="memberName">Name of a bool property, field or parameterless method on the owning object.</param>
    public BuffTypeAttribute(string memberName)
    {
        MemberName = memberName;
    }
}