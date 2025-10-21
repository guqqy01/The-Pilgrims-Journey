using System;
using UnityEngine;

[Serializable]
public struct IntOrFloatObject : IEquatable<IntOrFloatObject>
{
    [SerializeField] private int intValue;
    [SerializeField] private float floatValue;
    [SerializeField] private bool isInt;

    public bool IsInt => isInt;
    public bool IsFloat => !isInt;

    public int IntValue => isInt ? intValue : Mathf.RoundToInt(floatValue);
    public float FloatValue => isInt ? intValue : floatValue;

    public object Value => isInt ? intValue : floatValue;

    // Constructors
    public IntOrFloatObject(int value)
    {
        intValue = value;
        floatValue = value;
        isInt = true;
    }

    public IntOrFloatObject(float value)
    {
        intValue = Mathf.RoundToInt(value);
        floatValue = value;
        isInt = false;
    }

    // Implicit conversions
    public static implicit operator IntOrFloatObject(int value) => new IntOrFloatObject(value);
    public static implicit operator IntOrFloatObject(float value) => new IntOrFloatObject(value);

    public static implicit operator float(IntOrFloatObject obj) => obj.FloatValue;
    public static explicit operator int(IntOrFloatObject obj) => obj.IntValue;

    // Equality
    public bool Equals(IntOrFloatObject other) => Mathf.Approximately(FloatValue, other.FloatValue);
    public override bool Equals(object obj) => obj is IntOrFloatObject other && Equals(other);
    public override int GetHashCode() => FloatValue.GetHashCode();

    public static bool operator ==(IntOrFloatObject a, IntOrFloatObject b) => a.Equals(b);
    public static bool operator !=(IntOrFloatObject a, IntOrFloatObject b) => !a.Equals(b);

    // Comparisons
    public static bool operator >(IntOrFloatObject a, IntOrFloatObject b) => a.FloatValue > b.FloatValue;
    public static bool operator <(IntOrFloatObject a, IntOrFloatObject b) => a.FloatValue < b.FloatValue;
    public static bool operator >=(IntOrFloatObject a, IntOrFloatObject b) => a.FloatValue >= b.FloatValue;
    public static bool operator <=(IntOrFloatObject a, IntOrFloatObject b) => a.FloatValue <= b.FloatValue;

    // Arithmetic
    public static IntOrFloatObject operator +(IntOrFloatObject a, IntOrFloatObject b) 
    {
        if (a.isInt || b.isInt)
        {
            return new IntOrFloatObject(a.IntValue + b.IntValue);
        }
        return new IntOrFloatObject(a.FloatValue + b.FloatValue);
    } 
    public static IntOrFloatObject operator -(IntOrFloatObject a, IntOrFloatObject b)
    {
        if (a.isInt || b.isInt)
        {
            return new IntOrFloatObject(a.IntValue - b.IntValue);
        }
        return new IntOrFloatObject(a.FloatValue - b.FloatValue);
    }
    public static IntOrFloatObject operator *(IntOrFloatObject a, IntOrFloatObject b)
    {
        if (a.isInt || b.isInt)
        {
            return new IntOrFloatObject(a.IntValue * b.IntValue);
        }
        return new IntOrFloatObject(a.FloatValue * b.FloatValue);
    }
    public static IntOrFloatObject operator /(IntOrFloatObject a, IntOrFloatObject b)
    {
        if (a.isInt || b.isInt)
        {
            return new IntOrFloatObject(a.IntValue / b.IntValue);
        }
        return new IntOrFloatObject(a.FloatValue / b.FloatValue);
    }

    public override string ToString() => isInt ? intValue.ToString() : floatValue.ToString("0.###");
}