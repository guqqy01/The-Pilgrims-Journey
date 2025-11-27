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
    public class MonsterSpawnManager : Singleton<MonsterSpawnManager>,
        IOnPlayLocalLevel, IOnAfterCreateLocalMap, IOnClear, IOnCompleteLocalMapLevel
    {
        // ---------------------------------------------------------
        // Inspector Fields
        // ---------------------------------------------------------

        [SerializeField]
        private MonsterSpawnSettings monsterSpawnSettings;

        [SerializeField, HideInInspector]
        private List<Monster> monsters = new();

        private WeightMap weightMap;


        // ---------------------------------------------------------
        // Runtime State
        // ---------------------------------------------------------

        private readonly List<Vector2Int> occupiedSpots = new();
        private readonly Dictionary<MonsterSO, int> aliveCountByMonster = new();
        private readonly Dictionary<MonsterSO, float> nextSpawnTimeByMonster = new();

        private bool isSpawning = false;

        private float levelStartTime;
        private int nextWaveIndex = 0;

        // Horde mode state
        private bool hordeActive = false;
        private float hordeEndTime = 0f;
        private int hordeMultiplier = 1;
        private bool hordeIgnoreCaps = true;
        private Direction? hordePreferredSide = null;

        [SerializeField] private float hordeSpawnRate = 0.25f;   // seconds between horde spawns
        private float nextHordeSpawnTime = 0f;

        private readonly System.Random seededRandom = new();

        private LocalMapGenerator Map => LocalMapGenerator.Instance;

        private WeightedChanceList<Vector2> directMonsterWeightList = null;

        // ---------------------------------------------------------
        // Unity Events / Lifecycle
        // ---------------------------------------------------------

        private void Awake()
        {
            if (monsterSpawnSettings == null)
                Debug.LogWarning("MonsterSpawnSettings not assigned.");
        }

        public void AfterCreateLocalMap()
        {
            isSpawning = false;
            CreateWeightMap();
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


        // ---------------------------------------------------------
        // Update Loop
        // ---------------------------------------------------------

        private void Update()
        {
            if (!isSpawning)
                return;

            float now = Time.time;

            ProcessWaveTriggers(now);
            ProcessHordeTimer(now);
            ProcessNormalSpawns(now);
        }


        // ---------------------------------------------------------
        // Wave Logic
        // ---------------------------------------------------------

        private void ProcessWaveTriggers(float now)
        {
            if (nextWaveIndex >= monsterSpawnSettings.waves.Count)
                return;

            var wave = monsterSpawnSettings.waves[nextWaveIndex];
            if (now - levelStartTime >= wave.triggerTime)
            {
                SpawnWave(wave);
                nextWaveIndex++;
            }
        }


        // ---------------------------------------------------------
        // Horde Logic
        // ---------------------------------------------------------

        private void ProcessHordeTimer(float now)
        {
            if (!hordeActive)
                return;

            // Check timer end
            if (now >= hordeEndTime)
            {
                hordeActive = false;
                hordeMultiplier = 1;
                return;
            }

            // Horde spawn tick
            if (now >= nextHordeSpawnTime)
            {
                SpawnHordeBatch();
                nextHordeSpawnTime = now + hordeSpawnRate;
            }
        }

        private void SpawnHordeBatch()
        {
            // Use PathPicker as the authoritative spawn direction
            Direction side = PathPicker.Instance.PickedDirection.Value.OppositeDirection();

            foreach (var spawn in monsterSpawnSettings.rangeAndMonster)
            {
                MonsterSO monster = spawn.monster;

                EnsureMonsterTrackingExists(monster);

                if (!hordeIgnoreCaps && !CanSpawnMore(monster, spawn))
                    continue;

                TrySpawnHordeMonster(monster, side);
            }
        }

        private void TrySpawnHordeMonster(MonsterSO monster, Direction side)
        {
            List<Vector2Int> positions = GetAllSideTiles(side);

            // Shuffle for randomness
            positions = positions.OrderBy(_ => seededRandom.Next()).ToList();

            foreach (var p in positions)
            {
                if (SpotOccupied(p, monster.size, out Vector2 pos))
                    continue;

                Monster m = SpawnMonsterForceDirect(monster, pos);
                MarkOccupied(p, monster.size);

                aliveCountByMonster[monster] = aliveCountByMonster.GetValueOrDefault(monster) + 1;
                return;
            }
        }

        private List<Vector2Int> GetAllSideTiles(Direction side)
        {
            List<Vector2Int> result = new();

            int w = Map.MapSizeX;
            int h = Map.MapSizeY;

            switch (side)
            {
                case Direction.North:
                    for (int x = 0; x < w; x++)
                        result.Add(new Vector2Int(x, h - 1));
                    break;

                case Direction.South:
                    for (int x = 0; x < w; x++)
                        result.Add(new Vector2Int(x, 0));
                    break;

                case Direction.East:
                    for (int y = 0; y < h; y++)
                        result.Add(new Vector2Int(w - 1, y));
                    break;

                case Direction.West:
                    for (int y = 0; y < h; y++)
                        result.Add(new Vector2Int(0, y));
                    break;
            }

            return result;
        }

        private Monster SpawnMonsterForceDirect(MonsterSO monsterSO, Vector2 spawnPos)
        {
            Monster m = Instantiate(monsterSO.prefab, (Vector3)spawnPos, Quaternion.identity, transform);
            m.Init(monsterSO);

            // FORCE DIRECT MOVEMENT
            if (m.TryGetComponent(out MonsterMovement move))
                move.OverrideToDirect();

            monsters.Add(m);
            BeastaryManager.Instance.BufferMonster(monsterSO);

            if (m.TryGetComponent(out Health health))
                health.onDie += OnMonsterDie;

            return m;
        }

        // ---------------------------------------------------------
        // Normal Spawn Tick Logic
        // ---------------------------------------------------------

        private void ProcessNormalSpawns(float now)
        {
            foreach (var spawn in monsterSpawnSettings.rangeAndMonster)
            {
                var monsterSO = spawn.monster;

                EnsureMonsterTrackingExists(monsterSO);

                // Skip if already at cap
                if (!CanSpawnMore(monsterSO, spawn))
                    continue;

                // Skip if not time yet
                if (now < nextSpawnTimeByMonster[monsterSO])
                    continue;

                AttemptSpawnForMonster(spawn);

                // Always set next spawn time to prevent excessive loops
                nextSpawnTimeByMonster[monsterSO] = now + GetNextSpawnDelay(spawn);
            }
        }


        // ---------------------------------------------------------
        // Helpers: Monster Tracking
        // ---------------------------------------------------------

        private void EnsureMonsterTrackingExists(MonsterSO monster)
        {
            if (!aliveCountByMonster.ContainsKey(monster))
                aliveCountByMonster[monster] = 0;

            if (!nextSpawnTimeByMonster.ContainsKey(monster))
            {
                float delay = Random.Range(
                    0f,
                    monsterSpawnSettings
                        .rangeAndMonster
                        .Find(s => s.monster == monster)
                        .spawnRateVSLevel.Evaluate(GameManager.Instance.Level)
                );

                nextSpawnTimeByMonster[monster] = Time.time + delay;
            }
        }

        private bool CanSpawnMore(MonsterSO monster, MonsterSpawn spawn)
        {
            int baseMax = Mathf.FloorToInt(spawn.maxAmountVSLevel.Evaluate(GameManager.Instance.Level));

            return aliveCountByMonster[monster] < baseMax;
        }

        private float GetNextSpawnDelay(MonsterSpawn spawn)
        {
            float baseRate = Mathf.Max(0.01f, spawn.spawnRateVSLevel.Evaluate(GameManager.Instance.Level));
            float jitter = Random.Range(0.75f, 1.25f);
            return baseRate * jitter;
        }


        // ---------------------------------------------------------
        // Spawn Attempts
        // ---------------------------------------------------------

        private void AttemptSpawnForMonster(MonsterSpawn spawn)
        {
            MonsterSO monsterSO = spawn.monster;

            if (TrySpawnOne(spawn))
            {
                aliveCountByMonster[monsterSO]++;
            }
        }


        // ---------------------------------------------------------
        // One-Spawn Routing
        // ---------------------------------------------------------

        private bool TrySpawnOne(MonsterSpawn spawn)
        {
            var points = weightMap.WeightAndMapPoints(spawn.range);
            if (points == null || points.Count == 0)
                return false;

            Vector2Int hero = GetHeroGridPosition();

            return spawn.monster.movementType == MonsterMovementType.Direct
                ? TrySpawnDirect(spawn, points, hero)
                : TrySpawnStationary(spawn, points, hero);
        }

        private Vector2Int GetHeroGridPosition()
        {
            Vector3 pos = HeroManager.Instance.ActiveHero != null
                ? HeroManager.Instance.ActiveHero.transform.position
                : Vector3.zero;

            return new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));
        }


        // ---------------------------------------------------------
        // Direct Movement Spawning
        // ---------------------------------------------------------

        private bool TrySpawnDirect(
            MonsterSpawn spawn,
            List<KeyValuePair<int, List<Vector2Int>>> weightedPoints,
            Vector2Int heroGrid)
        {
            var list = new WeightedChanceList<Vector2>();
            int maxWeight = weightedPoints.Max(wp => wp.Key);

            foreach (var (weight, points) in weightedPoints)
            {
                foreach (var p in points)
                {
                    if (!IsWithinSpawnDistance(spawn, p, heroGrid))
                        continue;

                    if (SpotOccupied(p, spawn.monster.size, out Vector2 pos))
                        continue;

                    int invertedWeight = Mathf.Max(maxWeight - weight, 0);
                    list.Add(pos, invertedWeight);
                }
            }

            if (list.weightedList.Count == 0)
                return false;

            Vector2 spawnPos = list.GetRandom();
            SpawnMonster(spawn.monster, spawnPos);

            return true;
        }


        // ---------------------------------------------------------
        // Stationary Spawning
        // ---------------------------------------------------------

        private bool TrySpawnStationary(
            MonsterSpawn spawn,
            List<KeyValuePair<int, List<Vector2Int>>> weightedPoints,
            Vector2Int heroGrid)
        {
            foreach (var (_, points) in weightedPoints)
            {
                if (points == null || points.Count == 0)
                    continue;

                var shuffled = points.ToList();

                while (shuffled.Count > 0)
                {
                    int idx = seededRandom.Next(shuffled.Count);
                    var p = shuffled[idx];
                    shuffled.RemoveAt(idx);

                    if (!IsCandidateSuitable(spawn, p, heroGrid))
                        continue;

                    if (SpotOccupied(p, spawn.monster.size, out Vector2 spawnPos))
                        continue;

                    SpawnMonster(spawn.monster, spawnPos);
                    MarkOccupied(p, spawn.monster.size);
                    return true;
                }
            }

            return false;
        }

        private bool IsCandidateSuitable(
            MonsterSpawn spawn,
            Vector2Int p,
            Vector2Int heroGrid)
        {
            if (spawn.monster.movementType == MonsterMovementType.Direct)
            {
                if (!IsWithinSpawnDistance(spawn, p, heroGrid))
                    return false;
            }
            
            return true;
        }

        private bool IsWithinSpawnDistance(MonsterSpawn spawn, Vector2Int point, Vector2Int hero)
        {
            float dist = Vector2.Distance(point, hero);
            return dist >= spawn.minSpawnDistance && dist <= spawn.maxSpawnDistance;
        }


        // ---------------------------------------------------------
        // Core Spawn / Cleanup
        // ---------------------------------------------------------

        private void SpawnMonster(MonsterSO monsterSO, Vector2 spawnPos)
        {
            Monster m = Instantiate(monsterSO.prefab, (Vector3)spawnPos, Quaternion.identity, transform);
            m.Init(monsterSO);

            monsters.Add(m);
            BeastaryManager.Instance.BufferMonster(monsterSO);

            if (m.TryGetComponent(out Health health))
                health.onDie += OnMonsterDie;
        }

        private void MarkOccupied(Vector2Int origin, Vector2 size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                    occupiedSpots.Add(origin + new Vector2Int(x, y));
            }
        }

        public bool SpotOccupied(Vector2Int point, Vector2 size, out Vector2 spawnPosition)
        {
            spawnPosition = point;

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2Int cell = point + new Vector2Int(x, y);

                    if (occupiedSpots.Contains(cell) ||
                        Map.pathway.ContainsStep(cell))
                        return true;

                    if (cell.x < 0 || cell.y < 0 ||
                        cell.x >= Map.MapSizeX || cell.y >= Map.MapSizeY)
                        return true;
                }
            }

            spawnPosition = (Vector2)point + size / 2f;
            return false;
        }


        // ---------------------------------------------------------
        // Waves
        // ---------------------------------------------------------

        private void SpawnWave(Wave wave)
        {
            if (wave.entries == null || wave.entries.Count == 0)
                return;

            foreach (var entry in wave.entries)
            {
                MonsterSpawn spawnDef = monsterSpawnSettings.rangeAndMonster.FirstOrDefault(s => s.monster == entry.monster);

                for (int i = 0; i < entry.count; i++)
                {
                    if (!TrySpawnOne(spawnDef))
                        break;

                    aliveCountByMonster[entry.monster] =
                        aliveCountByMonster.GetValueOrDefault(entry.monster) + 1;
                }
            }
        }

        private MonsterSpawn CreateDefaultWaveSpawn(WaveEntry entry) =>
            new()
            {
                monster = entry.monster,
                range = entry.range,
                minSpawnDistance = 0,
                maxSpawnDistance = 0,
                maxAmountVSLevel = AnimationCurve.Constant(0, 1, 1),
                spawnRateVSLevel = AnimationCurve.Constant(0, 1, 1)
            };


        // ---------------------------------------------------------
        // Boss Spawn
        // ---------------------------------------------------------

        private void SpawnBoss(BossMonsterSpawn boss)
        {
            int index = 0;
            int limit = Map.MapSizeX * Map.MapSizeY;

            Vector2Int p = weightMap[boss.range, limit - 1];
            while (SpotOccupied(p, boss.boss.size, out Vector2 spawnPos) && limit-- > 0)
            {
                index++;
                p = weightMap[boss.range, Map.MapSizeX * Map.MapSizeY - 1 - index];
            }

            if (limit <= 0)
            {
                Debug.LogWarning("Could not find boss spawn position.");
                return;
            }

            SpawnMonster(boss.boss, p);
            MarkOccupied(p, boss.boss.size);

            aliveCountByMonster[boss.boss] =
                aliveCountByMonster.GetValueOrDefault(boss.boss) + 1;
        }


        // ---------------------------------------------------------
        // Weight Map Construction
        // ---------------------------------------------------------

        private void CreateWeightMap()
        {
            int maxRange = 0;

            foreach (var s in monsterSpawnSettings.rangeAndMonster)
                maxRange = Mathf.Max(maxRange, s.range);

            foreach (var boss in monsterSpawnSettings.bossMonsters)
                maxRange = Mathf.Max(maxRange, boss.range);

            weightMap = new WeightMap(maxRange, Map.MapSizeX, Map.MapSizeY);

            for (int x = 0; x < Map.MapSizeX; x++)
            {
                for (int y = 0; y < Map.MapSizeY; y++)
                {
                    // Skip tiles on path
                    if (Map.pathway.ContainsStep(x, y))
                        continue;

                    // Weight is based on proximity to the path
                    for (int r = 1; r <= maxRange; r++)
                        weightMap[r, x, y] = 0;

                    for (int dx = -maxRange; dx <= maxRange; dx++)
                    {
                        for (int dy = -maxRange; dy <= maxRange; dy++)
                        {
                            if (Map.pathway.ContainsStep(x + dx, y + dy))
                            {
                                int r = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
                                weightMap[r, x, y] += 2 * maxRange - Mathf.Abs(dx) - Mathf.Abs(dy);
                            }
                        }
                    }
                }
            }

            weightMap.Sort();
        }


        // ---------------------------------------------------------
        // Start / Reset / Horde API
        // ---------------------------------------------------------

        private void BeginSpawning(BossMonsterSpawn? boss)
        {
            DestroyExistingMonsters();

            aliveCountByMonster.Clear();
            nextSpawnTimeByMonster.Clear();

            // Setup spawn timers for each monster type
            foreach (var spawn in monsterSpawnSettings.rangeAndMonster)
            {
                EnsureMonsterTrackingExists(spawn.monster);
            }

            if (boss.HasValue)
                SpawnBoss(boss.Value);

            // Pre-spawn stationary monsters to fill their caps
            PreSpawnStationaryMonsters();

            levelStartTime = Time.time;
            nextWaveIndex = 0;
            isSpawning = true;
        }

        private void ResetState()
        {
            isSpawning = false;
            occupiedSpots.Clear();

            monsters.DestroyAndClearList();
            aliveCountByMonster.Clear();
            nextSpawnTimeByMonster.Clear();

            nextWaveIndex = 0;
            hordeActive = false;
        }

        private void DestroyExistingMonsters()
        {
            foreach (var m in monsters)
            {
                if (m == null)
                    continue;

#if UNITY_EDITOR
                if (Application.isPlaying)
                    Destroy(m.gameObject);
                else
                    DestroyImmediate(m.gameObject);
#else
                Destroy(m.gameObject);
#endif
            }

            monsters.Clear();
            occupiedSpots.Clear();
        }

        private void PreSpawnStationaryMonsters()
        {
            foreach (var spawn in monsterSpawnSettings.rangeAndMonster
                         .Where(s => s.monster.movementType == MonsterMovementType.Stationary))
            {
                int amount = Mathf.FloorToInt(spawn.maxAmountVSLevel.Evaluate(GameManager.Instance.Level));

                for (int i = 0; i < amount; i++)
                {
                    int idx = i;
                    int limit = Map.MapSizeX * Map.MapSizeY;

                    Vector2Int p = weightMap[spawn.range, limit - 1];

                    while (SpotOccupied(p, spawn.monster.size, out Vector2 _) && limit-- > 0)
                    {
                        idx++;
                        p = weightMap[spawn.range, Map.MapSizeX * Map.MapSizeY - 1 - idx];
                    }

                    if (limit <= 0)
                        break;

                    SpawnMonster(spawn.monster, p);
                    MarkOccupied(p, spawn.monster.size);

                    aliveCountByMonster[spawn.monster] =
                        aliveCountByMonster.GetValueOrDefault(spawn.monster) + 1;
                }
            }
        }


        // ---------------------------------------------------------
        // Monster Death Callback
        // ---------------------------------------------------------

        private void OnMonsterDie(Health health)
        {
            if (!health.TryGetComponent(out Monster monster))
                return;

            HandleStationaryDeath(monster);
            aliveCountByMonster[monster.MonsterSO]--;
            monsters.Remove(monster);

            health.onDie -= OnMonsterDie;

            HandleRespawnDelay(monster.MonsterSO);
        }

        private void HandleStationaryDeath(Monster m)
        {
            if (!m.TryGetComponent(out MonsterMovement movement))
                return;

            Vector2Int pos = movement.StationaryPosition;
            if (pos == default)
                return;

            for (int x = 0; x < m.MonsterSO.size.x; x++)
            {
                for (int y = 0; y < m.MonsterSO.size.y; y++)
                    occupiedSpots.Remove(pos + new Vector2Int(x, y));
            }
        }

        private void HandleRespawnDelay(MonsterSO monster)
        {
            var spawn = monsterSpawnSettings.rangeAndMonster.Find(s => s.monster == monster);

            int previous = aliveCountByMonster[monster];
            int maxAllowed = Mathf.FloorToInt(spawn.maxAmountVSLevel.Evaluate(GameManager.Instance.Level));

            if (previous >= maxAllowed)
            {
                float now = Time.time;
                nextSpawnTimeByMonster[monster] = now + GetNextSpawnDelay(spawn);
            }
        }


        // ---------------------------------------------------------
        // Horde API
        // ---------------------------------------------------------

        public void StartHorde(int multiplier, float duration, bool ignoreCap = true, Direction? side = null)
        {
            hordeActive = true;
            hordeMultiplier = Mathf.Max(1, multiplier);
            hordeIgnoreCaps = ignoreCap;
            hordeEndTime = Time.time + Mathf.Max(0.1f, duration);
            hordePreferredSide = side;
        }
    }


    // Small helper extensions
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(
            this Dictionary<TKey, TValue> dict,
            TKey key)
        {
            dict.TryGetValue(key, out TValue v);
            return v;
        }
    }
}
