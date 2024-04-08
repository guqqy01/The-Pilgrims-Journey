using System.Collections.Generic;
using System;
using UnityEditor;

[Serializable]
public struct WeightedChanceList<T>
{
    public List<WeightedChanceObject<T>> list;

    public float GetChance(T item)
    {
        if (list.Count <= 0)
            throw new Exception("List not populated");

        int totalChance = 0;

        foreach (WeightedChanceObject<T> obj in list)
        {
            totalChance += obj.weight;
        }

        for (int i = 0; i < list.Count; i++)
        {
            if (item.Equals(list[i].item)) 
            {
                return (float)list[i].weight / (float)totalChance;
            }
        }

        return 0;
    }

    public Dictionary<T, float> GetItemAndPercentages()
    {
        Dictionary<T, float> returnDictionary = new Dictionary<T, float>();

        if (list == null || list.Count <= 0)
            return returnDictionary;

        int totalChance = 0;

        foreach (WeightedChanceObject<T> obj in list)
        {
            totalChance += obj.weight;
        }

        foreach(WeightedChanceObject<T> item in list)
        {
            returnDictionary.Add(item.item, (float)item.weight / (float)totalChance);
        }

        return returnDictionary;
    }

    public T GetRandom()
    {
        if (list.Count <= 0)
            throw new Exception("List not populated");

        int totalChance = 0;

        foreach(WeightedChanceObject<T> obj in list)
        {
            totalChance += obj.weight;
        }

        int index = UnityEngine.Random.Range(1, totalChance);

        foreach (WeightedChanceObject<T> obj in list)
        {
            index -= obj.weight;
            if (index <= 0)
                return obj.item;
        }

        return list[0].item;
    }

    public void Add(T item, int weight)
    {
        if (list == null)
            list = new List<WeightedChanceObject<T>>();

        list.Add(new WeightedChanceObject<T>() { item = item, weight = weight});
    }

    public void RemoveZeros()
    {
        if (list == null)
            return;

        for (int i = 0; i < list.Count; i++)
        {
            if(list[i].weight <= 0)
            {
                list.RemoveAt(i);
                i--;
            }
        }
    }
}