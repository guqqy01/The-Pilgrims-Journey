using UnityEngine;

public static class FloatExtensions
{
    public static float RandomSign(this float value)
    {
        float random = Random.Range(1, 3);
        int randomSign = 1;

        if (random == 2)
            randomSign = -1;

        return value * randomSign;
    }

    public static bool IsWithin(this float value, float f1, float f2)
    {
        float min = Mathf.Min(f1, f2);
        float max = Mathf.Max(f1, f2);

        return min <= value && value <= max;
    }

    public static bool Negative(this float value)
    {
        return (value < 0);
    }

    public static bool Positive(this float value)
    {
        return (value > 0);
    }

    public static bool CloseTo(this float f, float other, float limit = 0.0001f)
    {
        if (f == other) return true;
        if (f > other)
        {
            float newF = f - limit;
            if (newF <= other) return true;
        }
        else
        {
            float newF = f + limit;
            if (newF >= other) return true;
        }

        return false;
    }
}
