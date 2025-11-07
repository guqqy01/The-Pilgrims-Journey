using UnityEngine;
using System;

namespace LocalMap
{
    [Serializable]
    public struct MapWeightPoint : IComparable<MapWeightPoint>
    {
        public Vector2Int coords;
        public int weight;

        public MapWeightPoint(Vector2Int coords, int weight)
        {
            this.coords = coords;
            this.weight = weight;
        }

        public int CompareTo(MapWeightPoint other)
        {
            return weight.CompareTo(other.weight);
        }

        public static MapWeightPoint operator +(MapWeightPoint mwp, int weight)
        {
            return new MapWeightPoint(mwp.coords, mwp.weight + weight);
        } 
    }
}


