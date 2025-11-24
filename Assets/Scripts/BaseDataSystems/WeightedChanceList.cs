using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using System.Collections;

[Serializable]
public class WeightedChanceList<T>
{
    public List<WeightedChanceObject<T>> weightedList;

    public WeightedChanceList()
    {
        weightedList = new List<WeightedChanceObject<T>>();
    }

    public WeightedChanceList(WeightedChanceList<T> other)
    {
        weightedList = new List<WeightedChanceObject<T>>(other.weightedList);
    }

    public float GetChance(T item)
    {
        if (weightedList.Count <= 0)
        {
            return 0;
        }

        int totalChance = 0;

        foreach (WeightedChanceObject<T> obj in weightedList)
        {
            totalChance += obj.weight;
        }

        for (int i = 0; i < weightedList.Count; i++)
        {
            if (item.Equals(weightedList[i].item)) 
            {
                return (float)weightedList[i].weight / (float)totalChance;
            }
        }

        return 0;
    }

    public Dictionary<T, float> GetItemAndPercentages()
    {
        Dictionary<T, float> returnDictionary = new Dictionary<T, float>();

        if (weightedList == null || weightedList.Count <= 0)
            return returnDictionary;

        int totalChance = 0;

        foreach (WeightedChanceObject<T> obj in weightedList)
        {
            totalChance += obj.weight;
        }

        foreach(WeightedChanceObject<T> item in weightedList)
        {
            returnDictionary.Add(item.item, (float)item.weight / (float)totalChance);
        }

        return returnDictionary;
    }

    public T GetRandom()
    {
        if (weightedList == null || weightedList.Count == 0)
            return default;

        int count = weightedList.Count;
        int[] weights = new int[count];
        int total = 0;

        // Compute weight array ONCE
        for (int i = 0; i < count; i++)
        {
            int w = weightedList[i].weight;
            weights[i] = w;
            total += w;
        }

        if (total <= 0)
            return default;

        int roll = UnityEngine.Random.Range(0, total);
        int cumulative = 0;

        // Weighted selection
        for (int i = 0; i < count; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative)
                return weightedList[i].item;
        }

        // Safety: should never happen
        return weightedList[count - 1].item;
    }

    public T GetRandom(float time)
    {
        if (weightedList == null || weightedList.Count == 0)
            return default;

        int count = weightedList.Count;
        int[] weights = new int[count];
        int total = 0;

        for (int i = 0; i < count; i++)
        {
            int w = weightedList[i].Weight(time);
            weights[i] = w;
            total += w;
        }

        if (total <= 0)
            return default;

        int roll = UnityEngine.Random.Range(0, total);
        int cumulative = 0;

        for (int i = 0; i < count; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative)
                return weightedList[i].item;
        }

        return weightedList[count - 1].item;
    }

    public T GetRandom(float time, float lerpFactor)
    {
        if (weightedList.Count <= 0)
            return default(T);

        int totalChance = 0;

        foreach (WeightedChanceObject<T> obj in weightedList)
        {
            totalChance += obj.Weight(time, lerpFactor);
        }

        int index = UnityEngine.Random.Range(1, totalChance + 1);

        foreach (WeightedChanceObject<T> obj in weightedList)
        {
            index -= obj.Weight(time, lerpFactor);
            if (index <= 0)
                return obj.item;
        }

        return weightedList[0].item;
    }

    public void Add(T item, int weight)
    {
        if (weightedList == null)
            weightedList = new List<WeightedChanceObject<T>>();

        weightedList.Add(new WeightedChanceObject<T>() { item = item, weight = weight});
    }

    public void Clear()
    {
        if (weightedList != null)
            weightedList.Clear();
    }

    public void Remove(T item)
    {
        weightedList.RemoveAll(w => Equals(w.item, item));
    }

    private IEnumerator RemoveNextFrame(T item)
    {
        yield return null;
        weightedList.RemoveAll(w => Equals(w.item, item));
    }

    public void RemoveZeros()
    {
        if (weightedList == null)
            return;

        for (int i = 0; i < weightedList.Count; i++)
        {
            if(weightedList[i].weight <= 0)
            {
                weightedList.RemoveAt(i);
                i--;
            }
        }
    }

    public int this[T obj]
    {
        get
        {
            for (int i = 0; i < weightedList.Count; i++)
            {
                if(weightedList[i].item.Equals(obj))
                {
                    return weightedList[i].weight;
                }
            }

            throw new Exception("Object not in list");
        }
        set
        {
            for (int i = 0; i < weightedList.Count; i++)
            {
                if (weightedList[i].item.Equals(obj))
                {
                    weightedList[i].weight = value;
                }
            }
        }
    }
}