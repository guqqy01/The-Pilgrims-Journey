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

    public static T GetRandom<T>(this List<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    public static int RandomSign(this int value)
    {
        float random = Random.Range(1, 3);
        int randomSign = 1;

        if (random == 2)
            randomSign = -1;

        return value * randomSign;
    }

    public static float RandomSign(this float value)
    {
        float random = Random.Range(1, 3);
        int randomSign = 1;

        if (random == 2)
            randomSign = -1;

        return value * randomSign;
    }
}