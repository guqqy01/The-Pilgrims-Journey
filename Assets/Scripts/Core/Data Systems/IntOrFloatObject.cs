using System;
using UnityEngine;

[Serializable]
public struct IntOrFloatObject : IEquatable<IntOrFloatObject>
{
    [SerializeField] private int intValue;
    [SerializeField] private float floatValue;
    [SerializeField] private bool isInt;

    public bool IsInt
    {
        get
        {
            return isInt;
        }
        set
        {
            if (value)
            {
                if (isInt)
                    return;

                intValue = Mathf.RoundToInt(floatValue);
            }
            else
            {
                if (!isInt)
                    return;

                floatValue = intValue;
            }

            isInt = value;
        }
    }

    public bool IsFloat
    {
        get
        {
            return !isInt;
        }
        set
        {
            IsInt = !value;
        }
    }

    public int IntValue => isInt ? intValue : Mathf.RoundToInt(floatValue);
    public float FloatValue => isInt ? intValue : floatValue;

    public object Value => isInt ? intValue : floatValue;

    // Constructors
    public IntOrFloatObject(int value)
    {
        isInt = true;
        intValue = value;
        floatValue = value;
    }

    public IntOrFloatObject(float value)
    {
        isInt = false;
        intValue = Mathf.RoundToInt(value);
        floatValue = value;
    }

    // Implicit conversions
    public static implicit operator IntOrFloatObject(int value) => new IntOrFloatObject(value);
    public static implicit operator IntOrFloatObject(float value) => new IntOrFloatObject(value);

    public static implicit operator float(IntOrFloatObject obj) => obj.FloatValue;
    public static implicit operator int(IntOrFloatObject obj) => obj.IntValue;

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
        if (a.IsFloat || b.IsFloat)
        {
            return new IntOrFloatObject(a.FloatValue + b.FloatValue);
        }
        return new IntOrFloatObject(a.IntValue + b.IntValue);
    } 
    public static IntOrFloatObject operator -(IntOrFloatObject a, IntOrFloatObject b)
    {
        if (a.IsFloat || b.IsFloat)
        {
            return new IntOrFloatObject(a.FloatValue - b.FloatValue);
        }
        return new IntOrFloatObject(a.IntValue - b.IntValue);
    }

    public static IntOrFloatObject operator *(IntOrFloatObject a, IntOrFloatObject b)
    {
        if (a.IsFloat || b.IsFloat)
        {
            return new IntOrFloatObject(a.FloatValue * b.FloatValue);
        }
        return new IntOrFloatObject(a.IntValue * b.IntValue);
    }
    public static IntOrFloatObject operator /(IntOrFloatObject a, IntOrFloatObject b)
    {
        if (a.IsFloat || b.IsFloat)
        {
            return new IntOrFloatObject(a.FloatValue / b.FloatValue);
        }
        return new IntOrFloatObject(a.IntValue / b.IntValue);
    }

    public override string ToString() => isInt ? intValue.ToString() : floatValue.ToString("0.###");
}