using System;
using UnityEngine;

[Serializable]
public struct IntOrFloatObject
{
    [SerializeField]
    private int intValue;
    [SerializeField]
    private float floatValue;

    [SerializeField]
    private bool isInt;

    public bool IsInt
    {
        get
        {
            return isInt;
        }
        set
        {
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
            isInt = !value;
        }
    }

    public int IntValue => IsInt ? (int)intValue : throw new Exception("object is not an int");
    public float FloatValue =>  IsFloat ? (float)floatValue : throw new Exception("object is not a float");

    public object Value
    {
        get
        {
            if(IsInt)
            {
                return IntValue;
            }
            else if(IsFloat)
            {
                return FloatValue;
            }

            return null;
        }
    }

    public IntOrFloatObject(int intValue)
    {
        isInt = true;
        this.intValue = intValue;
        floatValue = 0;
    }

    public IntOrFloatObject(float floatValue)
    {
        isInt = false;
        intValue = 0;
        this.floatValue = floatValue;
    }

    public IntOrFloatObject(object value)
    {
        
        if(value is float)
        {
            isInt = false;
            intValue = 0;
            this.floatValue = (float)value;
            return;
        }
        else if(value is int)
        {
            isInt = true;
            this.intValue = (int)value;
            floatValue = 0;
            return;
        }

        throw new Exception();
    }

    public static bool operator >(IntOrFloatObject obj1, IntOrFloatObject obj2)
    {
        if (obj1.IsInt && obj2.IsInt)
        {
            return obj1.IntValue > obj2.IntValue;
        }
        else if (obj1.IsFloat && obj2.IsFloat)
        {
            return obj1.FloatValue > obj2.FloatValue;
        }

        throw new Exception("Null");
    }

    public static bool operator <(IntOrFloatObject obj1, IntOrFloatObject obj2)
    {
        if (obj1.IsInt && obj2.IsInt)
        {
            return obj1.IntValue < obj2.IntValue;
        }
        else if (obj1.IsFloat && obj2.IsFloat)
        {
            return obj1.FloatValue < obj2.FloatValue;
        }

        throw new Exception("Null");
    }

    public static bool operator >=(IntOrFloatObject obj1, float f)
    {
        if (obj1.IsInt)
        {
            return obj1.intValue >= f;
        }
        else
        {
            return obj1.floatValue >= f;
        }
    }

    public static bool operator <=(IntOrFloatObject obj1, float f)
    {
        if (obj1.IsInt)
        {
            return obj1.intValue <= f;
        }
        else
        {
            return obj1.floatValue <= f;
        }
    }

    public static bool operator >=(float f, IntOrFloatObject obj1)
    {
        if (obj1.IsInt)
        {
            return f >= obj1.intValue;
        }
        else
        {
            return f >= obj1.floatValue;
        }
    }

    public static bool operator <=(float f, IntOrFloatObject obj1)
    {
        if (obj1.IsInt)
        {
            return f <= obj1.intValue;
        }
        else
        {
            return f <= obj1.floatValue;
        }
    }

    public static bool operator >(IntOrFloatObject obj1, int f)
    {
        if (obj1.IsInt)
        {
            return obj1.intValue > f;
        }
        else
        {
            return obj1.floatValue > f;
        }
    }

    public static bool operator <(IntOrFloatObject obj1, int f)
    {
        if (obj1.IsInt)
        {
            return obj1.intValue < f;
        }
        else
        {
            return obj1.floatValue < f;
        }
    }

    public static bool operator >=(IntOrFloatObject obj1, int f)
    {
        if (obj1.IsInt)
        {
            return obj1.intValue >= f;
        }
        else
        {
            return obj1.floatValue >= f;
        }
    }

    public static bool operator <=(IntOrFloatObject obj1, int f)
    {
        if (obj1.IsInt)
        {
            return obj1.intValue <= f;
        }
        else
        {
            return obj1.floatValue <= f;
        }
    }

    public static bool operator ==(IntOrFloatObject obj1, int f)
    {
        if (obj1.IsInt)
        {
            return obj1.intValue == f;
        }
        else
        {
            return obj1.floatValue == f;
        }
    }

    public static bool operator !=(IntOrFloatObject obj1, int f)
    {
        if (obj1.IsInt)
        {
            return obj1.intValue != f;
        }
        else
        {
            return obj1.floatValue != f;
        }
    }

    public static IntOrFloatObject operator +(IntOrFloatObject obj1, IntOrFloatObject obj2)
    {
        if(obj1.IsInt && obj2.IsInt)
        {
            return new IntOrFloatObject(obj1.IntValue + obj2.IntValue);
        }
        else if (obj1.IsFloat && obj2.IsFloat)
        {
            return new IntOrFloatObject(obj1.FloatValue + obj2.FloatValue);
        }

        throw new Exception();
    }

    public static IntOrFloatObject operator -(IntOrFloatObject obj1, IntOrFloatObject obj2)
    {
        if (obj1.IsInt && obj2.IsInt)
        {
            return new IntOrFloatObject(obj1.IntValue - obj2.IntValue);
        }
        else if (obj1.IsFloat && obj2.IsFloat)
        {
            return new IntOrFloatObject(obj1.FloatValue - obj2.FloatValue);
        }

        throw new Exception("Null");
    }

    public static IntOrFloatObject operator /(IntOrFloatObject obj1, IntOrFloatObject obj2)
    {
        if (obj1.IsInt && obj2.IsInt)
        {
            return new IntOrFloatObject(obj1.IntValue / obj2.IntValue);
        }
        else if (obj1.IsFloat && obj2.IsFloat)
        {
            return new IntOrFloatObject(obj1.FloatValue / obj2.FloatValue);
        }

        throw new Exception("Null");
    }

    public static IntOrFloatObject operator * (float f, IntOrFloatObject obj)
    {
        if (obj.IsInt)
        {
            return new IntOrFloatObject(Mathf.RoundToInt(obj.IntValue * f));
        }
        else if (obj.IsFloat)
        {
            return new IntOrFloatObject(obj.FloatValue * f);
        }

        throw new Exception("Null");
    }

    public static IntOrFloatObject operator *(IntOrFloatObject obj1, IntOrFloatObject obj2)
    {
        if (obj1.IsInt && obj2.IsInt)
        {
            return new IntOrFloatObject(obj1.IntValue * obj2.IntValue);
        }
        else if (obj1.IsFloat && obj2.IsFloat)
        {
            return new IntOrFloatObject(obj1.FloatValue * obj2.FloatValue);
        }

        throw new Exception("Null");
    }

    public static IntOrFloatObject operator *(IntOrFloatObject obj, float f)
    {
        if (obj.IsInt)
        {
            return new IntOrFloatObject(Mathf.CeilToInt(obj.IntValue * f));
        }
        else if (obj.IsFloat)
        {
            return new IntOrFloatObject(obj.FloatValue * f);
        }

        throw new Exception("Null");
    }

    public static bool operator !=(IntOrFloatObject obj1, IntOrFloatObject obj2)
    {
        if (obj1.IsInt && obj2.IsInt)
        {
            return obj1.IntValue != obj2.IntValue;
        }
        else if (obj1.IsFloat && obj2.IsFloat)
        {
            return obj1.FloatValue != obj2.FloatValue;
        }

        throw new Exception("Null");
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(IntOrFloatObject obj1, IntOrFloatObject obj2)
    {
        if (obj1.IsInt && obj2.IsInt)
        {
            return obj1.IntValue == obj2.IntValue;
        }
        else if (obj1.IsFloat && obj2.IsFloat)
        {
            return obj1.FloatValue == obj2.FloatValue;
        }

        throw new Exception("Null");
    }

    public static bool operator !=(IntOrFloatObject obj1, float f)
    {
        if (obj1.IsInt)
        {
            return obj1.IntValue != f;
        }
        else if (obj1.IsFloat)
        {
            return obj1.FloatValue != f;
        }

        throw new Exception("Null");
    }

    public static bool operator ==(IntOrFloatObject obj1, float f)
    {
        if (obj1.IsInt)
        {
            return obj1.IntValue == f;
        }
        else if (obj1.IsFloat)
        {
            return obj1.FloatValue == f;
        }

        throw new Exception("Null");
    }
}