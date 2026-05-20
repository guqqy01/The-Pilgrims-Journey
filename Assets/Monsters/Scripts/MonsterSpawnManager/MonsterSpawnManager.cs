using GameManagerCalls;
using GlobalMap;
using Helper.Wait;
using LocalMap;
using System.Collections.Generic;
using System.Linq;
using Tiles;
using UnityEngine;

namespace Monsters
{
    /// <summary>
    /// Handles all monster spawning logic inside a local level:
    ///  • Normal spawns based on spawn definitions
    ///  • Wave-triggered spawns
    ///  • Boss spawn
    ///  • Horde/exponential spawn modes
    ///  • Occupied-grid tracking
    /// </summary>
    public partial class MonsterSpawnManager : Singleton<MonsterSpawnManager>,
        IOnPlayLocalLevel, IOnAfterCreateLocalMap, IOnClear, IOnCompleteLocalMapLevel
    {
        [SerializeField, HideInInspector]
        public List<Monster> Monsters { private set; get; } = new();

        private LocalMapGenerator Map => LocalMapGenerator.Instance;

        private MonsterSpawnSettings monsterSpawnSettings;

        private List<(MonsterSpawnSettings settings, Direction? blendDirection)> _allActiveSpawnSettings
            = new List<(MonsterSpawnSettings, Direction?)>();

        public void AfterCreateLocalMap()
        {
            isSpawning = false;
            BuildActiveSpawnSettings();

            if (monsterSpawnSettings == null)
                return;

            weightMap = new EnemySpawnWeightMap(
                Map.pathway,
                _allActiveSpawnSettings.Select(x => x.settings),
                Map.MapSizeX,
                Map.MapSizeY);
        }

        private void BuildActiveSpawnSettings()
        {
            _allActiveSpawnSettings.Clear();

            GlobalMapPoint currentPoint = GlobalMapGenerator.Instance.CurrentMapPoint;

            if (currentPoint.GenerationSettings == null)
            {
                Debug.LogWarning("MonsterSpawnManager: CurrentMapPoint has no GenerationSettings.");
                return;
            }

            monsterSpawnSettings = currentPoint.GenerationSettings.MonsterSpawnSettings;

            if (monsterSpawnSettings == null)
            {
                Debug.LogWarning($"MonsterSpawnManager: GenerationSettings at {currentPoint.Position} has no MonsterSpawnSettings.");
                return;
            }

            // Base biome — no direction constraint
            _allActiveSpawnSettings.Add((monsterSpawnSettings, null));

            // Blended biomes — each tied to their direction
            foreach (var (direction, blendSettings) in LocalMapGenerator.Instance.ActiveBlends)
            {
                if (blendSettings?.MonsterSpawnSettings == null) continue;
                _allActiveSpawnSettings.Add((blendSettings.MonsterSpawnSettings, direction));
            }
        }

        public void Clear()
        {
            ResetState();
        }

        public void OnCompleteLocalMapLevel()
        {
            ResetState();
        }

        public void OnPlayLocalLevel()
        {
            Random.InitState(GlobalMapGenerator.Instance.CurrentMapPoint.Seed);

            levelStartTime = Time.time;
            nextWaveIndex = 0;

            BossMonsterSpawn? bossSpawn = null;
            if (GlobalMapGenerator.Instance.CurrentMapPoint.Type == GlobalMapPointType.Relic)
                bossSpawn = monsterSpawnSettings.bossMonsters.GetRandom();

            BeginSpawning(bossSpawn);
        }

        private void Awake()
        {
            WaitForHelper.WaitForObjectThenPerformAction(() => TilePlayerManager.Instance, SubscribeToTilePlayer);
        }

        private void SubscribeToTilePlayer()
        {
            TilePlayerManager.Instance.onPositionChangeCurrentMonsterTiles -= SetupNewMonsterSpawnRates;
            TilePlayerManager.Instance.onPositionChangeCurrentMonsterTiles += SetupNewMonsterSpawnRates;
        }

        private void OnDestroy()
        {
            if(TilePlayerManager.Instance != null)
                TilePlayerManager.Instance.onPositionChangeCurrentMonsterTiles -= SetupNewMonsterSpawnRates;
        }

        private List<(MonsterSpawn, int)> spawns = new List<(MonsterSpawn, int)>();

        private void SetupNewMonsterSpawnRates(Dictionary<MonsterTile, int> tileAndSize)
        {
            spawns.Clear();

            foreach (var kvp in tileAndSize)
            {
                MonsterTile monsterTile = kvp.Key;
                int tileSize = kvp.Value;

                spawns.Add((monsterTile.MonsterSpawn, tileSize));
            }
        }

        private void Update()
        {
            if (!isSpawning)
                return;

            float now = Time.time;

            ProcessWaveTriggers(now);
            ProcessHordeTimer(now);
            ProcessNormalSpawns(now);
            HandleSpawnBossMinions(now);
        }
    }


}
