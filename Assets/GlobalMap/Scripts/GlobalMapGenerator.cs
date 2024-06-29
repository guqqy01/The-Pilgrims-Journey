using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using Random = UnityEngine.Random;

namespace Map
{
    public class GlobalMapGenerator : MonoBehaviour, ISaveLoad
    {
        public static GlobalMapGenerator Instance;

        public GlobalMapUI GlobalMapUI;

        public int Seed { get; private set; }

        public string SaveFileName => "GlobalMapSave";

        public GlobalMapGenerationSettings GenerationSettings;

        [SerializeField]
        private GlobalMapPoint mapPointPrefab;

        public GlobalMapPoint[,] mapPoints;
        public List<GlobalMapPoint> takenPath = new List<GlobalMapPoint>();
        
        public Vector2Int CurrentPosition {  get; private set; }
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

                if(!takenPath.Contains(value))
                    takenPath.Add(value);
            }
        }

        public delegate void UpdateUI();
        public UpdateUI updateUI;

        public delegate void WaitForMapGenerated();
        public static WaitForMapGenerated OnMapGenerated;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            GameManager.onSave += Save;
            GameManager.onLoad += Load;
            GameManager.onCompleteLevel += CompleteLevel;
        }

        private void OnDisable()
        {
            GameManager.onSave -= Save;
            GameManager.onLoad -= Load;
            GameManager.onCompleteLevel -= CompleteLevel;
        }

        #region Map Generation

        public void GenerateMap(int seed)
        {
            Seed = seed;

            Random.InitState(Seed);

            mapPoints = new GlobalMapPoint[GenerationSettings.MapSize.x, GenerationSettings.MapSize.y];

            //Start Point
            Vector2Int currentPosition = new Vector2Int(0, Mathf.FloorToInt(GenerationSettings.MapSize.y / 2f));
            Direction currentDirection = Direction.East;

            GlobalMapPoint previousMapPoint = new GlobalMapPoint(currentPosition);
            previousMapPoint.Seed = Random.Range(int.MinValue, int.MaxValue);
            mapPoints[currentPosition.x, currentPosition.y] = previousMapPoint;
            CurrentMapPoint = previousMapPoint;
            
            List<GlobalMapPoint> pathPoints = new List<GlobalMapPoint>();

            //Start Road
            for (int i = 0; i < GenerationSettings.StartRoadLength; i++)
            {
                currentPosition += Vector2Int.right;

                SetupPoint();
            }

            GlobalMapPoint startMapPoint = previousMapPoint;
            Vector2Int innerMapStartPosition = currentPosition;
            List<GlobalMapPoint> usedPoints = new List<GlobalMapPoint>();

            foreach (GlobalPathSettings pathSettings in GenerationSettings.PathSettings)
            {
                pathPoints = new List<GlobalMapPoint>();

                for (int i = 0; i < pathSettings.NumberOfPaths; i++)
                {
                    previousMapPoint = startMapPoint;
                    currentPosition = innerMapStartPosition;

                    int breaker = 100;

                    //Inner Map
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
                        SetupPoint();

                        breaker--;
                    }
                }

                if (pathPoints.Count <= 0)
                    continue;

                for (int i = 0; i < pathSettings.NumberOfRandomEncounters; i++)
                {
                    GlobalMapPoint point = pathPoints.GetRandom();

                    int breaker = 100;

                    while (usedPoints.Contains(point) && breaker > 0)
                    {
                        point = pathPoints.GetRandom();
                        breaker--;
                    }

                    point.Type = GlobalMapPointType.Random;
                    usedPoints.Add(point);
                }

                for (int i = 0; i < pathSettings.NumberOfRelics; i++)
                {
                    GlobalMapPoint point = pathPoints.GetRandom();

                    int breaker = 100;

                    while (usedPoints.Contains(point) && breaker > 0)
                    {
                        point = pathPoints.GetRandom();
                        breaker--;
                    }

                    point.Type = GlobalMapPointType.Relic;
                    usedPoints.Add(point);
                }

                for (int i = 0; i < pathSettings.NumberOfShops; i++)
                {
                    GlobalMapPoint point = pathPoints.GetRandom();

                    int breaker = 100;

                    while (usedPoints.Contains(point) && breaker > 0)
                    {
                        point = pathPoints.GetRandom();
                        breaker--;
                    }

                    point.Type = GlobalMapPointType.Shop;
                    usedPoints.Add(point);
                }
            }

            currentDirection = Direction.East;

            //End Road
            for (int i = 0; i < GenerationSettings.EndRoadLength; i++)
            {
                currentPosition += Vector2Int.right;
                SetupPoint();
            }

            //Helper Methods


            void SetupPoint()
            {
                GlobalMapPoint mapPoint;

                if (mapPoints[currentPosition.x, currentPosition.y] == null)
                {
                    mapPoint = new GlobalMapPoint(currentPosition);

                    mapPoints[currentPosition.x, currentPosition.y] = mapPoint;

                    mapPoint.Seed = Random.Range(int.MinValue, int.MaxValue);
                }
                else
                {
                    mapPoint = mapPoints[currentPosition.x, currentPosition.y];
                }

                if(!pathPoints.Contains(mapPoint))
                    pathPoints.Add(mapPoint);

                ConnectMapPoints(previousMapPoint, mapPoint);

                previousMapPoint = mapPoint;
            }

            updateUI?.Invoke();

            OnMapGenerated?.Invoke();
        }

        private void ConnectMapPoints(GlobalMapPoint from, GlobalMapPoint to)
        {
            Vector2Int direction = to.Position - from.Position;

            if(direction == Vector2Int.right)
            {
                if (!from.connectedToPoints.ContainsValue(to))
                    from.connectedToPoints.Add(Direction.East, to);

                if (!to.connectedFromPoints.ContainsValue(from))
                    to.connectedFromPoints.Add(Direction.West, from);
            }
            else if(direction == Vector2Int.left)
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

        private void CompleteLevel()
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

            foreach(GlobalMapPoint mapPoint in mapPoints)
            {
                if (mapPoint == null)
                    continue;

                Gizmos.DrawSphere((Vector2)mapPoint.Position, sphereRadius);

                foreach(GlobalMapPoint toPoint in mapPoint.connectedToPoints.Values)
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
            SaveData saveData = SaveManager.Load<SaveData>(SaveFileName).saveData;

            GenerateMap(saveData.Seed);

            CurrentPosition = saveData.CurrentPosition;

            takenPath = new List<GlobalMapPoint>();

            foreach(Vector2Int position in saveData.takenPath)
            {
                takenPath.Add(mapPoints[position.x, position.y]);
            }

            updateUI?.Invoke();
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

                foreach(GlobalMapPoint point in takenPathPoints)
                {
                    takenPath.Add(point.Position);
                }
            }
        }

        #endregion
    }
}
