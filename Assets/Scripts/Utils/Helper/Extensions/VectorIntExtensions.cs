using System.Collections.Generic;
using UnityEngine;

public static class VectorIntExtensions
{
    public static List<Vector2Int> AddToAll(this List<Vector2Int> v2IntList, Vector2Int v2Int)
    {
        List<Vector2Int> returnList = new List<Vector2Int>();

        for (int i = 0; i < v2IntList.Count; i++)
        {
            returnList.Add(v2IntList[i] + v2Int);
        }

        return returnList;
    }



    public static List<Vector2Int> Rotate(this List<Vector2Int> v2Ints, float rotation)
    {
        List<Vector2Int> returnV2Ints = new List<Vector2Int>();

        for (int i = 0; i < v2Ints.Count; i++)
        {
            int x = Mathf.RoundToInt(v2Ints[i].x * Mathf.Cos(rotation * Mathf.Deg2Rad) - v2Ints[i].y * Mathf.Sin(rotation * Mathf.Deg2Rad));
            int y = Mathf.RoundToInt(v2Ints[i].x * Mathf.Sin(rotation * Mathf.Deg2Rad) + v2Ints[i].y * Mathf.Cos(rotation * Mathf.Deg2Rad));
            returnV2Ints.Add(new Vector2Int(x, y));
        }

        return returnV2Ints;
    }

    public static int Min(this Vector2Int v2)
    {
        return Mathf.Min(v2.x, v2.y);
    }

    public static int Max(this Vector2Int v2)
    {
        return Mathf.Max(v2.x, v2.y);
    }

    public static Vector2Int Bounds(this List<Vector2Int> v2)
    {
        if (v2 == null || v2.Count == 0)
            return Vector2Int.zero;

        int minX = v2[0].x;
        int maxX = v2[0].x;
        int minY = v2[0].y;
        int maxY = v2[0].y;

        foreach (var point in v2)
        {
            minX = Mathf.Min(minX, point.x);
            maxX = Mathf.Max(maxX, point.x);
            minY = Mathf.Min(minY, point.y);
            maxY = Mathf.Max(maxY, point.y);
        }

        return new Vector2Int(maxX - minX, maxY - minY);
    }

    public static int RandomBetweenTwoValuesInclusive(this Vector2Int v2)
    {
        return Random.Range(Mathf.Min(v2.x, v2.y), Mathf.Max(v2.x, v2.y) + 1);
    }
}
