using System;
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

    public static (int x, int y) Get2DIndex<T>(this T[] array, int width, int index)
    {
        if (array == null || array.Length == 0)
            throw new ArgumentException("Array is null or empty.", nameof(array));
        if (width <= 0 || array.Length % width != 0)
            throw new ArgumentException($"Array length {array?.Length} is not divisible by width {width}.", nameof(width));

        int height = array.Length / width;
        if (index < 0 || index >= height * width)
            throw new ArgumentOutOfRangeException(nameof(index), $"index: {index}, height: {height}, width: {width}");

        int x = index % width;
        int y = index / width;
        return (x, y);
    }

    public static bool IsInBounds(this Array source, params int[] indices)
    {
        if (indices.Length != source.Rank)
            throw new ArgumentException($"Expected {source.Rank} indices, got {indices.Length}.");

        for (int i = 0; i < source.Rank; i++)
        {
            if (indices[i] < source.GetLowerBound(i) || indices[i] > source.GetUpperBound(i))
                return false;
        }
        return true;
    }

    public static bool IsInBounds<T>(this IEnumerable<T> source, int index)
    {
        if(source == null)
            return false;

        if (index < 0 || index > source.Count() - 1)
            return false;

        return true;
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

    public enum AddNewElementType
    {
        Default,
        LastElement,
        CustomFunction,
    }

    public static void MatchAmount<A, B>(this List<A> source, ICollection<B> other, AddNewElementType addNewElementType, Func<A> getDefaultCall = null)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (other == null)
            throw new ArgumentNullException(nameof(other));

        int sourceCount = source.Count;
        int otherCount = other.Count;

        if (sourceCount == otherCount)
        {
            return;
        }

        if (sourceCount < otherCount)
        {
            for (int i = 0; i < otherCount - sourceCount; i++)
            {
                A addA = addNewElementType switch
                {
                    AddNewElementType.LastElement => source.Last(),
                    AddNewElementType.Default => default,
                    AddNewElementType.CustomFunction => getDefaultCall != null ? getDefaultCall() : default,
                    _ => default,
                };

                source.Add(addA);
            }
        }
        else
        {
            for (int i = 0; i < sourceCount - otherCount; i++)
            {
                source.RemoveFromEnd();
            }
        }
    }

    public static void MatchAmount<A, B>(this List<A> source, ICollection<B> other, Func<A> getDefaultCall = null)
    {
        if(source == null)
            throw new ArgumentNullException(nameof(source));

        if(other == null)
            throw new ArgumentNullException(nameof(other));

        int sourceCount = source.Count;
        int otherCount = other.Count;

        if (sourceCount == otherCount)
        {
            return;
        }

        if(sourceCount < otherCount)
        {
            for (int i = 0; i < otherCount - sourceCount; i++)
            {
                A addA = default;

                if(getDefaultCall != null)
                {
                    addA = getDefaultCall();
                }

                source.Add(addA);
            }
        }
        else
        {
            for (int i = 0; i < sourceCount - otherCount; i++)
            {
                source.RemoveFromEnd();
            }
        }
    }

    public static void RemoveFromEnd<T>(this List<T> list)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));

        list.RemoveAt(list.Count - 1);
    }

    public static T GetRandom<T>(this List<T> list)
    {
        if(list == null || list.Count == 0)
        {
            UnityEngine.Debug.LogWarning("Get random failed, List either null or had no elements");
            return default;
        }

        return list[UnityEngine.Random.Range(0, list.Count)];
    }
}
