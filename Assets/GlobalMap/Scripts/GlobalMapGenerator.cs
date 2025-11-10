using Equipment;
using GameManagerCalls;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using Random = UnityEngine.Random;

namespace GlobalMap
{
    public class GlobalMapGenerator : MonoBehaviour, ISaveLoad, IOnCompletedLevel, IOnSave, IOnLoad
    {
        public static GlobalMapGenerator Instance { private set; get; }

        public int Seed { get; private set; }

        public string SaveFileName => "GlobalMapSave";

        public GlobalMapGenerationSettings GenerationSettings;

        [SerializeField]
        private GlobalMapPoint mapPointPrefab;

        public GlobalMapPoint[,] mapPoints;
        public List<GlobalMapPoint> takenPath = new List<GlobalMapPoint>();

        public Vector2Int CurrentPosition { get; private set; }
        public GlobalMapPoint CurrentMapPoint
        {
            get
            {
                mapPoints ??= new GlobalMapPoint[GenerationSettings.MapSize.x, GenerationSettings.MapSize.y];

                GlobalMapPoint currentMapPoint = mapPoints[CurrentPosition.x, CurrentPosition.y];

                if (!takenPath.Contains(currentMapPoint))
                    takenPath.Add(currentMapPoint);

                return currentMapPoint;
            }
            set
            {
                CurrentPosition = value.Position;

                if (!takenPath.Contains(value))
                    takenPath.Add(value);
            }
        }

        public delegate void UpdateUI();
        public UpdateUI updateUI;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        #region Map Generation

        public void GenerateMap(int seed)
        {
            Seed = seed;
            Random.InitState(Seed);

            mapPoints = new GlobalMapPoint[GenerationSettings.MapSize.x, GenerationSettings.MapSize.y];

            // Start Point
            Vector2Int currentPosition = new Vector2Int(0, Mathf.FloorToInt(GenerationSettings.MapSize.y / 2f));
            Direction currentDirection = Direction.East;

            GlobalMapPoint previousMapPoint = CreateOrGetMapPoint(currentPosition, null);
            CurrentMapPoint = previousMapPoint;

            // Start road
            GenerateStartRoad(ref currentPosition, ref previousMapPoint);

            GlobalMapPoint startMapPoint = previousMapPoint;
            Vector2Int innerMapStartPosition = currentPosition;

            // Generate inner paths and assign special points
            List<GlobalMapPoint> usedPoints = new List<GlobalMapPoint>();
            GeneratePaths(startMapPoint, innerMapStartPosition, ref currentDirection, usedPoints, out Vector2Int endPosition, out GlobalMapPoint endPoint);

            // End road
            currentDirection = Direction.East;
            GenerateEndRoad(ref endPosition, ref endPoint);

            updateUI?.Invoke();
        }

        // Create or return existing map point at position. If pathPoints provided, the point is added there.
        private GlobalMapPoint CreateOrGetMapPoint(Vector2Int position, List<GlobalMapPoint>? pathPoints)
        {
            GlobalMapPoint mapPoint;
            if (mapPoints[position.x, position.y] == null)
            {
                mapPoint = new GlobalMapPoint(position);
                mapPoints[position.x, position.y] = mapPoint;
                mapPoint.Seed = Random.Range(int.MinValue, int.MaxValue);
            }
            else
            {
                mapPoint = mapPoints[position.x, position.y];
            }

            if (pathPoints != null && !pathPoints.Contains(mapPoint))
                pathPoints.Add(mapPoint);

            return mapPoint;
        }

        private void GenerateStartRoad(ref Vector2Int currentPosition, ref GlobalMapPoint previousMapPoint)
        {
            for (int i = 0; i < GenerationSettings.StartRoadLength; i++)
            {
                currentPosition += Vector2Int.right;
                var mapPoint = CreateOrGetMapPoint(currentPosition, null);
                ConnectMapPoints(previousMapPoint, mapPoint);
                previousMapPoint = mapPoint;
            }
        }

        private void GeneratePaths(GlobalMapPoint startMapPoint, Vector2Int innerMapStartPosition, ref Direction currentDirection, List<GlobalMapPoint> usedPoints, out Vector2Int currentPosition, out GlobalMapPoint endPoint)
        {
            currentPosition = innerMapStartPosition;
            endPoint = startMapPoint;

            foreach (GlobalPathSettings pathSettings in GenerationSettings.PathSettings)
            {
                List<GlobalMapPoint> pathPoints = new List<GlobalMapPoint>();

                for (int i = 0; i < pathSettings.NumberOfPaths; i++)
                {
                    GlobalMapPoint previousMapPoint = startMapPoint;
                    currentPosition = innerMapStartPosition;

                    int breaker = GenerationSettings.MapSize.x * 3;

                    // Inner Map traversal
                    while (currentPosition != GenerationSettings.EndOfInnerMapPosition && breaker > 0)
                    {
                        currentDirection = GenerationSettings.GetNextDirection(pathSettings, currentDirection, currentPosition);

                        switch (currentDirection)
                        {
                            case Direction.North:
                                currentPosition += Vector2Int.up;
                                break;
                            case Direction.South:
                                currentPosition += Vector2Int.down;
                                break;
                            case Direction.East:
                                currentPosition += Vector2Int.right;
                                break;
                            case Direction.West:
                                currentPosition += Vector2Int.left;
                                break;
                        }

                        var mapPoint = CreateOrGetMapPoint(currentPosition, pathPoints);
                        ConnectMapPoints(previousMapPoint, mapPoint);
                        previousMapPoint = mapPoint;

                        breaker--;
                    }

                    endPoint = previousMapPoint;
                }

                

                if (pathPoints.Count <= 0)
                    continue;

                AssignSpecialMapPoints(pathSettings, pathPoints, usedPoints);
            }
        }

        private void AssignSpecialMapPoints(GlobalPathSettings pathSettings, List<GlobalMapPoint> pathPoints, List<GlobalMapPoint> usedPoints)
        {
            // Relics
            for (int i = 0; i < pathSettings.NumberOfRelics; i++)
            {
                GlobalMapPoint point = GetRandomUnused(pathPoints, usedPoints);
                point.Type = GlobalMapPointType.Relic;
                usedPoints.Add(point);
            }

            // Random encounters
            for (int i = 0; i < pathSettings.NumberOfRandomEncounters; i++)
            {
                GlobalMapPoint point = GetRandomUnused(pathPoints, usedPoints);
                point.Type = GlobalMapPointType.Random;
                usedPoints.Add(point);
            }

            // Shops
            for (int i = 0; i < pathSettings.NumberOfShops; i++)
            {
                GlobalMapPoint point = GetRandomUnused(pathPoints, usedPoints);
                point.Type = GlobalMapPointType.Shop;
                usedPoints.Add(point);
            }
        }

        private GlobalMapPoint GetRandomUnused(List<GlobalMapPoint> pathPoints, List<GlobalMapPoint> usedPoints)
        {
            int breaker = 100;
            GlobalMapPoint point = pathPoints.GetRandom();
            while (usedPoints.Contains(point) && breaker-- > 0)
                point = pathPoints.GetRandom();
            return point;
        }

        private void GenerateEndRoad(ref Vector2Int currentPosition, ref GlobalMapPoint previousMapPoint)
        {
            for (int i = 0; i < GenerationSettings.EndRoadLength; i++)
            {
                currentPosition += Vector2Int.right;
                var mapPoint = CreateOrGetMapPoint(currentPosition, null);
                ConnectMapPoints(previousMapPoint, mapPoint);
                previousMapPoint = mapPoint;
            }
        }

        private void ConnectMapPoints(GlobalMapPoint from, GlobalMapPoint to)
        {
            Vector2Int direction = to.Position - from.Position;

            if (direction == Vector2Int.right)
            {
                if (!from.connectedToPoints.ContainsValue(to))
                    from.connectedToPoints.Add(Direction.East, to);

                if (!to.connectedFromPoints.ContainsValue(from))
                    to.connectedFromPoints.Add(Direction.West, from);
            }
            else if (direction == Vector2Int.left)
            {
                if (!from.connectedToPoints.ContainsValue(to))
                    from.connectedToPoints.Add(Direction.West, to);

                if (!to.connectedFromPoints.ContainsValue(from))
                    to.connectedFromPoints.Add(Direction.East, from);
            }
            else if (direction == Vector2Int.up)
            {
                if (!from.connectedToPoints.ContainsValue(to))
                    from.connectedToPoints.Add(Direction.North, to);

                if (!to.connectedFromPoints.ContainsValue(from))
                    to.connectedFromPoints.Add(Direction.South, from);
            }
            else if (direction == Vector2Int.down)
            {
                if (!from.connectedToPoints.ContainsValue(to))
                    from.connectedToPoints.Add(Direction.South, to);

                if (!to.connectedFromPoints.ContainsValue(from))
                    to.connectedFromPoints.Add(Direction.North, from);
            }
        }

        #endregion

        #region Map Traversal

        private Direction nextDirection;

        public bool TryPickDirection(Direction direction)
        {
            if (!CurrentMapPoint.connectedToPoints.ContainsKey(direction))
                return false;

            nextDirection = direction;
            return true;
        }

        public void OnLevelCompleted()
        {
            CurrentMapPoint = CurrentMapPoint.connectedToPoints[nextDirection];

            updateUI?.Invoke();
        }

        #endregion

        #region Gizmos

        [Header("Gizmos")]
        [SerializeField]
        private bool showGizmos;
        [SerializeField]
        private float sphereRadius;
        [SerializeField]
        private float arrowLength;


        private void OnDrawGizmosSelected()
        {
            if (!showGizmos)
                return;

            if (mapPoints == null)
                return;

            foreach (GlobalMapPoint mapPoint in mapPoints)
            {
                if (mapPoint == null)
                    continue;

                Gizmos.DrawSphere((Vector2)mapPoint.Position, sphereRadius);

                foreach (GlobalMapPoint toPoint in mapPoint.connectedToPoints.Values)
                {
                    Gizmos.DrawLine((Vector2)mapPoint.Position, (Vector2)toPoint.Position);
                    Vector2 halfway = ((Vector2)toPoint.Position + (Vector2)mapPoint.Position) / 2f;
                    Vector2 direction = toPoint.Position - mapPoint.Position;
                    Gizmos.DrawLine(halfway, halfway - (direction + direction.Perpendicular1()).normalized * arrowLength);
                    Gizmos.DrawLine(halfway, halfway - (direction + direction.Perpendicular2()).normalized * arrowLength);
                }
            }
        }

        #endregion

        #region Save and Load

        public void Save()
        {
            SaveData saveData = new SaveData(Seed, CurrentPosition, takenPath);
            SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
            SaveManager.SaveOver(saveProfile);
        }

        public void Load()
        {
            if (SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> outSaveProfile))
            {
                SaveData saveData = outSaveProfile.saveData;

                GenerateMap(saveData.Seed);

                CurrentPosition = saveData.CurrentPosition;

                takenPath = new List<GlobalMapPoint>();

                foreach (Vector2Int position in saveData.takenPath)
                {
                    takenPath.Add(mapPoints[position.x, position.y]);
                }

                updateUI?.Invoke();
            }
            else
            {
                Debug.LogWarning($"No save data found for {SaveFileName}");
                return;
            }
        }

        [Serializable]
        private record SaveData : SaveProfileData
        {
            public int Seed;
            public Vector2Int CurrentPosition;
            public List<Vector2Int> takenPath;
            public SaveData(int Seed, Vector2Int currentPosition, List<GlobalMapPoint> takenPathPoints)
            {
                this.Seed = Seed;
                this.CurrentPosition = currentPosition;

                takenPath = new List<Vector2Int>();

                foreach (GlobalMapPoint point in takenPathPoints)
                {
                    takenPath.Add(point.Position);
                }
            }
        }

        #endregion
    }
}
