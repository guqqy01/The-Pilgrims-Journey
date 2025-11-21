using GameManagerCalls;
using Monsters;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using GlobalMap;
using Heros;
namespace LocalMap
{
    public class LocalMapGenerator : Singleton<LocalMapGenerator>, IOnClear, IOnCreateLocalMap
    {
        public Pathway pathway;

        [SerializeField]
        private MapTile mapTilePrefab;

        public MinMaxCurve targetDirectionVSLevelCurve;
        public MinMaxCurve traverseDirectionVSLevelCurve;

        public MapTile[,] Tiles { private set; get; }
        [SerializeField, HideInInspector]
        public List<GameObject> mapObjectGOs;

        [SerializeField]
        private GameObject roadSignPrefab;
        public Dictionary<GameObject, Direction> RoadSigns { get; private set; } = new Dictionary<GameObject, Direction>();

        [SerializeField]
        private Dictionary<string, int> mapObjects = new Dictionary<string, int>();

        private List<Vector2Int> occupiedSpots = new List<Vector2Int>();

        public LocalMapGenerationSettings generationSettings;

        public List<Direction> TargetDirections => pathway.ToDirections;

        public Direction StartWall { get; private set; }

        [SerializeField]
        private int mapSizeX = 32;
        public int MapSizeX => mapSizeX;

        [SerializeField]
        private int mapSizeY = 18;
        public int MapSizeY => mapSizeY;

        [SerializeField]
        private Transform maptileParent;

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

        #region Preview Settings
        #if UNITY_EDITOR
        
        public void ShowMapPreview(Camera previewCamera, LocalMapGenerationSettings generationSettings, MapGenerationPreviewSettings previewSettings, List<int> terrainIndecies = null)
        {
            Clear();

            StartWall = Direction.West;

            if (previewSettings.HasFlag(MapGenerationPreviewSettings.BaseTiles))
            {
                CreatePath(StartWall, new List<Direction>() { Direction.East }, new List<Direction>());
                pathway.CreatePaths();
            }

            GeneratePreviewMap(generationSettings, previewSettings, terrainIndecies);

            if(previewSettings.HasFlag(MapGenerationPreviewSettings.MapObjects))
            {
                SpawnMapObjects(generationSettings);
            }

            SetupCamera(previewCamera);
        }

        private void SetupCamera(Camera camera)
        {
            camera.transform.position = new Vector3(MapSizeX / 2f - 0.5f, MapSizeY / 2f - 0.5f, -10f);
            camera.orthographicSize = MapSizeY / 1.6f;
        }

        private void GeneratePreviewMap(LocalMapGenerationSettings generationSettings, MapGenerationPreviewSettings previewSettings, List<int> terrainIndecies)
        {
            DestroyTiles();

            Tiles = new MapTile[mapSizeX, mapSizeY];

            generationSettings.SetupSeeds();

            for (int i = 0; i < mapSizeX; i++)
            {
                for (int j = 0; j < mapSizeY; j++)
                {
                    Vector2Int position = new Vector2Int(i, j);

                    MapTile mapTile = null;

                    if (pathway != null && pathway.ContainsStep(position) && previewSettings.HasFlag(MapGenerationPreviewSettings.BaseTiles))
                    {
                        mapTile = Instantiate(mapTilePrefab, new Vector3(i, j), Quaternion.identity, transform);
                        mapTile.Init(generationSettings.pathTiles.GetRandom(), position);
                    }
                    else
                    {
                        Sprite sprite = generationSettings.GetMapTileSpriteForPreview(position, previewSettings, terrainIndecies);

                        if (sprite != null)
                        {
                            mapTile = Instantiate(mapTilePrefab, new Vector3(i, j), Quaternion.identity, transform);
                            mapTile.Init(sprite, position);
                        }
                    }

                    Tiles[i, j] = mapTile;
                }
            }
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

            if(generator.takenPath.Count > 1)
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

            CreateMap(startWall);
        }

        public void CreateMap(Direction startWall)
        {
            Clear();

            StartWall = startWall;

            UnityEngine.Random.InitState(GlobalMapGenerator.Instance.CurrentMapPoint.Seed);

            LocalCameraMovementManager.Instance.FullZoomCamera();

            occupiedSpots.Clear();

            CreatePath(startWall, GlobalMapGenerator.Instance.CurrentMapPoint.GetOtherToDirections(startWall), GlobalMapGenerator.Instance.CurrentMapPoint.GetOtherFromDirections(startWall));

            pathway.CreatePaths();

            PlaceTiles();

            PlaceRoadSigns();

            SpawnMapObjects(generationSettings);

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
            {
                occupiedSpots = new List<Vector2Int>();
            }


            foreach (OverlayMapObject mapObject in generationSettings.mapObjects)
            {
                int amountToPlace = Mathf.CeilToInt(mapObject.amountToSpawnCurve.Evaluate(UnityEngine.Random.value));

                int breaker = mapSizeX * mapSizeY;

                while (amountToPlace > 0 && breaker > 0)
                {
                    int i = UnityEngine.Random.Range(0, mapSizeX);
                    int j = UnityEngine.Random.Range(0, mapSizeY);

                    bool canPlace = true;

                    for (int x = 0; x < mapObject.size.x; x++)
                    {
                        for (int y = 0; y < mapObject.size.y; y++)
                        {
                            if (occupiedSpots.Contains(new Vector2Int(i + x, j + y)) ||
                                i + x > mapSizeX ||
                                j + y > mapSizeY ||
                                (pathway != null && pathway.ContainsStep(new Vector2Int(i + x, j + y))))
                            {
                                canPlace = false;
                            }
                        }
                    }

                    if (canPlace)
                    {
                        mapObjectGOs.Add(Instantiate(mapObject.overlay, new Vector3(i + (mapObject.size.x - 1) / 2f, j + (mapObject.size.y - 1) / 2f), Quaternion.identity, transform));

                        if (mapObjects.ContainsKey(mapObject.name))
                        {
                            mapObjects[mapObject.name]++;
                        }
                        else
                        {
                            mapObjects.Add(mapObject.name, 1);
                        }

                        for (int x = 0; x < mapObject.size.x; x++)
                        {
                            for (int y = 0; y < mapObject.size.y; y++)
                            {
                                occupiedSpots.Add(new Vector2Int(i + x, j + y));
                            }
                        }

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


        private bool SpotOccupied(Vector2Int point, Vector2Int size, out Vector2 spawnPosition)
        {
            spawnPosition = point;

            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    if (occupiedSpots.Contains(point + new Vector2Int(i, j)) || pathway.ContainsStep(point + new Vector2Int(i, j)))
                        return true;

                    if (point.x + i > mapSizeX || point.y + j > mapSizeY)
                        return true;
                }
            }

            spawnPosition = (Vector2)point + ((Vector2)size / 2f);

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

        private void PlaceTiles()
        {
            DestroyTiles();

            Tiles = new MapTile[mapSizeX, mapSizeY];

            generationSettings.SetupSeeds();

            for (int i = 0; i < mapSizeX; i++)
            {
                for (int j = 0; j < mapSizeY; j++)
                {
                    MapTile mapTile = Instantiate(mapTilePrefab, new Vector3(i, j), Quaternion.identity, maptileParent != null ? maptileParent : transform);

                    if (pathway.ContainsStep(new Vector2Int(i, j)))
                    {
                        mapTile.Init(generationSettings.GetPathTileSprite(), new Vector2Int(i, j));
                    }
                    else
                    {
                        mapTile.Init(generationSettings.GetMapTileSprite(new Vector2Int(i, j)), new Vector2Int(i, j));
                    }

                    Tiles[i, j] = mapTile;
                }
            }
        }

        private void DestoryRoadSigns()
        {
            foreach (GameObject obj in RoadSigns.Keys)
            {
                Destroy(obj);
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
                        position += new Vector2(ExtensionsAndHelper.RandomSign(), 1);
                        break;
                    case Direction.North:
                        position += new Vector2(ExtensionsAndHelper.RandomSign(), -1);
                        break;
                    case Direction.West:
                        position += new Vector2(1, ExtensionsAndHelper.RandomSign());
                        break;
                    case Direction.East:
                        position += new Vector2(-1, ExtensionsAndHelper.RandomSign());
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


