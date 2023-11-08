using System.Collections.Generic;
using UnityEngine;
using System;

namespace Map
{
    [Serializable]
    public class WeightMap
    {
        public MapWeightPoint[] points;

        public Dictionary<Vector2Int, int> coordsAndIndex;

        public WeightMap(int xSize, int ySize)
        {
            points = new MapWeightPoint[xSize * ySize];

            coordsAndIndex = new Dictionary<Vector2Int, int>();

            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    coordsAndIndex.Add(new Vector2Int(i, j), i * ySize + j);
                    points[i * ySize + j] = new MapWeightPoint(new Vector2Int(i, j), 0);
                }
            }

        }

        public int this[int i, int j]
        {
            get
            {
                return points[coordsAndIndex[new Vector2Int(i, j)]].weight;
            }
            set
            {
                points[coordsAndIndex[new Vector2Int(i, j)]].weight = value;
            }
        }

        public Vector2Int this[int i]
        {
            get
            {
                return points[i].coords;
            }
            set
            {
                points[i].coords = value;
            }
        }

        public void Sort()
        {
            Array.Sort(points);

            coordsAndIndex.Clear();

            for (int i = 0; i < points.Length; i++)
            {
                coordsAndIndex.Add(points[i].coords, i);
            }
        }
    }
}


