using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public static class OperationOrderHelper
{
    private static SortedDictionary<int, List<Action>> list = new SortedDictionary<int, List<Action>>();

    public static void Execute()
    {
        foreach (KeyValuePair<int, List<Action>> item in list)
        {
            foreach (Action action in item.Value)
            {
                action?.Invoke();
            }
        }

        list.Clear();
    }

    public static void Reset()
    {
        list.Clear();
    }

    public static void Add(int order, Action action)
    {
        if (list.ContainsKey(order))
        {
            list[order].Add(action);
        }
        else
        {
            list.Add(order, new List<Action> { action });
        }
    }

    public static void Remove(Action action)
    {
        foreach (KeyValuePair<int, List<Action>> item in list)
        {
            if (item.Value.Contains(action))
            {
                item.Value.Remove(action);
                return;
            }
        }
    }
}




