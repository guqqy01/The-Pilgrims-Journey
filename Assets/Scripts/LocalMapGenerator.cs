using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

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

        public List<Vector2Int>[] paths;

        public Vector2Int start = new Vector2Int(10, 0);

        public List<Direction> targetDirections;
        public Direction startDirection;
        private void Start()
        {
            CreatePath(start, startDirection, targetDirections);
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

        

        public void CreatePathFromStartToTargetDirection(ref List<Vector2Int> path, Vector2Int start, Direction startDirection, Direction targetDirection)
        {
            Direction currentDirection = startDirection;
            int[] position = new int[2];

            position[0] = start.x;
            position[1] = start.y;

            int targetInt = (int)targetDirection > 0 ? (targetDirection == Direction.Right) ? mapSize.x : mapSize.y: 0;
            Debug.Log("TargetInt = " + targetInt);
            int GetDistanceToTarget()
            {
                return Mathf.Abs(position[Mathf.Abs((int)targetDirection) - 1] - targetInt);
            }

            int distanceToTarget = GetDistanceToTarget();
            Debug.Log("distance = " + distanceToTarget);
            while (distanceToTarget > 0)
            {
                int additional = targetDirection == currentDirection ? 0 : 1;

                switch (currentDirection)
                {
                    case Direction.Up:
                        position[1] = Mathf.Min(UnityEngine.Random.Range(position[1] + 2, position[1] + 8), mapSize.y - additional);
                        break;
                    case Direction.Left:
                        position[0] = Mathf.Max(UnityEngine.Random.Range(position[0] - 8, position[0] - 2), additional);
                        break;
                    case Direction.Right:
                        position[0] = Mathf.Min(UnityEngine.Random.Range(position[0] + 2, position[0] + 8), mapSize.x - additional);
                        break;
                    case Direction.Down:
                        position[1] = Mathf.Max(UnityEngine.Random.Range(position[1] - 8, position[1] - 2), additional);
                        break;
                }



                distanceToTarget = GetDistanceToTarget();

                Debug.Log("distance = " + distanceToTarget);

                Vector2Int point = new Vector2Int(position[0], position[1]);

                currentDirection = GetNextDirection(currentDirection, targetDirection, point);
                path.Add(point);
            }
        }

        private Vector2Int GetClosestNodeToTargetDirection(Direction targetDirection)
        {
            List<Vector2Int> closestPoints = new List<Vector2Int>();

            int closestDistance = int.MaxValue;

            foreach (List<Vector2Int> path in paths)
            {
                if (path == null || path.Count == 0)
                    continue;

                for (int i = 0; i < path.Count; i++)
                {
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

        public void CreatePath(Vector2Int start, Direction startDirection, List<Direction> targetDirections)
        {
            paths = new List<Vector2Int> [targetDirections.Count];

            for (int i = 0; i < paths.Length; i++)
            {
                paths[i] = new List<Vector2Int>();
            }

            for (int i = 0; i < targetDirections.Count; i++)
            {
                Vector2Int startPoint = i == 0 ? start : GetClosestNodeToTargetDirection(targetDirections[i]);
                paths[i].Add(startPoint);
                CreatePathFromStartToTargetDirection(ref paths[i], startPoint, i == 0 ? startDirection : targetDirections[i], targetDirections[i]);
            }


            foreach (List<Vector2Int> path in paths)
            {
                if (path == null)
                    continue;

                for (int i = 0; i < path.Count; i++)
                {
                    Debug.Log(path[i]);
                }
            }
        }

        public void GenerateMap()
        {

        }

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmosColor;
            Gizmos.DrawWireCube(Vector3.zero, (Vector2)mapSize * mapScale);

            if (paths == null)
                return;

            foreach (List<Vector2Int> path in paths)
            {
                if (path == null)
                    continue;

                for (int i = 0; i < path.Count; i++)
                {
                    Vector2 pos = ((Vector2)path[i] - mapSize / 2) * mapScale;
                    Gizmos.DrawCube(pos, Vector3.one * mapScale * 0.5f);

                    if (i != path.Count - 1)
                        Gizmos.DrawLine(pos, ((Vector2)path[i + 1] - mapSize / 2) * mapScale);
                }
            }
        }
    }


    public enum Direction
    {
        Up = 2,
        Down = -2,
        Left = -1,
        Right = 1,
    }
}


