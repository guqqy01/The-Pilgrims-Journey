using UnityEngine;
using System;

[Serializable]
public class DoubleInt
{
    [SerializeField]
    protected int value1;
    [SerializeField]
    protected int value2;

    public int Value1
    {
        get
        {
            return value1;
        }
        set
        {
            value1 = value;

            onValueOneChange?.Invoke();
        }
    }

    public int Value2
    {
        get
        {
            return value2;
        }
        set
        {
            value2 = value;

            onValueTwoChange?.Invoke();
        }
    }

    public delegate void onValueChange();
    public onValueChange onValueOneChange;
    public onValueChange onValueTwoChange;
}

