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

    public static float Min(this Vector2Int v2)
    {
        return Mathf.Min(v2.x, v2.y);
    }

    public static float Max(this Vector2Int v2)
    {
        return Mathf.Max(v2.x, v2.y);
    }

    public static int RandomBetweenTwoValuesInclusive(this Vector2Int v2)
    {
        return Random.Range(Mathf.Min(v2.x, v2.y), Mathf.Max(v2.x, v2.y) + 1);
    }
}
