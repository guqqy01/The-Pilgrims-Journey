using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
namespace Map
{
    [Serializable]
    public class WeightMap
    {
        public Dictionary<int, MapWeightPoint[]> rangeAndPoints;

        public Dictionary<int, Dictionary<Vector2Int, int>> rangeAndCoordsAndIndex;

        public WeightMap(int maxRange, int xSize, int ySize)
        {
            rangeAndPoints = new Dictionary<int, MapWeightPoint[]>();
            rangeAndCoordsAndIndex = new Dictionary<int, Dictionary<Vector2Int, int>>();

            for (int range = 1; range <= maxRange; range++)
            {

                MapWeightPoint[] points = new MapWeightPoint[xSize * ySize];
                Dictionary<Vector2Int, int> rangeDictionary = new Dictionary<Vector2Int, int>();

                for (int i = 0; i < xSize; i++)
                {
                    for (int j = 0; j < ySize; j++)
                    {
                        rangeDictionary.Add(new Vector2Int(i, j), i * ySize + j);
                        points[i * ySize + j] = new MapWeightPoint(new Vector2Int(i, j), 0);
                    }
                }
                rangeAndPoints.Add(range, points);
                rangeAndCoordsAndIndex.Add(range, rangeDictionary);
            }

            

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


