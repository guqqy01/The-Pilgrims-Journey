using GameManagerCalls;
using LocalMap;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

        private Dictionary<GlobalMapPoint, int> _biomeRunLengths = new Dictionary<GlobalMapPoint, int>();

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
            _biomeRunLengths = new Dictionary<GlobalMapPoint, int>();

            mapPoints = new GlobalMapPoint[GenerationSettings.MapSize.x, GenerationSettings.MapSize.y];

            Vector2Int currentPosition = new Vector2Int(0, Mathf.FloorToInt(GenerationSettings.MapSize.y / 2f));
            Direction currentDirection = Direction.East;

            GlobalMapPoint previousMapPoint = CreateOrGetMapPoint(currentPosition, null);
            CurrentMapPoint = previousMapPoint;

            GenerateStartRoad(ref currentPosition, ref previousMapPoint);

            GlobalMapPoint startMapPoint = previousMapPoint;
            Vector2Int innerMapStartPosition = currentPosition;

            List<GlobalMapPoint> usedPoints = new List<GlobalMapPoint>();
            GeneratePaths(startMapPoint, innerMapStartPosition, ref currentDirection, usedPoints, out Vector2Int endPosition, out GlobalMapPoint endPoint);

            currentDirection = Direction.East;
            GenerateEndRoad(ref endPosition, ref endPoint);

            AssignBiomes();

            updateUI?.Invoke();
        }

        // Create or return existing map point at position. If pathPoints provided, the point is added there.
        private GlobalMapPoint CreateOrGetMapPoint(
    Vector2Int position,
    List<GlobalMapPoint> pathPoints)
        {
            GlobalMapPoint mapPoint;
            bool isNew = mapPoints[position.x, position.y] == null;

            if (isNew)
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

        private float GetXProgress(int x)
        {
            float denom = Mathf.Max(1f, GenerationSettings.EndOfInnerMapPosition.x - GenerationSettings.StartRoadLength);
            return Mathf.Clamp01((x - GenerationSettings.StartRoadLength) / denom);
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

                List<KeyValuePair<float, GlobalMapPoint>> pathPointDictionary = pathPoints.Select(x => new KeyValuePair<float, GlobalMapPoint>((float)x.Position.x / GenerationSettings.MapSize.x, x)).ToList();

                AssignSpecialMapPoints(pathSettings, pathPointDictionary, usedPoints);
            }
        }

        private void AssignBiomes()
        {
            // Seed the start
            GlobalMapPoint startPoint = mapPoints[0, Mathf.FloorToInt(GenerationSettings.MapSize.y / 2f)];
            if (startPoint != null)
            {
                startPoint.GenerationSettings = GenerationSettings.startingBiome;
                _biomeRunLengths[startPoint] = 1;
            }

            // Walk all points in column order so fromPoint is always already assigned
            for (int x = 0; x < GenerationSettings.MapSize.x; x++)
            {
                for (int y = 0; y < GenerationSettings.MapSize.y; y++)
                {
                    GlobalMapPoint mapPoint = mapPoints[x, y];

                    if (mapPoint == null || mapPoint.GenerationSettings != null)
                        continue;

                    // Find the first connected-from point that already has a biome
                    GlobalMapPoint fromPoint = null;
                    foreach (var kvp in mapPoint.connectedFromPoints)
                    {
                        if (kvp.Value.GenerationSettings != null)
                        {
                            fromPoint = kvp.Value;
                            break;
                        }
                    }

                    if (fromPoint == null)
                    {
                        mapPoint.GenerationSettings = GenerationSettings.startingBiome;
                        _biomeRunLengths[mapPoint] = 1;
                        continue;
                    }

                    // Non-eligible types always inherit, never trigger a change
                    if (mapPoint.Type != GlobalMapPointType.Normal && mapPoint.Type != GlobalMapPointType.Relic)
                    {
                        mapPoint.GenerationSettings = fromPoint.GenerationSettings;
                        _biomeRunLengths[mapPoint] = _biomeRunLengths.TryGetValue(fromPoint, out int inheritedRun)
                            ? inheritedRun
                            : 1;
                        continue;
                    }

                    // Gather siblings — other points already connected to fromPoint
                    List<LocalMapGenerationSettings> siblings = new List<LocalMapGenerationSettings>();
                    foreach (var kvp in fromPoint.connectedToPoints)
                    {
                        if (kvp.Value != mapPoint && kvp.Value.GenerationSettings != null)
                            siblings.Add(kvp.Value.GenerationSettings);
                    }

                    float xProgress = GetXProgress(x);
                    int runLength = _biomeRunLengths.TryGetValue(fromPoint, out int r) ? r : 1;
                    bool canChange = runLength >= GenerationSettings.minSameSettingsInARow;
                    bool willChange = canChange && Random.value < GenerationSettings.biomeChangeChance;

                    if (willChange)
                    {
                        mapPoint.GenerationSettings = GenerationSettings.PickBiome(
                            xProgress,
                            fromPoint.GenerationSettings,
                            siblings);
                        _biomeRunLengths[mapPoint] = 1;
                    }
                    else
                    {
                        mapPoint.GenerationSettings = fromPoint.GenerationSettings;
                        _biomeRunLengths[mapPoint] = runLength + 1;
                    }
                }
            }
        }

        private void AssignSpecialMapPoints(GlobalPathSettings pathSettings, List<KeyValuePair<float, GlobalMapPoint>> pathPointsDictionary, List<GlobalMapPoint> usedPoints)
        {
            foreach(MapPointSettings mapPointSetting in pathSettings.mapPointSettings)
            {
                int amount = mapPointSetting.amountOfPointsInRange.RandomBetweenTwoValuesInclusive();
                FloatRangeSlider range = mapPointSetting.range;
                GlobalMapPointType mapPointType = mapPointSetting.type;

                for (int i = 0; i < amount; i++)
                {
                    GlobalMapPoint point = GetRandomUnused(pathPointsDictionary, range, usedPoints);

                    if (point == null)
                        continue;

                    point.Type = mapPointType;
                    usedPoints.Add(point);
                }
            }
        }

        private GlobalMapPoint GetRandomUnused(List<KeyValuePair<float, GlobalMapPoint>> pathPointsDictionary, FloatRangeSlider range, List<GlobalMapPoint> usedPoints)
        {
            List<GlobalMapPoint> pathPoints = pathPointsDictionary.Where(x => x.Key.IsWithin(range.Start, range.End)).Select(x => x.Value).ToList();

            if (pathPoints.Count == 0)
                return null;

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
            if(CurrentMapPoint.connectedToPoints.TryGetValue(nextDirection, out GlobalMapPoint nextPoint))
            {
                CurrentMapPoint = nextPoint; 
            }
            else
            {
                Debug.LogWarning("Global map generator :Couldnt get value in direction, trying to use fallback");

                if (CurrentMapPoint.connectedToPoints.Count <= 0)
                {
                    throw new Exception("Current map point has no connected to-points to fall back on");
                }

                GlobalMapPoint fallbackPoint = CurrentMapPoint.connectedToPoints.First().Value;

                if(fallbackPoint == null)
                {
                    throw new Exception("fallback global map point is null");
                }

                CurrentMapPoint = fallbackPoint;
            }

            updateUI?.Invoke();
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
        private class SaveData : SaveProfileData
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
