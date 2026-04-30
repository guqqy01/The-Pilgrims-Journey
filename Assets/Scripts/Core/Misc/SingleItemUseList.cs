using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleItemUseList<T> where T : class
{
    private List<T> avaliable = new List<T>();
    private List<T> unavaliable = new List<T>();

    public Dictionary<T, bool> ItemsAndIfAvaliable
    {
        get
        {
            Dictionary<T, bool> returnDictionary = new Dictionary<T, bool>();

            foreach (var item in avaliable)
            {
                returnDictionary.Add(item, true);
            }
            foreach (var item in unavaliable)
            {
                returnDictionary.Add(item, false);
            }

            return returnDictionary;
        }
    }

    public void Add(T item)
    {
        avaliable.Add(item);
    }

    public bool Remove(T item)
    {
        return avaliable.Remove(item) | unavaliable.Remove(item);
    }

    public T GetAvaliableItemOrNull()
    {
        if(avaliable.Count > 0)
        {
            T item = avaliable[0];

            MakeUnavaliable(item);

            return item;
        }

        return null;
    }

    public void MakeAvaliable(T item)
    {
        if (unavaliable.Contains(item))
        {
            unavaliable.Remove(item);
            avaliable.Add(item);
        }
    }

    public void MakeUnavaliable(T item)
    {
        if (avaliable.Contains(item))
        {
            unavaliable.Add(item);
            avaliable.Remove(item);
        }
    }

}

