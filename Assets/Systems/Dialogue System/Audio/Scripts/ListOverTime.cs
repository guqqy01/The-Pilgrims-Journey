using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class ListOverTime<T> where T : IListOverTimeItem
{
    private List<T> list = new List<T>();

    private float GetAvaliableTime(float length)
    {
        list.Sort();

        for (int i = 0; i < length; i++)
        {
            if (i == length - 1)
                return list[i].EndTime;

            if (list[i].EndTime - list[i + 1].StartTime >= length)
            {
                return list[i].EndTime; 
            }
        }

        return 0f;
    }

    private float GetAvaliableTime(float length, float preferedTime)
    {
        list.Sort();

        if (NoOverlaps(preferedTime, length))
            return preferedTime;

        for (int i = 0; i < length; i++)
        {
            if (i == length - 1)
                return list[i].EndTime;

            if (preferedTime > list[i].EndTime)
                continue;

            if (list[i].EndTime - list[i + 1].StartTime >= length)
            {
                return list[i].EndTime;
            }
        }

        return 0f;
    }

    private bool NoOverlaps(float length, float time)
    {
        foreach(var item in list)
        {
            if (item.Overlap(time, length))
                return false;
        }

        return true;
    }

    public void AddInOpening(T item)
    {
        list.Sort();
        item.StartTime = GetAvaliableTime(item.Length);
        list.Add(item);
    }
    public void AddAtEnd(T item)
    {
        list.Sort();
        if(list.Count > 0)
        {
            item.StartTime = list.Last().EndTime;
        }
        else
        {
            item.StartTime = 0f;
        }
        list.Add(item);

    }
    public void Add(T item, float time)
    {
        item.StartTime = GetAvaliableTime(item.Length, time);
        list.Add(item);
    }
}
