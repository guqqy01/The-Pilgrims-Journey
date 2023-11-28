using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static Color SetAlpha(this Color color, float alpha)
    {
        Color holder = new Color(color.r, color.g, color.b, alpha);
        return holder;
    }

    public static List<Vector2Int> AddToAll(this List<Vector2Int> v2IntList, Vector2Int v2Int)
    {
        List<Vector2Int> returnList = new List<Vector2Int>();

        for (int i = 0; i < v2IntList.Count; i++)
        {
            returnList.Add(v2IntList[i] + v2Int);
        }

        return returnList;
    }
}