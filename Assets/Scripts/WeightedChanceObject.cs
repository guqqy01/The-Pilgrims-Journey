using System;
using UnityEngine;
[Serializable]
public struct WeightedChanceObject<T>
{
    public T item;
    [Min(0)]
    public int weight;
}
