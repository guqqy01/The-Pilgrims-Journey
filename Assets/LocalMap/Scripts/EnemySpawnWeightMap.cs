using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Monsters;

namespace LocalMap
{
    [Serializable]
    public class EnemySpawnWeightMap
    {
        private readonly Dictionary<int, MapWeightPoint[]> rangeAndPoints;

        private readonly Dictionary<int, Dictionary<Vector2Int, int>> rangeAndCoordsAndIndex;

        public EnemySpawnWeightMap(Pathway pathway, IEnumerable<MonsterSpawnSettings> allSettings, int xSize, int ySize)
        {
            int maxRange = 0;

            foreach (var settings in allSettings)
            {
                foreach (var s in settings.rangeAndMonster)
                    maxRange = Mathf.Max(maxRange, s.range);

                foreach (var boss in settings.bossMonsters)
                {
                    maxRange = Mathf.Max(maxRange, boss.range);
                    foreach (var minion in boss.bossMinionSpawns)
                        maxRange = Mathf.Max(maxRange, minion.range);
                }
            }

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

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    // Skip tiles on path
                    if (pathway.ContainsStep(x, y))
                        continue;

                    if (!LocalMapGenerator.Instance.IsConnectedToPath(new Vector2Int(x, y)))
                        continue;

                    // Weight is based on proximity to the path
                    for (int r = 1; r <= maxRange; r++)
                        this[r, x, y] = 0;

                    for (int dx = -maxRange; dx <= maxRange; dx++)
                    {
                        for (int dy = -maxRange; dy <= maxRange; dy++)
                        {
                            if (pathway.ContainsStep(x + dx, y + dy))
                            {
                                int r = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
                                this[r, x, y] += 2 * maxRange - Mathf.Abs(dx) - Mathf.Abs(dy);
                            }
                        }
                    }
                }
            }

            Sort();
        }

        public List<KeyValuePair<int, List<Vector2Int>>> WeightAndMapPointsConnectedToPath(int range)
        {
            var result = new Dictionary<int, List<Vector2Int>>();

            if (rangeAndPoints == null)
                return new List<KeyValuePair<int, List<Vector2Int>>>();

            if (!rangeAndPoints.TryGetValue(range, out var mapPointsAtRange))
                return new List<KeyValuePair<int, List<Vector2Int>>>();

            if (mapPointsAtRange == null || mapPointsAtRange.Length == 0)
                return new List<KeyValuePair<int, List<Vector2Int>>>();

            for (int i = 0; i < mapPointsAtRange.Length; i++)
            {
                var mp = mapPointsAtRange[i];

                if (!LocalMapGenerator.Instance.IsConnectedToPath(mp.coords))
                    continue;

                if (!result.TryGetValue(mp.weight, out var list))
                {
                    list = new List<Vector2Int>();
                    result[mp.weight] = list;
                }
                list.Add(mp.coords);
            }

            return result
                .OrderByDescending(kv => kv.Key)
                .ToList();
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


