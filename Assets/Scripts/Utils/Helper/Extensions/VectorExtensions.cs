using System.Collections.Generic;
using UnityEngine;

public static class VectorExtensions
{
    public static List<Vector2> SplitConeDirections(this Vector2 mainDirection, int numOfSplits, float coneAngleRad)
    {
        if (numOfSplits <= 0)
            return new List<Vector2>();

        if (numOfSplits == 1)
            return new List<Vector2> { mainDirection };

        List<Vector2> returnList = new List<Vector2>();

        float angleDeltaRad = coneAngleRad / (numOfSplits - 1);

        Vector2 currentDirection = mainDirection.RotateCounterClockwise(coneAngleRad / 2f);

        for (int i = 0; i < numOfSplits; i++)
        {
            returnList.Add(currentDirection);

            currentDirection = currentDirection.RotateClockwise(angleDeltaRad);
        }

        return returnList;
    }

    public static List<Vector2> SplitConeDirectionsAlwaysHaveMainDirection(this Vector2 mainDirection, int numOfSplits, float coneAngleRad)
    {
        if (numOfSplits <= 0)
            return new List<Vector2>();

        if (numOfSplits.Odd())
            return SplitConeDirections(mainDirection, numOfSplits, coneAngleRad);

        List<Vector2> returnList = SplitConeDirections(mainDirection, numOfSplits + 1, coneAngleRad);

        int ran = Random.Range(0, 2);

        if (ran == 0)
            returnList.RemoveAt(0);
        else
            returnList.RemoveAt(returnList.Count - 1);

        return returnList;
    }

    public static bool Approximately(this Vector2 v, Vector2 other)
    {
        return Mathf.Approximately(v.x, other.x) && Mathf.Approximately(v.y, other.y);
    }

    public static Vector2 Middle(this Vector2 v1, Vector2 v2)
    {
        return (v2 + v1) / 2f;
    }

    public static Vector2 RotateCounterClockwise(this Vector2 v2, float angleRad)
    {
        float x = v2.x * Mathf.Cos(angleRad) - v2.y * Mathf.Sin(angleRad);
        float y = v2.x * Mathf.Sin(angleRad) + v2.y * Mathf.Cos(angleRad);

        return new Vector2(x, y);
    }

    public static Vector2 RotateClockwise(this Vector2 v2, float angleRad)
    {
        return RotateCounterClockwise(v2, -angleRad);
    }

    public static bool InBetweenValues(this Vector2 v2, float value)
    {
        return value > v2.Min() && value < v2.Max();
    }

    public static float Min(this Vector2 v2)
    {
        return Mathf.Min(v2.x, v2.y);
    }

    public static float Max(this Vector2 v2)
    {
        return Mathf.Max(v2.x, v2.y);
    }

    public static float RandomBetweenTwoValues(this Vector2 v2)
    {
        return Random.Range(Mathf.Min(v2.x, v2.y), Mathf.Max(v2.x, v2.y));
    }

    public static Vector2Int V2ToV2Int(this Vector2 v2)
    {
        return new Vector2Int(Mathf.RoundToInt(v2.x), Mathf.RoundToInt(v2.y));
    }
}
