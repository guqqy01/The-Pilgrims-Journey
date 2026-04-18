using GameManagerCalls;
using Monsters;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using GlobalMap;
using Unity.VisualScripting;
using LocalMap.Editor;

namespace LocalMap
{
    public class LocalMapGenerator : Singleton<LocalMapGenerator>, IOnClear, IOnCreateLocalMap
    {
        public Pathway pathway;

        public AutoTileSettings autoBaseTileSettings;

        [SerializeField] private MapTile mapTilePrefab;
        [SerializeField] private GameObject roadSignPrefab;
        [SerializeField] private Transform maptileParent;

        [SerializeField] private int mapSizeX = 32;
        [SerializeField] private int mapSizeY = 18;

        [SerializeField] private float biomeBlendNoiseScale = 0.15f;
        [SerializeField] private float biomeBlendNoiseStrength = 0.25f;

        [SerializeField, HideInInspector]
        public List<GameObject> mapObjectGOs;

        [SerializeField]
        private Dictionary<string, int> mapObjects = new Dictionary<string, int>();

        public MinMaxCurve targetDirectionVSLevelCurve;
        public MinMaxCurve traverseDirectionVSLevelCurve;

        private List<(Direction direction, LocalMapGenerationSettings settings)> _activeBlends = new List<(Direction, LocalMapGenerationSettings)>();

        public MapTile[,] Tiles { private set; get; }
        public Dictionary<GameObject, Direction> RoadSigns { get; private set; } = new Dictionary<GameObject, Direction>();

        private List<Vector2Int> occupiedSpots = new List<Vector2Int>();
        public List<Direction> TargetDirections => pathway.ToDirections;
        public Direction StartWall { get; private set; }
        public int MapSizeX => mapSizeX;
        public int MapSizeY => mapSizeY;

        /// <summary>
        /// x and y are positions, returns if part of visual path, which will be bigger than the actual path taken.
        /// </summary>
        bool[,] pathGrid;

        private int randomTargetDirection 
        {
            get
            {
                float time;
                if (GameManager.Instance != null)
                    time = GameManager.Instance.Level;
                else
                    time = 1;

                return Mathf.Max(Mathf.RoundToInt(targetDirectionVSLevelCurve.Evaluate(time, UnityEngine.Random.value)), 1);
            }
        }

        private int randomTraverseDirection
        {
            get
            {
                float time;
                if (GameManager.Instance != null)
                    time = GameManager.Instance.Level;
                else
                    time = 1;

                return Mathf.Max(Mathf.RoundToInt(traverseDirectionVSLevelCurve.Evaluate(time, UnityEngine.Random.value)), 1);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        
        #region Walkability

        private bool[,] _connectedToPath;

        private void BuildConnectedRegion()
        {
            _connectedToPath = new bool[mapSizeX, mapSizeY];

            if (pathGrid == null)
                return;

            Queue<Vector2Int> frontier = new Queue<Vector2Int>();

            // Seed from all path tiles
            for (int x = 0; x < mapSizeX; x++)
            {
                for (int y = 0; y < mapSizeY; y++)
                {
                    if (pathGrid[x, y])
                    {
                        _connectedToPath[x, y] = true;
                        frontier.Enqueue(new Vector2Int(x, y));
                    }
                }
            }

            // BFS outward to all orthogonally adjacent walkable tiles
            while (frontier.Count > 0)
            {
                Vector2Int current = frontier.Dequeue();

                foreach (Vector2Int neighbour in GetAdjacentNeighbours(current))
                {
                    if (_connectedToPath[neighbour.x, neighbour.y])
                        continue;

                    MapTile neighbourTile = Tiles[neighbour.x, neighbour.y];

                    if (!neighbourTile.TileType.IsWalkable())
                        continue;

                    _connectedToPath[neighbour.x, neighbour.y] = true;
                    frontier.Enqueue(neighbour);
                }
            }
        }

        public IEnumerable<Vector2Int> GetAdjacentNeighbours(Vector2Int pos)
        {
            if (pos.x > 0) yield return new Vector2Int(pos.x - 1, pos.y);
            if (pos.x < mapSizeX - 1) yield return new Vector2Int(pos.x + 1, pos.y);
            if (pos.y > 0) yield return new Vector2Int(pos.x, pos.y - 1);
            if (pos.y < mapSizeY - 1) yield return new Vector2Int(pos.x, pos.y + 1);

            if (pos.x > 0 && pos.y > 0) yield return new Vector2Int(pos.x - 1, pos.y - 1);
            if (pos.x < mapSizeX - 1 && pos.y > 0) yield return new Vector2Int(pos.x + 1, pos.y - 1);
            if (pos.x > 0 && pos.y < mapSizeY - 1) yield return new Vector2Int(pos.x - 1, pos.y + 1);
            if (pos.x < mapSizeX - 1 && pos.y < mapSizeY - 1) yield return new Vector2Int(pos.x + 1, pos.y + 1);
        }

        public bool IsPositionOutOfMap(Vector2 position)
        {
            Vector2Int v2IntPos = position.V2ToV2Int();
            return !Tiles.IsInBounds(v2IntPos.x, v2IntPos.y);
        }

        public bool IsConnectedToPath(Vector2Int tilePos)
        {
            if (_connectedToPath == null)
                return false;

            if (!_connectedToPath.IsInBounds(tilePos.x, tilePos.y))
                return false;

            return _connectedToPath[tilePos.x, tilePos.y];
        }

        public bool IsPositionOnFullyWalkableTile(Vector2 position)
        {
            Vector2Int v2IntPos = position.V2ToV2Int();

            if (!IsConnectedToPath(v2IntPos))
                return false;

            return Tiles[v2IntPos.x, v2IntPos.y].IsFullyWalkable;
        }

        public bool IsPositionWalkable(Vector2 position)
        {
            Vector2Int v2IntPos = position.V2ToV2Int();

            if (!IsConnectedToPath(v2IntPos))
                return false;

            MapTile tile = Tiles[v2IntPos.x, v2IntPos.y];
            if (tile == null)
                return false;

            return Tiles[v2IntPos.x, v2IntPos.y].IsPositionWalkable(position);
        }

        public bool TryFindNearestFullyWalkable(Vector2 givenPos, out Vector2 walkablePosition, int maxSearchRadius = -1)
        {
            walkablePosition = Vector2.zero;

            if (IsPositionOnFullyWalkableTile(givenPos))
            {
                walkablePosition = givenPos;
                return true;
            }

            Vector2Int centerTile = new Vector2Int(
                Mathf.Clamp(Mathf.RoundToInt(givenPos.x), 0, mapSizeX - 1),
                Mathf.Clamp(Mathf.RoundToInt(givenPos.y), 0, mapSizeY - 1)
            );

            int maxRadius = maxSearchRadius >= 0 ? maxSearchRadius : Mathf.Max(mapSizeX, mapSizeY);

            for (int radius = 1; radius <= maxRadius; radius++)
            {
                Vector2Int bestTile = default;
                bool foundTile = false;
                float minDistance = float.MaxValue;

                void CheckTile(Vector2Int tilePos)
                {
                    if (tilePos.x < 0 || tilePos.x >= mapSizeX || tilePos.y < 0 || tilePos.y >= mapSizeY)
                        return;

                    if (!IsConnectedToPath(tilePos))
                        return;

                    if (!Tiles[tilePos.x, tilePos.y].IsFullyWalkable)
                        return;

                    float distance = Vector2.Distance(givenPos, (Vector2)(Vector2)tilePos);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestTile = tilePos;
                        foundTile = true;
                    }
                }

                if (radius == 1)
                    CheckTile(centerTile);

                for (int x = -radius; x <= radius; x++)
                {
                    CheckTile(centerTile + new Vector2Int(x, -radius));
                    CheckTile(centerTile + new Vector2Int(x, radius));
                }

                for (int y = -radius + 1; y <= radius - 1; y++)
                {
                    CheckTile(centerTile + new Vector2Int(-radius, y));
                    CheckTile(centerTile + new Vector2Int(radius, y));
                }

                if (foundTile)
                {
                    walkablePosition = (Vector2)bestTile;
                    return true;
                }
            }

            return false;
        }

        public bool TryFindNearestWalkable(Vector2 givenPos, out Vector2 walkablePosition, int maxSearchRadius = -1)
        {
            if (IsPositionWalkable(givenPos))
            {
                walkablePosition = givenPos;
                return true;
            }

            Vector2Int centerTile = new Vector2Int(
                Mathf.Clamp(Mathf.RoundToInt(givenPos.x), 0, mapSizeX - 1),
                Mathf.Clamp(Mathf.RoundToInt(givenPos.y), 0, mapSizeY - 1)
            );

            int maxRadius = maxSearchRadius >= 0 ? maxSearchRadius : Mathf.Max(mapSizeX, mapSizeY);

            for (int radius = 1; radius <= maxRadius; radius++)
            {
                Vector2 candidateWalkablePos;
                Vector2 closestWalkablePosition = default;
                bool foundWalkable = false;
                float minDistance = float.MaxValue;

                if (radius == 1)
                {
                    if (TryGetNearestWalkablePositionInTile(centerTile, givenPos, out candidateWalkablePos))
                    {
                        float distance = Vector2.Distance(candidateWalkablePos, givenPos);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestWalkablePosition = candidateWalkablePos;
                        }
                        foundWalkable = true;
                    }
                }

                for (int x = -radius; x <= radius; x++)
                {
                    if (TryGetNearestWalkablePositionInTile(centerTile + new Vector2Int(x, -radius), givenPos, out candidateWalkablePos))
                    {
                        float distance = Vector2.Distance(candidateWalkablePos, givenPos);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestWalkablePosition = candidateWalkablePos;
                        }
                        foundWalkable = true;
                    }
                    if (TryGetNearestWalkablePositionInTile(centerTile + new Vector2Int(x, radius), givenPos, out candidateWalkablePos))
                    {
                        float distance = Vector2.Distance(candidateWalkablePos, givenPos);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestWalkablePosition = candidateWalkablePos;
                        }
                        foundWalkable = true;
                    }
                }

                for (int y = -radius + 1; y <= radius - 1; y++)
                {
                    if (TryGetNearestWalkablePositionInTile(centerTile + new Vector2Int(-radius, y), givenPos, out candidateWalkablePos))
                    {
                        float distance = Vector2.Distance(candidateWalkablePos, givenPos);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestWalkablePosition = candidateWalkablePos;
                        }
                        foundWalkable = true;
                    }
                    if (TryGetNearestWalkablePositionInTile(centerTile + new Vector2Int(radius, y), givenPos, out candidateWalkablePos))
                    {
                        float distance = Vector2.Distance(candidateWalkablePos, givenPos);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestWalkablePosition = candidateWalkablePos;
                        }
                        foundWalkable = true;
                    }
                }

                if (foundWalkable)
                {
                    walkablePosition = closestWalkablePosition;
                    return true;
                }
            }

            walkablePosition = default;
            return false;
        }

        private bool TryGetNearestWalkablePositionInTile(Vector2Int tilePos, Vector2 searchOrigin, out Vector2 walkablePosition)
        {
            walkablePosition = default;

            if (tilePos.x < 0 || tilePos.x >= mapSizeX || tilePos.y < 0 || tilePos.y >= mapSizeY)
                return false;

            if (!IsConnectedToPath(tilePos))
                return false;

            return Tiles[tilePos.x, tilePos.y].TryGetNearestWalkablePosition(searchOrigin, out walkablePosition);
        }


        #endregion

        #region Preview Settings
#if UNITY_EDITOR

        public void ShowMapPreview(Camera previewCamera,
            LocalMapGenerationSettings generationSettings,
            MapGenerationPreviewSettings previewSettings,
            List<int> terrainIndecies = null,
            Direction startWall = Direction.West,
            List<Direction> toDirections = null,
            List<Direction> otherFromDirections = null,
            int? nullableSeed = null)
        {
            if (startWall == default)
                startWall = Direction.West;

            if (toDirections == null || toDirections.Count == 0)
                toDirections = new List<Direction>() { startWall.OppositeDirection() };

            otherFromDirections ??= new List<Direction>();
            toDirections.RemoveAll(x => x == startWall);
            otherFromDirections.RemoveAll(x => x == startWall);

            int seed = nullableSeed.HasValue ? nullableSeed.Value : UnityEngine.Random.Range(0, 100000);

            CreateMap(startWall, toDirections, otherFromDirections, seed, generationSettings, terrainIndecies, previewSettings);
            SetupCamera(previewCamera);
        }

        private void SetupCamera(Camera camera)
        {
            camera.transform.position = new Vector3(MapSizeX / 2f - 0.5f, MapSizeY / 2f - 0.5f, -10f);
            camera.orthographicSize = MapSizeY / 1.6f;
        }

        #endif
        #endregion

        public void Clear()
        {
            occupiedSpots.Clear();

            DestroyMapObjects();

            DestroyTiles();

            DestoryRoadSigns();
        }

        private void SetupNewLevel()
        {
            GlobalMapGenerator generator = GlobalMapGenerator.Instance;

            Direction startWall = Direction.West;

            if (generator.takenPath.Count > 1)
            {
                foreach (var dirAndPoint in generator.CurrentMapPoint.connectedFromPoints)
                {
                    if (dirAndPoint.Value == generator.takenPath[generator.takenPath.Count - 2])
                    {
                        startWall = dirAndPoint.Key;
                        break;
                    }
                }
            }

            GlobalMapPoint currentMapPoint = generator.CurrentMapPoint;

            LocalMapGenerationSettings settings = currentMapPoint.GenerationSettings;

            if (settings == null)
            {
                Debug.LogError($"LocalMapGenerator: GlobalMapPoint at {currentMapPoint.Position} has no GenerationSettings assigned.");
                return;
            }

            List<(Direction direction, LocalMapGenerationSettings settings)> blends
                = new List<(Direction, LocalMapGenerationSettings)>();

            foreach (var kvp in currentMapPoint.connectedToPoints)
            {
                GlobalMapPoint toPoint = kvp.Value;

                // Only show biome blend previews toward eligible point types
                if (toPoint.Type != GlobalMapPointType.Normal && toPoint.Type != GlobalMapPointType.Relic)
                    continue;

                if (toPoint.GenerationSettings != null
                    && toPoint.GenerationSettings != settings)
                {
                    blends.Add((kvp.Key, toPoint.GenerationSettings));
                }
            }

            List<Direction> toDirections = currentMapPoint.GetOtherToDirections(startWall);
            List<Direction> otherFromDirections = currentMapPoint.GetOtherFromDirections(startWall);

            int seed = currentMapPoint.Seed;

            List<int> terrainIndecies = new List<int>();
            for (int i = 0; i < settings.terrainNoiseLists.Count; i++)
                terrainIndecies.Add(i);

            CreateMap(startWall, toDirections, otherFromDirections, seed, settings, terrainIndecies, blends: blends);
        }

        public void CreateMap(
            Direction startWall,
            List<Direction> toDirections,
            List<Direction> otherFromDirections,
            int seed,
            LocalMapGenerationSettings generationSettings,
            List<int> terrainNoiseIndecies = null,
            MapGenerationPreviewSettings previewSettings = MapGenerationPreviewSettings.MapObjects |
                                                           MapGenerationPreviewSettings.BaseTiles |
                                                           MapGenerationPreviewSettings.TerrainNoise,
            List<(Direction direction, LocalMapGenerationSettings settings)> blends = null)
        {
            _activeBlends = blends ?? new List<(Direction, LocalMapGenerationSettings)>();

            Clear();

            StartWall = startWall;

            UnityEngine.Random.InitState(seed);

            if (LocalCameraMovementManager.Instance != null)
                LocalCameraMovementManager.Instance.FullZoomCamera();

            if (previewSettings.HasFlag(MapGenerationPreviewSettings.BaseTiles))
            {
                CreatePath(startWall, toDirections, otherFromDirections);
                pathway.CreatePaths();
                PlaceRoadSigns();
            }

            PlaceTiles(generationSettings, previewSettings, terrainNoiseIndecies);

            BuildConnectedRegion();

            if (previewSettings.HasFlag(MapGenerationPreviewSettings.MapObjects))
                SpawnMapObjects(generationSettings);

            if (PathPicker.Instance != null)
                PathPicker.Instance.Setup();
        }

        private void DestroyMapObjects()
        {
            for (int i = 0; i < mapObjectGOs.Count; i++)
            {
                if (Application.isPlaying)
                {
                    Destroy(mapObjectGOs[i]);
                }
                else
                {
                    DestroyImmediate(mapObjectGOs[i]);
                }
            }

            mapObjectGOs.Clear();
        }

        private void SpawnMapObjects(LocalMapGenerationSettings generationSettings)
        {
            DestroyMapObjects();

            if (occupiedSpots == null)
                occupiedSpots = new List<Vector2Int>();

            // Collect all unique map object lists needed across the map,
            // picking the dominant biome per spawn attempt based on position
            int breaker;

            // We need to spawn from both base settings and any blended biomes,
            // using position to determine which biome owns each spawn point
            List<OverlayMapObject> baseObjects = generationSettings.mapObjects;

            // Spawn base biome objects — they get the whole map but lose spots near blend walls
            foreach (OverlayMapObject mapObject in baseObjects)
            {
                int amountToPlace = Mathf.CeilToInt(mapObject.amountToSpawnCurve.Evaluate(UnityEngine.Random.value));
                breaker = mapSizeX * mapSizeY;

                while (amountToPlace > 0 && breaker > 0)
                {
                    int xPos = UnityEngine.Random.Range(0, mapSizeX);
                    int yPos = UnityEngine.Random.Range(0, mapSizeY);
                    Vector2Int spawnPos = new Vector2Int(xPos, yPos);

                    // Only place if this position is dominated by the base biome
                    if (!SpotOccupied(spawnPos, mapObject.size, true) && IsBaseDominant(spawnPos))
                    {
                        PlaceMapObject(mapObject, xPos, yPos);
                        breaker = mapSizeX * mapSizeY;
                        amountToPlace--;
                    }
                    else
                    {
                        breaker--;
                    }
                }
            }

            // Spawn each blended biome's objects in their dominant zones
            foreach (var (direction, blendSettings) in _activeBlends)
            {
                if (blendSettings == null) continue;

                foreach (OverlayMapObject mapObject in blendSettings.mapObjects)
                {
                    int amountToPlace = Mathf.CeilToInt(mapObject.amountToSpawnCurve.Evaluate(UnityEngine.Random.value));
                    breaker = mapSizeX * mapSizeY;

                    while (amountToPlace > 0 && breaker > 0)
                    {
                        int xPos = UnityEngine.Random.Range(0, mapSizeX);
                        int yPos = UnityEngine.Random.Range(0, mapSizeY);
                        Vector2Int spawnPos = new Vector2Int(xPos, yPos);

                        if (!SpotOccupied(spawnPos, mapObject.size, true)
                            && GetDominantBlend(spawnPos) == blendSettings)
                        {
                            PlaceMapObject(mapObject, xPos, yPos);
                            breaker = mapSizeX * mapSizeY;
                            amountToPlace--;
                        }
                        else
                        {
                            breaker--;
                        }
                    }
                }
            }
        }

        private float GetBlendWeight(Direction direction, Vector2Int position)
        {
            float baseWeight = direction switch
            {
                Direction.West => 1f - (float)position.x / (mapSizeX - 1),
                Direction.East => (float)position.x / (mapSizeX - 1),
                Direction.South => 1f - (float)position.y / (mapSizeY - 1),
                Direction.North => (float)position.y / (mapSizeY - 1),
                _ => 0f
            };

            // Sample Perlin noise offset per direction so each boundary has independent variation
            float noiseOffset = direction switch
            {
                Direction.North => 0f,
                Direction.South => 100f,
                Direction.East => 200f,
                Direction.West => 300f,
                _ => 0f
            };

            float noise = Mathf.PerlinNoise(
                position.x * biomeBlendNoiseScale + noiseOffset,
                position.y * biomeBlendNoiseScale + noiseOffset);

            // Remap noise from 0-1 to -1..1 and apply strength
            float noiseShift = (noise - 0.5f) * 2f * biomeBlendNoiseStrength;

            return Mathf.Clamp01(baseWeight + noiseShift);
        }

        private LocalMapGenerationSettings GetDominantBlend(Vector2Int position)
        {
            LocalMapGenerationSettings dominant = null;
            float bestWeight = 0.5f;

            foreach (var (direction, settings) in _activeBlends)
            {
                float w = GetBlendWeight(direction, position);
                if (w > bestWeight)
                {
                    bestWeight = w;
                    dominant = settings;
                }
            }

            return dominant; // null means base is dominant
        }

        private bool IsBaseDominant(Vector2Int position)
        {
            return GetDominantBlend(position) == null;
        }


        private void PlaceMapObject(OverlayMapObject mapObject, int xPos, int yPos)
        {
            mapObjectGOs.Add(Instantiate(
                mapObject.overlay,
                new Vector3(xPos + (mapObject.size.x - 1) / 2f, yPos + (mapObject.size.y - 1) / 2f),
                Quaternion.identity,
                transform));

            if (mapObjects.ContainsKey(mapObject.name))
                mapObjects[mapObject.name]++;
            else
                mapObjects.Add(mapObject.name, 1);

            for (int x = 0; x < mapObject.size.x; x++)
                for (int y = 0; y < mapObject.size.y; y++)
                    occupiedSpots.Add(new Vector2Int(xPos + x, yPos + y));
        }

        private bool SpotOccupied(Vector2Int point, Vector2Int size, bool useVisualPath)
        {
            for (int xPos = point.x; xPos < point.x + size.x; xPos++)
            {
                for (int yPos = point.y; yPos < point.y + size.y; yPos++)
                {
                    if (
                            occupiedSpots.Contains(new Vector2Int(xPos, yPos)) ||
                            xPos > mapSizeX ||
                            yPos > mapSizeY ||
                            (useVisualPath ? 
                                pathGrid != null && xPos < pathGrid.GetLength(0) && yPos < pathGrid.GetLength(1) && pathGrid[xPos, yPos] : 
                                pathway != null && pathway.ContainsStep(new Vector2Int(xPos, yPos)))
                        )
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        private Direction GetNextDirection(Direction currentDirection, Direction targetDirection, Vector2Int position)
        {
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
                        position[1] = Mathf.Min(position[1] + (isTargetDir ? randomTargetDirection : randomTraverseDirection), mapSizeY - 1 - additional);
                        if (isTargetDir && position[1] == mapSizeY - 1)
                            position[1] = mapSizeY;
                        break;
                    case Direction.South:
                        position[1] = Mathf.Max(position[1] - (isTargetDir ? randomTargetDirection : randomTraverseDirection), additional);
                        if (isTargetDir && position[1] <= 1)
                            position[1] = -1;
                        break;
                    case Direction.East:
                        position[0] = Mathf.Min(position[0] + (isTargetDir ? randomTargetDirection : randomTraverseDirection), mapSizeX - 1 - additional);
                        if (isTargetDir && position[0] == mapSizeX - 1)
                            position[0] = mapSizeX;
                        break;
                    case Direction.West:
                        position[0] = Mathf.Max(position[0] - (isTargetDir ? randomTargetDirection : randomTraverseDirection), additional);
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
            List<Vector2Int> closestPoints = new List<Vector2Int>();

            int closestDistance = int.MaxValue;

            foreach (KeyValuePair<Direction, List<Vector2Int>> directionAndPath in pathway.SectionNodesPaths)
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

        private void DestroyTiles()
        {
            if (Tiles == null)
                return;

            for (int i = 0; i < Tiles.GetLength(0); i++)
            {
                for (int j = 0; j < Tiles.GetLength(1); j++)
                {
                    if (Application.isPlaying)
                    {
                        if (Tiles[i, j] != null)
                            Destroy(Tiles[i, j].gameObject);
                    }
                    else
                    {
                        if (Tiles[i, j] != null)
                            DestroyImmediate(Tiles[i, j].gameObject);
                    }
                }
            }
        }

        private void PlaceTiles(
            LocalMapGenerationSettings generationSettings,
            MapGenerationPreviewSettings previewSettings,
            List<int> terrainNoiseIndecies)
        {
            DestroyTiles();

            Tiles = new MapTile[mapSizeX, mapSizeY];

            generationSettings.SetupSeeds();
            foreach (var (_, blendSettings) in _activeBlends)
                blendSettings?.SetupSeeds();

            pathGrid = pathway != null ? GetPathGrid(generationSettings) : null;

            for (int xPos = 0; xPos < mapSizeX; xPos++)
            {
                for (int yPos = 0; yPos < mapSizeY; yPos++)
                {
                    MapTile mapTile = Instantiate(mapTilePrefab, new Vector3(xPos, yPos), Quaternion.identity,
                        maptileParent != null ? maptileParent : transform);

                    Vector2Int position = new Vector2Int(xPos, yPos);

                    int? level = null;
                    if (GameManager.Instance != null)
                        level = GameManager.Instance.Level;

                    if (pathGrid != null && pathGrid[xPos, yPos] && previewSettings.HasFlag(MapGenerationPreviewSettings.BaseTiles))
                    {
                        // Path tiles use the dominant biome's path sprite at this position
                        LocalMapGenerationSettings dominantSettings = GetDominantBlend(position) ?? generationSettings;
                        mapTile.Init(dominantSettings.GetPathTileSprite(level), position, true);
                    }
                    else
                    {
                        TileTypeAndVisual tileTypeAndSprite = GetMapTileSprite(generationSettings, position, level, previewSettings, terrainNoiseIndecies);
                        mapTile.Init(tileTypeAndSprite, position, tileTypeAndSprite.TileType.IsWalkable());
                    }

                    Tiles[xPos, yPos] = mapTile;
                }
            }

            // Autotile pass — unchanged
            for (int xPos = 0; xPos < mapSizeX; xPos++)
            {
                for (int yPos = 0; yPos < mapSizeY; yPos++)
                {
                    Grid3x3<TileType> tileTypeGrid = new Grid3x3<TileType>();
                    FillTileTypeGrid(tileTypeGrid, xPos, yPos);
                    Tiles[xPos, yPos].SetupAutoTiles(autoBaseTileSettings, tileTypeGrid);
                }
            }

            void FillTileTypeGrid(Grid3x3<TileType> boolGrid, int xPos, int yPos)
            {
                boolGrid.TopLeft = DownShiftXValid() || UpShiftYValid() ? TileType.Nothing : Tiles[xPos - 1, yPos + 1].TileType;
                boolGrid.TopCenter = UpShiftYValid() ? TileType.Nothing : Tiles[xPos, yPos + 1].TileType;
                boolGrid.TopRight = UpShiftXValid() || UpShiftYValid() ? TileType.Nothing : Tiles[xPos + 1, yPos + 1].TileType;
                boolGrid.CenterLeft = DownShiftXValid() ? TileType.Nothing : Tiles[xPos - 1, yPos].TileType;
                boolGrid.CenterRight = UpShiftXValid() ? TileType.Nothing : Tiles[xPos + 1, yPos].TileType;
                boolGrid.BottomLeft = DownShiftXValid() || DownShiftYValid() ? TileType.Nothing : Tiles[xPos - 1, yPos - 1].TileType;
                boolGrid.BottomCenter = DownShiftYValid() ? TileType.Nothing : Tiles[xPos, yPos - 1].TileType;
                boolGrid.BottomRight = UpShiftXValid() || DownShiftYValid() ? TileType.Nothing : Tiles[xPos + 1, yPos - 1].TileType;

                bool DownShiftXValid() => xPos - 1 < 0;
                bool UpShiftXValid() => xPos + 1 >= MapSizeX;
                bool DownShiftYValid() => yPos - 1 < 0;
                bool UpShiftYValid() => yPos + 1 >= MapSizeY;
            }
        }

        private TileTypeAndVisual GetMapTileSprite(
            LocalMapGenerationSettings generationSettings,
            Vector2Int position,
            float? level,
            MapGenerationPreviewSettings previewSettings,
            List<int> terrainNoiseIndecies)
        {
            if (_activeBlends.Count == 0)
                return generationSettings.GetMapTileSprite(position, level, previewSettings, terrainNoiseIndecies);

            // Find dominant biome at this position
            LocalMapGenerationSettings dominant = generationSettings;
            float bestWeight = 0.5f;

            foreach (var (direction, settings) in _activeBlends)
            {
                float w = GetBlendWeight(direction, position);
                if (w > bestWeight)
                {
                    bestWeight = w;
                    dominant = settings;
                }
            }

            return dominant.GetMapTileSprite(position, level, previewSettings, terrainNoiseIndecies);
        }

        private bool[,] GetPathGrid(LocalMapGenerationSettings generationSettings)
        {
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
            {
                for (int yPos = 0; yPos < mapSizeY; yPos++)
                {
                    Noise(xPos, yPos);
                }
            }

            for (int xPos = 0; xPos < mapSizeX; xPos++)
            {
                for (int yPos = 0; yPos < mapSizeY; yPos++)
                {
                    returnGrid[xPos, yPos] = noiseGrid[xPos, yPos] >= generationSettings.cullingAmount;
                }
            }

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

            return returnGrid;
        }

        private void DestoryRoadSigns()
        {
            foreach (GameObject obj in RoadSigns.Keys)
            {
                if(Application.isPlaying)
                    Destroy(obj);
                else
                    DestroyImmediate(obj);
            }
            RoadSigns.Clear();
        }

        private void PlaceRoadSigns()
        {
            DestoryRoadSigns();
            
            Dictionary<Direction, Vector2Int> endPoints = pathway.ToEndPoints;

            foreach(KeyValuePair<Direction, Vector2Int> kvp in endPoints)
            {
                Vector2 position = kvp.Value;

                switch (kvp.Key)
                {
                    case Direction.South:
                        position += new Vector2(CompatibilityExtensions.RandomSign(), 1);
                        break;
                    case Direction.North:
                        position += new Vector2(CompatibilityExtensions.RandomSign(), -1);
                        break;
                    case Direction.West:
                        position += new Vector2(1, CompatibilityExtensions.RandomSign());
                        break;
                    case Direction.East:
                        position += new Vector2(-1, CompatibilityExtensions.RandomSign());
                        break;
                }

                RoadSigns.Add(Instantiate(roadSignPrefab, position, Quaternion.identity, transform), kvp.Key);
                occupiedSpots.Add(position.V2ToV2Int());
            }
        }

        private void CreatePath(Direction startWall, List<Direction> toDirections, List<Direction> otherFromDirections)
        {
            foreach(Direction dir in toDirections)
            {
                if(otherFromDirections.Contains(dir))
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

            pathway = new Pathway(toDirections, otherFromDirections);

            for (int i = 0; i < toDirections.Count; i++)
            {
                Vector2Int startPoint = i == 0 ? pathStart : GetClosestNodeToTargetDirection(toDirections[i]);
                pathway[toDirections[i]].Add(startPoint);
                CreatePathFromStartToTargetDirection(pathway[toDirections[i]], startPoint, i == 0 ? startWall.OppositeDirection() : toDirections[i], toDirections[i]);
            }

            for (int i = 0; i < otherFromDirections.Count; i++)
            {
                Vector2Int startPoint = GetClosestNodeToTargetDirection(otherFromDirections[i]);
                pathway[otherFromDirections[i]].Add(startPoint);
                CreatePathFromStartToTargetDirection(pathway[otherFromDirections[i]], startPoint, otherFromDirections[i], otherFromDirections[i]);
            }
        }

        public void OnCreateLocalMap()
        {
            SetupNewLevel();
        }
    }

    [Flags]
    public enum MapGenerationPreviewSettings
    {
        BaseTiles = 1,
        MapObjects = 2,
        TerrainNoise = 4,
    }
}


