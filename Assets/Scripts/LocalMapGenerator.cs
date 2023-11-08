using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using UnityEngine.UIElements;

namespace Map
{
    public class LocalMapGenerator : MonoBehaviour
    {
        [SerializeField]
        private Vector2Int mapSize = new Vector2Int(20, 20);

        [SerializeField]
        private float mapScale = 1f;

        [SerializeField]
        private Color gizmosColor;

        public Pathway pathway;

        public Vector2Int start = new Vector2Int(10, 0);

        public List<Direction> targetDirections;
        public Direction startDirection;

        public GameObject pathTile;
        public GameObject wallTile;

        public List<GameObject> tiles;
        public List<GameObject> enemys;


        public Vector2Int targetDirectionMinMax = new Vector2Int(2, 5);
        public Vector2Int traverseDirectionMinMax = new Vector2Int(2, 10);

        public GameObject enemy;

        public WeightMap weightMap;

        public int numberOfEnemys;

        [SerializeField]
        private int tileWeightRange;

        private void Start()
        {
            CreatePath(start, startDirection, targetDirections);

            CreateEnemyWeightMap(tileWeightRange);

            SpawnEnemys();
        }

        private void CreateEnemyWeightMap(int range)
        {
            weightMap = new WeightMap(mapSize.x, mapSize.y);

            for (int i = 0; i < mapSize.x; i++)
            {
                for (int j = 0; j < mapSize.y; j++)
                {
                    weightMap[i, j] = 0;

                    if (pathway.ContainsStep(i, j))
                        continue;

                    for (int x = - range; x < range; x++)
                    {
                        for (int y = - range; y < range; y++)
                        {
                            if (pathway.ContainsStep(x, y))
                            {
                                weightMap[i, j] += 2 * range - Mathf.Abs(i) - Mathf.Abs(j);
                            }
                        }
                    }
                }
            }

            weightMap.Sort();
        }

        private void SpawnEnemys()
        {
            for (int i = 0; i < enemys.Count; i++)
                DestroyImmediate(enemys[i]);

            enemys.Clear();

            for (int i = 0; i < numberOfEnemys; i++)
            {

            }
        }

        private Direction GetNextDirection(Direction currentDirection, Direction targetDirection, Vector2Int position)
        {
            if (targetDirection != currentDirection)
                return targetDirection;

            if (Mathf.Abs((int)currentDirection) == 2)
            {
                float dirRan = UnityEngine.Random.Range(1f / mapSize.x, 1f - 1f / mapSize.x);
                return (dirRan > (float)position.x / mapSize.x) ? Direction.Right : Direction.Left;
            }
            else
            {
                float dirRan = UnityEngine.Random.Range(1f / mapSize.y, 1f - 1f / mapSize.y);
                return (dirRan > (float)position.y / mapSize.y) ? Direction.Up : Direction.Down;
            }
        }

        

        public void CreatePathFromStartToTargetDirection(List<Vector2Int> path, Vector2Int start, Direction startDirection, Direction targetDirection)
        {
            Direction currentDirection = startDirection;
            int[] position = new int[2];

            position[0] = start.x;
            position[1] = start.y;

            int targetInt = (int)targetDirection > 0 ? (targetDirection == Direction.Right) ? mapSize.x : mapSize.y: 0;

            int GetDistanceToTarget()
            {
                return Mathf.Abs(position[Mathf.Abs((int)targetDirection) - 1] - targetInt);
            }

            int distanceToTarget = GetDistanceToTarget();

            while (distanceToTarget > 0)
            {
                int additional = targetDirection == currentDirection ? 0 : 1;

                bool isTargetDir = targetDirection == currentDirection;

                switch (currentDirection)
                {
                    case Direction.Up:
                        position[1] = Mathf.Min(UnityEngine.Random.Range(position[1] + (isTargetDir ? targetDirectionMinMax.x : traverseDirectionMinMax.x), position[1] + (isTargetDir ? targetDirectionMinMax.y : traverseDirectionMinMax.y)), mapSize.y - additional);
                        break;
                    case Direction.Left:
                        position[0] = Mathf.Max(UnityEngine.Random.Range(position[0] - (isTargetDir ? targetDirectionMinMax.y : traverseDirectionMinMax.y), position[0] - (isTargetDir ? targetDirectionMinMax.x : traverseDirectionMinMax.x)), additional);
                        break;
                    case Direction.Right:
                        position[0] = Mathf.Min(UnityEngine.Random.Range(position[0] + (isTargetDir ? targetDirectionMinMax.x : traverseDirectionMinMax.x), position[0] + (isTargetDir ? targetDirectionMinMax.y : traverseDirectionMinMax.y)), mapSize.x - additional);
                        break;
                    case Direction.Down:
                        position[1] = Mathf.Max(UnityEngine.Random.Range(position[1] - (isTargetDir ? targetDirectionMinMax.y : traverseDirectionMinMax.y), position[1] - (isTargetDir ? targetDirectionMinMax.x : traverseDirectionMinMax.x)), additional);
                        break;
                }

                distanceToTarget = GetDistanceToTarget();

                Vector2Int point = new Vector2Int(position[0], position[1]);

                currentDirection = GetNextDirection(currentDirection, targetDirection, point);
                path.Add(point);
            }
        }

        private Vector2Int GetClosestNodeToTargetDirection(Direction targetDirection)
        {
            List<Vector2Int> closestPoints = new List<Vector2Int>();

            int closestDistance = int.MaxValue;

            foreach (KeyValuePair<Direction, List<Vector2Int>> directionAndPath in pathway.Nodes)
            {
                List<Vector2Int> path = directionAndPath.Value;

                if (path == null || path.Count == 0)
                    continue;

                for (int i = 0; i < path.Count; i++)
                {
                    if (path[i].x == 0 || path[i].x == mapSize.x || path[i].y == 0 || path[i].y == mapSize.y)
                        continue;

                    switch (targetDirection)
                    {
                        case Direction.Up:
                            if (mapSize.y - path[i].y < closestDistance)
                            {
                                closestPoints.Clear();
                                closestPoints.Add(path[i]);
                                closestDistance = mapSize.y - path[i].y;
                            }
                            else if (mapSize.y - path[i].y == closestDistance)
                            {
                                closestPoints.Add(path[i]);
                            }
                            break;
                        case Direction.Down:
                            if (path[i].y < closestDistance)
                            {
                                closestPoints.Clear();
                                closestPoints.Add(path[i]);
                                closestDistance = path[i].y;
                            }
                            else if (path[i].y == closestDistance)
                            {
                                closestPoints.Add(path[i]);
                            }
                            break;
                        case Direction.Left:
                            if (path[i].x < closestDistance)
                            {
                                closestPoints.Clear();
                                closestPoints.Add(path[i]);
                                closestDistance = path[i].x;
                            }
                            else if (path[i].x == closestDistance)
                            {
                                closestPoints.Add(path[i]);
                            }
                            break;
                        case Direction.Right:
                            if (mapSize.x - path[i].x < closestDistance)
                            {
                                closestPoints.Clear();
                                closestPoints.Add(path[i]);
                                closestDistance = mapSize.x - path[i].x;
                            }
                            else if (mapSize.x - path[i].x == closestDistance)
                            {
                                closestPoints.Add(path[i]);
                            }
                            break;
                    }
                }
            }

            return closestPoints[UnityEngine.Random.Range(0, closestPoints.Count)];
        }

        private void PlaceTiles()
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                DestroyImmediate(tiles[i]);
            }

            tiles.Clear();

            for (int i = 0; i < mapSize.x + 1; i++)
            {
                for (int j = 0; j < mapSize.y + 1; j++)
                {
                    if (pathway.ContainsStep(new Vector2Int(i, j)))
                    {
                        tiles.Add(Instantiate(pathTile, new Vector3(i, j), Quaternion.identity, transform));
                    }
                    else
                    {
                        tiles.Add(Instantiate(wallTile, new Vector3(i, j), Quaternion.identity, transform));
                    }
                }
            }
        }

        public void CreatePath(Vector2Int start, Direction startDirection, List<Direction> targetDirections)
        {
            pathway = new Pathway(targetDirections);

            for (int i = 0; i < targetDirections.Count; i++)
            {
                Vector2Int startPoint = i == 0 ? start : GetClosestNodeToTargetDirection(targetDirections[i]);
                pathway[targetDirections[i]].Add(startPoint);
                CreatePathFromStartToTargetDirection(pathway[targetDirections[i]], startPoint, i == 0 ? startDirection : targetDirections[i], targetDirections[i]);
            }

            pathway.CreatePaths(targetDirections);

            PlaceTiles();
        }


        /*private void OnDrawGizmos()
        {
            Gizmos.color = gizmosColor;
            Gizmos.DrawWireCube(Vector3.zero, (Vector2)mapSize * mapScale);

            if (pathway == null)
                return;

            for (int i = 0; i < pathway.Count; i++)
            {
                for (int j = 0; j < pathway[i].Count; j++)
                {
                    Vector2 pos = ((Vector2)pathway[i][j] - mapSize / 2) * mapScale;
                    Gizmos.DrawCube(pos, Vector3.one * mapScale * 0.5f);

                    if (j != pathway[i].Count - 1)
                        Gizmos.DrawLine(pos, ((Vector2)pathway[i][j + 1] - mapSize / 2) * mapScale);
                }
            }

            if (pathFromPathway == null)
                return;

            for (int i = 0; i < pathFromPathway.Count; i++)
            {
                Vector2 pos = ((Vector2)pathFromPathway[i] - mapSize / 2) * mapScale;
                Gizmos.DrawCube(pos, Vector3.one * mapScale * 0.5f);
            }
        }*/
    }



    public enum Direction
    {
        Up = 2,
        Down = -2,
        Left = -1,
        Right = 1,
    }

    [CreateAssetMenu()]
    public class BiomeSettings : ScriptableObject
    {
        
    }

    [CreateAssetMenu]
    public class MonsterStats : ScriptableObject
    {
        public int Map;

        public int baseMapPoints;

        public int mapPointsPerTileInRange;
    }

    public class WeightMap
    {
        public MapWeightPoint[] points;

        public Dictionary<Vector2Int, int> coordsAndIndex;

        public WeightMap(int xSize, int ySize)
        {
            points = new MapWeightPoint[xSize * ySize];

            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    coordsAndIndex.Add(new Vector2Int(i, j), 0);
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


