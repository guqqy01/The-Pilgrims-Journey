using System.Collections.Generic;
using System.Linq;

public static class EnumerableExtensions
{
    public static int NotNullCount<T>(this IEnumerable<T> source) where T : class
    {
        if (source == null)
            return 0;

        int count = 0;
        foreach (var item in source)
        {
            if (item != null)
                count++;
        }
        return count;
    }

    public static object GetRandom(this System.Array array)
    {
        return array.GetValue(UnityEngine.Random.Range(0, array.Length));
    }

    public static List<T> GetRandomAmount<T>(this List<T> list, int amount, bool allowSame = false)
    {
        if (list == null || list.Count == 0)
            return new List<T>();

        List<T> returnlist = new List<T>();

        if (allowSame)
        {
            for (int i = 0; i < amount; i++)
            {
                returnlist.Add(list.GetRandom());
            }
        }
        else
        {
            List<T> copyOfList = new List<T>();

            copyOfList.AddRange(list);

            while (returnlist.Count < UnityEngine.Mathf.Min(amount, list.Count) && copyOfList.Count > 0)
            {
                T item = copyOfList.GetRandom();
                returnlist.Add(item);
                copyOfList.Remove(item);
            }
        }

        return returnlist;
    }

    public static T GetRandom<T>(this List<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }
}
