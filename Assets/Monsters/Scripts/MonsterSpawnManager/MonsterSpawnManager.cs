using GameManagerCalls;
using GlobalMap;
using Heros;
using LocalMap;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField]
        private MonsterSpawnSettings monsterSpawnSettings;

        [SerializeField, HideInInspector]
        public List<Monster> Monsters { private set; get; } = new();

        private LocalMapGenerator Map => LocalMapGenerator.Instance;

        private void Awake()
        {
            if (monsterSpawnSettings == null)
                Debug.LogWarning("MonsterSpawnSettings not assigned.");
        }

        public void AfterCreateLocalMap()
        {
            isSpawning = false;

            weightMap = new WeightMap(Map.pathway, monsterSpawnSettings, Map.MapSizeX, Map.MapSizeY);
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
