using GameManagerCalls;
using GlobalMap;
using LocalMap.Editor;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace LocalMap
{
    public class LocalMapGenerator : Singleton<LocalMapGenerator>, IOnClear, IOnCreateLocalMap
    {
        public AutoTileSettings autoBaseTileSettings;

        [SerializeField] private MapTile mapTilePrefab;
        [SerializeField] private GameObject roadSignPrefab;
        [SerializeField] private Transform maptileParent;

        [SerializeField] private int mapSizeX = 32;
        [SerializeField] private int mapSizeY = 18;

        [Header("Visual Border")]
        [SerializeField] private int borderSize = 4;

        [SerializeField] private float biomeBlendNoiseScale = 0.15f;
        [SerializeField] private float biomeBlendNoiseStrength = 0.25f;

        [SerializeField, HideInInspector]
        public List<GameObject> mapObjectGOs;

        public MinMaxCurve targetDirectionVSLevelCurve;
        public MinMaxCurve traverseDirectionVSLevelCurve;

        internal LocalMapWalkability Walkability { get; private set; }
        internal LocalMapPathGenerator PathGenerator { get; private set; }
        internal LocalMapBiomeBlender BiomeBlender { get; private set; }
        internal LocalMapBorderTiles BorderTiles { get; private set; }
        internal LocalMapObjectSpawner ObjectSpawner { get; private set; }
        internal LocalMapRoadSignPlacer RoadSignPlacer { get; private set; }
        internal LocalMapTileGrid TileGrid { get; private set; }

        public Pathway Pathway { internal set; get; }
        public MapTile[,] Tiles { internal set; get; }
        public Direction StartWall { internal set; get; }

        /// x and y are positions, returns if part of visual path, which will be bigger than the actual path taken.
        internal bool[,] PathGrid { get; set; }

        public Dictionary<GameObject, Direction> RoadSigns => RoadSignPlacer.RoadSigns;
        public IReadOnlyList<(Direction direction, LocalMapGenerationSettings settings)> ActiveBlends => BiomeBlender.ActiveBlends;
        public List<Direction> TargetDirections => Pathway.ToDirections;

        public int MapSizeX => mapSizeX;
        public int MapSizeY => mapSizeY;
        internal int BorderSize => borderSize;
        internal float BiomeBlendNoiseScale => biomeBlendNoiseScale;
        internal float BiomeBlendNoiseStrength => biomeBlendNoiseStrength;
        internal MapTile MapTilePrefab => mapTilePrefab;
        internal GameObject RoadSignPrefab => roadSignPrefab;
        internal Transform MaptileParent => maptileParent;

        protected override void Awake()
        {
            base.Awake();

            Walkability = new LocalMapWalkability(this);
            PathGenerator = new LocalMapPathGenerator(this);
            BiomeBlender = new LocalMapBiomeBlender(this);
            BorderTiles = new LocalMapBorderTiles(this);
            ObjectSpawner = new LocalMapObjectSpawner(this);
            RoadSignPlacer = new LocalMapRoadSignPlacer(this);
            TileGrid = new LocalMapTileGrid(this);
        }

        #region Walkability
        // Thin forwarders so the public API is unchanged for the rest of the codebase.
        public IEnumerable<Vector2Int> GetAdjacentNeighbours(Vector2Int pos) => Walkability.GetAdjacentNeighbours(pos);
        public bool IsPositionOutOfMap(Vector2 position) => Walkability.IsPositionOutOfMap(position);
        public bool IsConnectedToPath(Vector2Int tilePos) => Walkability.IsConnectedToPath(tilePos);
        public TileType? GetTileTypeAtPosition(Vector2 position) => Walkability.GetTileTypeAtPosition(position);
        public bool IsPositionOnFullyWalkableTile(Vector2 position) => Walkability.IsPositionOnFullyWalkableTile(position);
        public bool IsPositionWalkable(Vector2 position) => Walkability.IsPositionWalkable(position);
        public bool TryFindNearestFullyWalkable(Vector2 givenPos, out Vector2 walkablePosition, int maxSearchRadius = -1)
            => Walkability.TryFindNearestFullyWalkable(givenPos, out walkablePosition, maxSearchRadius);
        public bool TryFindNearestWalkable(Vector2 givenPos, out Vector2 walkablePosition, int maxSearchRadius = -1)
            => Walkability.TryFindNearestWalkable(givenPos, out walkablePosition, maxSearchRadius);
        public void RebuildConnectivity() => Walkability.BuildConnectedRegion();
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
            if (ObjectSpawner != null)
            {
                ObjectSpawner.ClearOccupied();
                ObjectSpawner.DestroyMapObjects();
            }
            if(TileGrid != null)
                TileGrid.DestroyTiles();
            if(RoadSignPlacer != null)
                RoadSignPlacer.DestroyRoadSigns();
        }

        private void SetupNewLevel()
        {
            GlobalMapGenerator generator = GlobalMapGenerator.Instance;

            Direction startWall = Direction.West;

            if (generator.TakenPath.Count > 1)
            {
                foreach (var dirAndPoint in generator.CurrentMapPoint.connectedFromPoints)
                {
                    if (dirAndPoint.Value == generator.TakenPath[generator.TakenPath.Count - 2])
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
            if(LocalMapThemeManager.Instance != null)
                LocalMapThemeManager.Instance.ChangeMapTheme(generationSettings.LocalMapTheme);

            BiomeBlender.SetActiveBlends(blends);

            Clear();

            StartWall = startWall;

            UnityEngine.Random.InitState(seed);

            if (LocalCameraMovementManager.Instance != null)
                LocalCameraMovementManager.Instance.FullZoomCamera();

            if (previewSettings.HasFlag(MapGenerationPreviewSettings.BaseTiles))
            {
                PathGenerator.CreatePath(startWall, toDirections, otherFromDirections);
                Pathway.CreatePaths();
                RoadSignPlacer.PlaceRoadSigns();
            }

            TileGrid.PlaceTiles(generationSettings, previewSettings, terrainNoiseIndecies);
            BorderTiles.PlaceBorderTiles(generationSettings, previewSettings, terrainNoiseIndecies);
            BorderTiles.RunAutoTilePass();

            if (previewSettings.HasFlag(MapGenerationPreviewSettings.MapObjects))
                ObjectSpawner.SpawnMapObjects(generationSettings);

            Walkability.BuildConnectedRegion();

            if (PathPicker.Instance != null)
                PathPicker.Instance.Setup();
        }

        public MapTile GetTileAt(int x, int y)
        {
            if (x >= 0 && x < mapSizeX && y >= 0 && y < mapSizeY)
                return Tiles[x, y];

            return BorderTiles.GetBorderTile(x, y);
        }

        public T SpawnObject<T>(T spawnedObject, string name, Vector2Int size, TileType validTileTypes) where T : MonoBehaviour
            => ObjectSpawner.SpawnObject(spawnedObject, name, size, validTileTypes);

        public Direction? GetDominantBlendDirection(Vector2Int position) => BiomeBlender.GetDominantBlendDirection(position);

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
