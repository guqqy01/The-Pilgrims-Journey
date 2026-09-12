using GameManagerCalls;
using System.Collections.Generic;
using UnityEngine;

namespace LocalMap
{
    internal class LocalMapPathGenerator
    {
        private readonly LocalMapGenerator _owner;

        public LocalMapPathGenerator(LocalMapGenerator owner)
        {
            _owner = owner;
        }

        private int RandomTargetDirection
        {
            get
            {
                float time = GameManager.Instance != null ? GameManager.Instance.Level : 1;
                return Mathf.Max(Mathf.RoundToInt(_owner.targetDirectionVSLevelCurve.Evaluate(time, UnityEngine.Random.value)), 1);
            }
        }

        private int RandomTraverseDirection
        {
            get
            {
                float time = GameManager.Instance != null ? GameManager.Instance.Level : 1;
                return Mathf.Max(Mathf.RoundToInt(_owner.traverseDirectionVSLevelCurve.Evaluate(time, UnityEngine.Random.value)), 1);
            }
        }

        public void CreatePath(Direction startWall, List<Direction> toDirections, List<Direction> otherFromDirections)
        {
            int mapSizeX = _owner.MapSizeX;
            int mapSizeY = _owner.MapSizeY;

            foreach (Direction dir in toDirections)
            {
                if (otherFromDirections.Contains(dir))
                    otherFromDirections.Remove(dir);
            }

            Vector2Int pathStart = new Vector2Int();

            switch (startWall)
            {
                case Direction.South:
                    pathStart = new Vector2Int(mapSizeX / 2, -1);
                    break;
                case Direction.West:
                    pathStart = new Vector2Int(-1, mapSizeY / 2);
                    break;
                case Direction.East:
                    pathStart = new Vector2Int(mapSizeX, mapSizeY / 2);
                    break;
                case Direction.North:
                    pathStart = new Vector2Int(mapSizeX / 2, mapSizeY);
                    break;
            }

            _owner.Pathway = new Pathway(toDirections, otherFromDirections);

            for (int i = 0; i < toDirections.Count; i++)
            {
                Vector2Int startPoint = i == 0 ? pathStart : GetClosestNodeToTargetDirection(toDirections[i]);
                _owner.Pathway[toDirections[i]].Add(startPoint);
                CreatePathFromStartToTargetDirection(_owner.Pathway[toDirections[i]], startPoint, i == 0 ? startWall.OppositeDirection() : toDirections[i], toDirections[i]);
            }

            for (int i = 0; i < otherFromDirections.Count; i++)
            {
                Vector2Int startPoint = GetClosestNodeToTargetDirection(otherFromDirections[i]);
                _owner.Pathway[otherFromDirections[i]].Add(startPoint);
                CreatePathFromStartToTargetDirection(_owner.Pathway[otherFromDirections[i]], startPoint, otherFromDirections[i], otherFromDirections[i]);
            }
        }

        private Direction GetNextDirection(Direction currentDirection, Direction targetDirection, Vector2Int position)
        {
            int mapSizeX = _owner.MapSizeX;
            int mapSizeY = _owner.MapSizeY;

            if (targetDirection != currentDirection)
                return targetDirection;

            if (Mathf.Abs((int)currentDirection) == 2)
            {
                float dirRan = UnityEngine.Random.Range(1f / mapSizeX, 1f - 1f / mapSizeX);
                return (dirRan > (float)position.x / mapSizeX) ? Direction.East : Direction.West;
            }
            else
            {
                float dirRan = UnityEngine.Random.Range(1f / mapSizeY, 1f - 1f / mapSizeY);
                return (dirRan > (float)position.y / mapSizeY) ? Direction.North : Direction.South;
            }
        }

        private void CreatePathFromStartToTargetDirection(List<Vector2Int> path, Vector2Int start, Direction startDirection, Direction targetDirection)
        {
            int mapSizeX = _owner.MapSizeX;
            int mapSizeY = _owner.MapSizeY;

            Direction currentDirection = startDirection;
            int[] position = new int[2];

            position[0] = start.x;
            position[1] = start.y;

            int targetInt = (int)targetDirection > 0 ? ((targetDirection == Direction.East) ? mapSizeX : mapSizeY) : -1;

            int GetDistanceToTarget()
            {
                return Mathf.Abs(position[Mathf.Abs((int)targetDirection) - 1] - targetInt);
            }

            int distanceToTarget = GetDistanceToTarget();

            int safetyBreaker = 1000;

            while (distanceToTarget > 0 && safetyBreaker > 0)
            {
                bool isTargetDir = targetDirection == currentDirection;

                int additional = isTargetDir ? 0 : 1;

                switch (currentDirection)
                {
                    case Direction.North:
                        position[1] = Mathf.Min(position[1] + (isTargetDir ? RandomTargetDirection : RandomTraverseDirection), mapSizeY - 1 - additional);
                        if (isTargetDir && position[1] == mapSizeY - 1)
                            position[1] = mapSizeY;
                        break;
                    case Direction.South:
                        position[1] = Mathf.Max(position[1] - (isTargetDir ? RandomTargetDirection : RandomTraverseDirection), additional);
                        if (isTargetDir && position[1] <= 1)
                            position[1] = -1;
                        break;
                    case Direction.East:
                        position[0] = Mathf.Min(position[0] + (isTargetDir ? RandomTargetDirection : RandomTraverseDirection), mapSizeX - 1 - additional);
                        if (isTargetDir && position[0] == mapSizeX - 1)
                            position[0] = mapSizeX;
                        break;
                    case Direction.West:
                        position[0] = Mathf.Max(position[0] - (isTargetDir ? RandomTargetDirection : RandomTraverseDirection), additional);
                        if (isTargetDir && position[0] <= 1)
                            position[0] = -1;
                        break;
                }

                distanceToTarget = GetDistanceToTarget();

                Vector2Int point = new Vector2Int(position[0], position[1]);

                currentDirection = GetNextDirection(currentDirection, targetDirection, point);
                path.Add(point);

                safetyBreaker--;
            }
        }

        private Vector2Int GetClosestNodeToTargetDirection(Direction targetDirection)
        {
            int mapSizeX = _owner.MapSizeX;
            int mapSizeY = _owner.MapSizeY;

            List<Vector2Int> closestPoints = new List<Vector2Int>();
            int closestDistance = int.MaxValue;

            foreach (KeyValuePair<Direction, List<Vector2Int>> directionAndPath in _owner.Pathway.SectionNodesPaths)
            {
                List<Vector2Int> path = directionAndPath.Value;

                if (path == null || path.Count == 0)
                    continue;

                for (int i = 0; i < path.Count; i++)
                {
                    if (path[i].x <= 0 || path[i].x >= mapSizeX || path[i].y <= 0 || path[i].y >= mapSizeY)
                        continue;

                    switch (targetDirection)
                    {
                        case Direction.North:
                            if (mapSizeY - path[i].y < closestDistance)
                            {
                                closestPoints.Clear();
                                closestPoints.Add(path[i]);
                                closestDistance = mapSizeY - path[i].y;
                            }
                            else if (mapSizeY - path[i].y == closestDistance)
                            {
                                closestPoints.Add(path[i]);
                            }
                            break;
                        case Direction.South:
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
                        case Direction.West:
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
                        case Direction.East:
                            if (mapSizeX - path[i].x < closestDistance)
                            {
                                closestPoints.Clear();
                                closestPoints.Add(path[i]);
                                closestDistance = mapSizeX - path[i].x;
                            }
                            else if (mapSizeX - path[i].x == closestDistance)
                            {
                                closestPoints.Add(path[i]);
                            }
                            break;
                    }
                }
            }

            return closestPoints.GetRandom();
        }

        public bool[,] GetPathGrid(LocalMapGenerationSettings generationSettings)
        {
            int mapSizeX = _owner.MapSizeX;
            int mapSizeY = _owner.MapSizeY;
            Pathway pathway = _owner.Pathway;

            bool[,] returnGrid = new bool[mapSizeX, mapSizeY];
            bool[,] mainPathGrid = new bool[mapSizeX, mapSizeY];
            float[,] noiseGrid = new float[mapSizeX, mapSizeY];

            for (int xPos = 0; xPos < mapSizeX; xPos++)
            {
                for (int yPos = 0; yPos < mapSizeY; yPos++)
                {
                    bool containsStep = pathway.ContainsStep(new Vector2Int(xPos, yPos));
                    mainPathGrid[xPos, yPos] = containsStep;
                    noiseGrid[xPos, yPos] = containsStep ? 1f : 0f;
                }
            }

            bool[,] originalPathGrid = (bool[,])mainPathGrid.Clone();

            for (int xPos = 0; xPos < mapSizeX; xPos++)
            {
                for (int yPos = 0; yPos < mapSizeY; yPos++)
                {
                    if (originalPathGrid[xPos, yPos])
                    {
                        Blur(xPos, yPos, generationSettings.pathWidthRange.Min(), generationSettings.pathWidthRange.Max());
                    }
                }
            }

            for (int xPos = 0; xPos < mapSizeX; xPos++)
                for (int yPos = 0; yPos < mapSizeY; yPos++)
                    Noise(xPos, yPos);

            for (int xPos = 0; xPos < mapSizeX; xPos++)
                for (int yPos = 0; yPos < mapSizeY; yPos++)
                    returnGrid[xPos, yPos] = noiseGrid[xPos, yPos] >= generationSettings.cullingAmount;

            return returnGrid;

            void Blur(int startXPos, int startYPos, float minRange, float maxRange)
            {
                int extent = Mathf.FloorToInt(maxRange);

                for (int xPos = startXPos - extent; xPos <= startXPos + extent; xPos++)
                {
                    for (int yPos = startYPos - extent; yPos <= startYPos + extent; yPos++)
                    {
                        if (xPos < 0 || yPos < 0 || xPos >= mapSizeX || yPos >= mapSizeY)
                            continue;

                        float range = new Vector2(xPos - startXPos, yPos - startYPos).magnitude;

                        if (range > maxRange) continue;

                        if (range <= minRange)
                        {
                            // Core path - always solid, noise cannot erode this
                            noiseGrid[xPos, yPos] = 1f;
                            mainPathGrid[xPos, yPos] = true;
                        }
                        else
                        {
                            // Edge band - set to falloff value, Perlin noise will erode this later
                            float t = Mathf.InverseLerp(minRange, maxRange, range);
                            float falloff = Mathf.Lerp(1f, 0f, t);
                            noiseGrid[xPos, yPos] = Mathf.Max(noiseGrid[xPos, yPos], falloff);
                        }
                    }
                }
            }

            void Noise(int xPos, int yPos)
            {
                // Don't erode core path tiles
                if (mainPathGrid[xPos, yPos]) return;

                float noiseX = xPos * generationSettings.pathNoiseScale.x + generationSettings.pathNoiseTiling.x;
                float noiseY = yPos * generationSettings.pathNoiseScale.y + generationSettings.pathNoiseTiling.y;

                noiseGrid[xPos, yPos] *= Mathf.PerlinNoise(noiseX, noiseY);
            }
        }
    }
}
