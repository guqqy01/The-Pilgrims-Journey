using System;
using UnityEngine;

public static class IntExtensions
{
    public static bool Odd(this int value)
    {
        return !Even(value);
    }

    public static bool Even(this int value)
    {
        return value % 2 == 0;
    }

    public static int RoundToNearedInt(this int value, int nearestInterger)
    {
        int remainder = value % nearestInterger;

        if (remainder >= nearestInterger / 2f)
            return value + (nearestInterger - remainder);
        else
            return value - remainder;
    }

    public static int RandomSign(this int value)
    {
        float random = UnityEngine.Random.Range(1, 3);
        int randomSign = 1;

        if (random == 2)
            randomSign = -1;

        return value * randomSign;
    }

    public static int RandomSign()
    {
        var holder = new System.Collections.Generic.List<int>() { -1, 1 };

        return holder.GetRandom();
    }

    public static bool Between(this int interger, int minInclusive, int maxInclusive)
    {
        if (interger < minInclusive || interger > maxInclusive)
            return false;

        return true;
    }
}
