using Codice.CM.WorkspaceServer.Tree;
using System;
using UnityEngine;

public static class MathExtensions
{
    public static float Lerp(float v1, float v2, float t)
    {
        return v1 + (v2 - v1) * t;
    }

    public static float QuadraticBezier(float v0, float v1, float v2, float t)
    {
        float value = v1 + Mathf.Pow((1f - t), 2f) * (v0 - v1) + Mathf.Pow(t, 2f) * (v2 - v1);
        Debug.Log($"v0: {v0}, v1: {v1}, v2: {v2}, t: {t}, value: {value}");

        return value;
    }

    public static float LerpBetweenValues(this Vector2 vector, float t)
    {
        return Lerp(vector.x, vector.y, t);
    }

    public static Vector3 QuadraticBezier(Vector3 v1, Vector3 v2, Vector3 v3, float t)
    {
        return Mathf.Pow(1f - t, 2f) * v1 + 2f * (1f - t) * v2 + Mathf.Pow(t, 2f) * v3;
    }

    public static float RoundTo(this float value, float roundingValue)
    {
        if(roundingValue == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(roundingValue), "Rounding value cannot be zero");
        }

        roundingValue = Mathf.Abs(roundingValue);
        float numberOfDivisions = value / roundingValue;
        int numberOfDivisionsRounded = Mathf.RoundToInt(numberOfDivisions);
        return numberOfDivisionsRounded * roundingValue;
    }
}
