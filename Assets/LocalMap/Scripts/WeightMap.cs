using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;

namespace LocalMap
{
    [Serializable]
    public class WeightMap
    {
        private readonly Dictionary<int, MapWeightPoint[]> rangeAndPoints;

        private readonly Dictionary<int, Dictionary<Vector2Int, int>> rangeAndCoordsAndIndex;

        public WeightMap(int maxRange, int xSize, int ySize)
        {
            rangeAndPoints = new Dictionary<int, MapWeightPoint[]>();
            rangeAndCoordsAndIndex = new Dictionary<int, Dictionary<Vector2Int, int>>();

            for (int range = 1; range <= maxRange; range++)
            {

                MapWeightPoint[] points = new MapWeightPoint[xSize * ySize];
                Dictionary<Vector2Int, int> coordsAndIndex = new Dictionary<Vector2Int, int>();

                for (int i = 0; i < xSize; i++)
                {
                    for (int j = 0; j < ySize; j++)
                    {
                        coordsAndIndex.Add(new Vector2Int(i, j), i * ySize + j);
                        points[i * ySize + j] = new MapWeightPoint(new Vector2Int(i, j), 0);
                    }
                }
                rangeAndPoints.Add(range, points);
                rangeAndCoordsAndIndex.Add(range, coordsAndIndex);
            }
        }

        public List<KeyValuePair<int, List<Vector2Int>>> WeightAndMapPoints(int range)
        {
            var result = new Dictionary<int, List<Vector2Int>>();

            // Defensive: ensure rangeAndPoints and entry exist
            if (rangeAndPoints == null) return new List<KeyValuePair<int, List<Vector2Int>>>();
            if (range < 0 || range >= rangeAndPoints.Count) return new List<KeyValuePair<int, List<Vector2Int>>>();

            var mapPointsAtRange = rangeAndPoints[range];
            if (mapPointsAtRange == null || mapPointsAtRange.Length == 0)
                return new List<KeyValuePair<int, List<Vector2Int>>>();

            for (int i = 0; i < mapPointsAtRange.Length; i++)
            {
                var mp = mapPointsAtRange[i];
                if (!result.TryGetValue(mp.weight, out var list))
                {
                    list = new List<Vector2Int>();
                    result[mp.weight] = list;
                }
                list.Add(mp.coords);
            }

            // Convert to a list of KeyValuePair and order descending by key (highest weight first)
            var ordered = result
                .OrderByDescending(kv => kv.Key)
                .ToList();

            return ordered;
        }


        public int this[int range, int i, int j]
        {
            get
            {
                return rangeAndPoints[range][rangeAndCoordsAndIndex[range][new Vector2Int(i, j)]].weight;
            }
            set
            {
                rangeAndPoints[range][rangeAndCoordsAndIndex[range][new Vector2Int(i, j)]].weight = value;
            }
        }

        public Vector2Int this[int range, int i]
        {
            get
            {
                return rangeAndPoints[range][i].coords;
            }
            set
            {
                rangeAndPoints[range][i].coords = value;
            }
        }

        public void Sort()
        {
            List<int> keyArray = rangeAndPoints.Keys.ToList();

            for (int i = 0; i < keyArray.Count; i++)
            {
                Array.Sort(rangeAndPoints[keyArray[i]]);

                rangeAndCoordsAndIndex[keyArray[i]].Clear();

                for (int j = 0; j < rangeAndPoints[keyArray[i]].Length; j++)
                {
                    rangeAndCoordsAndIndex[keyArray[i]].Add(rangeAndPoints[keyArray[i]][j].coords, i);
                }
            } 
        }
    }
}


