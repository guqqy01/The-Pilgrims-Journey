using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public static class ExtensionsAndHelper
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

    public static object GetRandom(this System.Array array)
    {
        return array.GetValue(Random.Range(0, array.Length));
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

    public static int RandomSign()
    {
        List<int> holder = new List<int>() { -1, 1};

        return holder.GetRandom();
    }

    public static Vector2Int V2ToV2Int(this Vector2 v2)
    {
        return new Vector2Int(Mathf.RoundToInt(v2.x), Mathf.RoundToInt(v2.y));
    }
}