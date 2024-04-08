using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class LocalMapGenerator : MonoBehaviour
    {
        public static LocalMapGenerator Instance { private set; get; }

        public Pathway pathway;

        public List<Direction> targetDirections;
        public Direction startDirection;

        [SerializeField, HideInInspector]
        private List<GameObject> tiles;
        [SerializeField, HideInInspector]
        private List<GameObject> enemys;
        [SerializeField, HideInInspector]
        private List<GameObject> mapObjectGOs;

        private Dictionary<string, int> mapObjects = new Dictionary<string, int>();

        private List<Vector2Int> occupiedSpots = new List<Vector2Int>();

        public MonsterSpawnSettings monsterSettings;

        private WeightMap weightMap;

        public LocalMapGenerationSettings generationSettings;

        [SerializeField]
        private Camera cam;

        private int mapSizeX
        {
            get
            {
                int value = Mathf.RoundToInt(generationSettings.mapSize.Evaluate(GameManager.Instance.Level, randomTimeValueForMapSize)) * 16;
                return value;
            }
        } 
        private int mapSizeY
        {
            get
            {
                int value = Mathf.RoundToInt(generationSettings.mapSize.Evaluate(GameManager.Instance.Level, randomTimeValueForMapSize)) * 9;
                return value;
            }
        }

        private int randomTargetDirection => Mathf.RoundToInt(generationSettings.targetDirectionVSLevelCurve.Evaluate(GameManager.Instance.Level, Random.value));
        private int randomTraverseDirection => Mathf.RoundToInt(generationSettings.traverseDirectionVSLevelCurve.Evaluate(GameManager.Instance.Level, Random.value));

        private float randomTimeValueForMapSize;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }


        public void CreateMap()
        {
            randomTimeValueForMapSize = Random.value;

            SetupCamera();

            CreatePath(startDirection, targetDirections);

            CreateEnemyWeightMap();

            occupiedSpots.Clear();

            SpawnEnemys();

            SpawnMapObjects();

            PathPicker.instance?.Setup();
        }

        private void SetupCamera()
        {
            cam.transform.position = new Vector3(mapSizeX / 2f - 0.5f, mapSizeY / 2f - 0.5f, -10f);
            cam.orthographicSize = mapSizeY / 2f;
        }

        public void DestroyMap()
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

        private void SpawnMapObjects()
        {
            DestroyMap();


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
                                pathway.ContainsStep(new Vector2Int(i + x, j + y)))
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

        private void CreateEnemyWeightMap()
        {
            int maxRange = 0;

            for (int i = 0; i < monsterSettings.rangeAndMonster.Count; i++)
            {
                if (monsterSettings.rangeAndMonster[i].range > maxRange)
                    maxRange = monsterSettings.rangeAndMonster[i].range;
            }

            weightMap = new WeightMap(maxRange, mapSizeX, mapSizeY);

            for (int i = 0; i < mapSizeX; i++)
            {
                for (int j = 0; j < mapSizeY; j++)
                {

                    for (int k = 1; k <= maxRange; k++)
                    {
                        weightMap[k, i, j] = 0;
                    }
                    
                    if (pathway.ContainsStep(i, j))
                        continue;

                    for (int x = - maxRange; x <= maxRange; x++)
                    {
                        for (int y = - maxRange; y <= maxRange; y++)
                        {
                            if (pathway.ContainsStep(i + x, j + y))
                            {
                                int range = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

                                weightMap[range, i, j] += 2 * maxRange - Mathf.Abs(x) - Mathf.Abs(y);
                            }
                        }
                    }
                }
            }

            weightMap.Sort();
        }

        public void DestoryEnemys() 
        {
            for (int i = 0; i < enemys.Count; i++)
            {
                if (Application.isPlaying)
                {
                    Destroy(enemys[i]);
                }
                else
                {
                    DestroyImmediate(enemys[i]);
                }
            }

            enemys.Clear();
        }

        private void SpawnEnemys()
        {
            DestoryEnemys();

            foreach (MonsterSpawn spawn in monsterSettings.rangeAndMonster)
            {
                for (int i = 0; i < spawn.spawnAmountVSLevel.Evaluate(GameManager.Instance.Level); i++)
                {
                    int spawnIndex = i;

                    Vector2Int point = weightMap[spawn.range, mapSizeX * mapSizeY - 1 - spawnIndex];

                    int breaker = mapSizeX * mapSizeY;

                    while (occupiedSpots.Contains(point) && breaker > 0)
                    {
                        point = weightMap[spawn.range, mapSizeX * mapSizeY - 1 - spawnIndex];

                        spawnIndex++;
                        breaker--;
                    }

                    enemys.Add(Instantiate(spawn.monster, new Vector3(point.x, point.y), Quaternion.identity, transform));

                    occupiedSpots.Add(point);
                }
            }
        }

        private Direction GetNextDirection(Direction currentDirection, Direction targetDirection, Vector2Int position)
        {
            if (targetDirection != currentDirection)
                return targetDirection;

            if (Mathf.Abs((int)currentDirection) == 2)
            {
                float dirRan = UnityEngine.Random.Range(1f / mapSizeX, 1f - 1f / mapSizeX);
                return (dirRan > (float)position.x / mapSizeX) ? Direction.Right : Direction.Left;
            }
            else
            {
                float dirRan = UnityEngine.Random.Range(1f / mapSizeY, 1f - 1f / mapSizeY);
                return (dirRan > (float)position.y / mapSizeY) ? Direction.Up : Direction.Down;
            }
        }

        

        public void CreatePathFromStartToTargetDirection(List<Vector2Int> path, Vector2Int start, Direction startDirection, Direction targetDirection)
        {
            Direction currentDirection = startDirection;
            int[] position = new int[2];

            position[0] = start.x;
            position[1] = start.y;

            int targetInt = (int)targetDirection > 0 ? (targetDirection == Direction.Right) ? mapSizeX : mapSizeY : 0;

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
                        position[1] = Mathf.Min(position[1] + (isTargetDir ? randomTargetDirection : randomTraverseDirection), mapSizeY - additional);
                        break;
                    case Direction.Left:
                        position[0] = Mathf.Max(position[0] - (isTargetDir ? randomTargetDirection : randomTraverseDirection), additional);
                        break;
                    case Direction.Right:
                        position[0] = Mathf.Min(position[0] + (isTargetDir ? randomTargetDirection : randomTraverseDirection), mapSizeX - additional);
                        break;
                    case Direction.Down:
                        position[1] = Mathf.Max(position[1] - (isTargetDir ? randomTargetDirection : randomTraverseDirection), additional);
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

            foreach (KeyValuePair<Direction, List<Vector2Int>> directionAndPath in pathway.SectionNodesPaths)
            {
                List<Vector2Int> path = directionAndPath.Value;

                if (path == null || path.Count == 0)
                    continue;

                for (int i = 0; i < path.Count; i++)
                {
                    if (path[i].x == 0 || path[i].x == mapSizeX || path[i].y == 0 || path[i].y == mapSizeY)
                        continue;

                    switch (targetDirection)
                    {
                        case Direction.Up:
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

            return closestPoints[UnityEngine.Random.Range(0, closestPoints.Count)];
        }

        public void DestroyTiles()
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                if (Application.isPlaying)
                {
                    Destroy(tiles[i]);
                }
                else
                {
                    DestroyImmediate(tiles[i]);
                }
            }

            tiles.Clear();
        }



        private void PlaceTiles()
        {
            DestroyTiles();

            for (int i = 0; i < mapSizeX/* + 1*/; i++)
            {
                for (int j = 0; j < mapSizeY/* + 1*/; j++)
                {
                    if (pathway.ContainsStep(new Vector2Int(i, j)))
                    {
                        tiles.Add(Instantiate(generationSettings.GetPathTile(), new Vector3(i, j), Quaternion.identity, transform));
                    }
                    else
                    {
                        tiles.Add(Instantiate(generationSettings.GetMapTile(), new Vector3(i, j), Quaternion.identity, transform));
                    }
                }
            }
        }

        

        public void CreatePath(Direction startDirection, List<Direction> targetDirections)
        {
            Vector2Int pathStart = new Vector2Int();

            switch (startDirection)
            {
                case Direction.Down:
                    pathStart = new Vector2Int(mapSizeX / 2, mapSizeY);
                    break;
                case Direction.Left:
                    pathStart = new Vector2Int(mapSizeX, mapSizeY / 2);
                    break;
                case Direction.Right:
                    pathStart = new Vector2Int(0, mapSizeY / 2);
                    break;
                case Direction.Up:
                    pathStart = new Vector2Int(mapSizeX / 2, 0);
                    break;
            }

            pathway = new Pathway(targetDirections);

            for (int i = 0; i < targetDirections.Count; i++)
            {
                Vector2Int startPoint = i == 0 ? pathStart : GetClosestNodeToTargetDirection(targetDirections[i]);
                pathway[targetDirections[i]].Add(startPoint);
                CreatePathFromStartToTargetDirection(pathway[targetDirections[i]], startPoint, i == 0 ? startDirection : targetDirections[i], targetDirections[i]);
            }

            pathway.CreatePaths(targetDirections);

            PlaceTiles();
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


