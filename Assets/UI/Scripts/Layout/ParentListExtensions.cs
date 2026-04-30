using System;
using System.Collections.Generic;

public static class ParentListExtensions
{
    public static ParentList<T> ToParentList<T>(this IEnumerable<T> enumerable, Func<T, T> parentFunction) where T : IParentListItem<T>
    {
        ParentList<T> returnList = new ParentList<T>();

        foreach (T item in enumerable)
        {
            item.Parent = parentFunction(item);
            returnList.Add(item);
        }

        return returnList;
    }

    public static ParentList<T> ToParentList<T>(this IEnumerable<T> enumerable) where T : IParentListItem<T>
    {
        ParentList<T> returnList = new ParentList<T>();

        foreach (T item in enumerable)
        {
            returnList.Add(item);
        }

        return returnList;
    }
}
