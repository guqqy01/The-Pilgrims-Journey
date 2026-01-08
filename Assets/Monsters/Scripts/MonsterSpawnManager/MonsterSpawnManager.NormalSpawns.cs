using Heros;
using LocalMap;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Monsters
{

    /// <summary>
    /// State
    /// </summary>
    public partial class MonsterSpawnManager
    {
        private WeightMap weightMap;

        private readonly List<Vector2Int> occupiedSpots = new();
        private readonly Dictionary<MonsterSO, int> aliveCountByMonster = new();
        private readonly Dictionary<MonsterSO, float> nextSpawnTimeByMonster = new();

        private bool isSpawning;
        private float levelStartTime;
        private int nextWaveIndex;

        private readonly System.Random seededRandom = new();

        private void EnsureMonsterTrackingExists(MonsterSO monster)
        {
            if (!aliveCountByMonster.ContainsKey(monster))
                aliveCountByMonster[monster] = 0;

            if (!nextSpawnTimeByMonster.ContainsKey(monster))
            {
                var spawn = monsterSpawnSettings.rangeAndMonster
                    .Find(s => s.monster == monster);

                float delay = Random.Range(
                    0f,
                    spawn.spawnRateVSLevel.Evaluate(GameManager.Instance.Level)
                );

                nextSpawnTimeByMonster[monster] = Time.time + delay;
            }
        }

        private bool CanSpawnMore(MonsterSO monster, MonsterSpawn spawn)
        {
            int max = Mathf.FloorToInt(
                spawn.maxAmountVSLevel.Evaluate(GameManager.Instance.Level)
            );

            return aliveCountByMonster[monster] < max;
        }
    }


    /// <summary>
    /// Normal Spawns
    /// </summary>
    public partial class MonsterSpawnManager
    {
        
        private void ProcessNormalSpawns(float now)
        {
            foreach (var spawn in monsterSpawnSettings.rangeAndMonster)
            {
                var monster = spawn.monster;

                EnsureMonsterTrackingExists(monster);

                if (!CanSpawnMore(monster, spawn))
                    continue;

                if (now < nextSpawnTimeByMonster[monster])
                    continue;

                if (TrySpawnOne(spawn))
                    aliveCountByMonster[monster]++;

                nextSpawnTimeByMonster[monster] =
                    now + GetNextSpawnDelay(spawn);
            }
        }

        private float GetNextSpawnDelay(MonsterSpawn spawn)
        {
            float baseRate = Mathf.Max(
                0.01f,
                spawn.spawnRateVSLevel.Evaluate(GameManager.Instance.Level)
            );

            return baseRate * Random.Range(0.75f, 1.25f);
        }

        
    }


    /// <summary>
    /// Direct Spawns
    /// </summary>
    public partial class MonsterSpawnManager
    {
        private bool TrySpawnDirect(
            MonsterSpawn spawn,
            List<KeyValuePair<int, List<Vector2Int>>> weightedPoints,
            Vector2Int heroGrid)
        {
            var list = new WeightedChanceList<Vector2>();
            int maxWeight = weightedPoints.Max(w => w.Key);

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

            SpawnMonster(spawn.monster, list.GetRandom());
            return true;
        }
    }


    /// <summary>
    /// Boss Spawning
    /// </summary>
    public partial class MonsterSpawnManager
    {
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

    }

    /// <summary>
    /// Stationary Spawns
    /// </summary>
    public partial class MonsterSpawnManager
    {
        private bool TrySpawnStationary(
            MonsterSpawn spawn,
            List<KeyValuePair<int, List<Vector2Int>>> weightedPoints,
            Vector2Int heroGrid)
        {
            foreach (var (_, points) in weightedPoints)
            {
                var shuffled = points.OrderBy(_ => seededRandom.Next()).ToList();

                foreach (var p in shuffled)
                {
                    if (!IsWithinSpawnDistance(spawn, p, heroGrid))
                        continue;

                    if (SpotOccupied(p, spawn.monster.size, out Vector2 pos))
                        continue;

                    SpawnMonster(spawn.monster, pos);
                    MarkOccupied(p, spawn.monster.size);
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Wave Spawning
    /// </summary>
    public partial class MonsterSpawnManager
    {
        private void ProcessWaveTriggers(float now)
        {
            if (nextWaveIndex >= monsterSpawnSettings.waves.Count)
                return;

            var wave = monsterSpawnSettings.waves[nextWaveIndex];
            if (now - levelStartTime < wave.triggerTime)
                return;

            SpawnWave(wave);
            nextWaveIndex++;
        }

        private void SpawnWave(Wave wave)
        {
            foreach (var entry in wave.entries)
            {
                var spawnDef = monsterSpawnSettings.rangeAndMonster
                    .FirstOrDefault(s => s.monster == entry.monster);

                for (int i = 0; i < entry.count; i++)
                {
                    if (!TrySpawnOne(spawnDef))
                        break;

                    aliveCountByMonster[entry.monster]++;
                }
            }
        }

    }

    /// <summary>
    /// Hoard Management
    /// </summary>
    public partial class MonsterSpawnManager
    {
        private bool hordeActive;
        private float hordeEndTime;
        private int hordeMultiplier = 1;
        private bool hordeIgnoreCaps = true;

        [SerializeField] private float hordeSpawnRate = 0.25f;
        private float nextHordeSpawnTime;

        private void ProcessHordeTimer(float now)
        {
            if (!hordeActive)
                return;

            if (now >= hordeEndTime)
            {
                hordeActive = false;
                hordeMultiplier = 1;
                return;
            }

            if (now >= nextHordeSpawnTime)
            {
                SpawnHordeBatch();
                nextHordeSpawnTime = now + hordeSpawnRate;
            }
        }

        public void StartHorde(int multiplier, float duration, bool ignoreCap = true)
        {
            hordeActive = true;
            hordeMultiplier = Mathf.Max(1, multiplier);
            hordeIgnoreCaps = ignoreCap;
            hordeEndTime = Time.time + duration;
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

            Monsters.Add(m);
            BeastaryManager.Instance.BufferMonster(monsterSO);

            if (m.TryGetComponent(out Health health))
                health.onDie += OnMonsterDie;

            return m;
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
    }

    /// <summary>
    /// Monster Death
    /// </summary>
    public partial class MonsterSpawnManager
    {
        private void OnMonsterDie(Health health)
        {
            if (!health.TryGetComponent(out Monster monster))
                return;

            HandleStationaryDeath(monster);

            aliveCountByMonster[monster.MonsterSO]--;
            Monsters.Remove(monster);

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

    }

    /// <summary>
    /// Spawn Core
    /// </summary>
    public partial class MonsterSpawnManager
    {
        private void SpawnMonster(MonsterSO monsterSO, Vector2 spawnPos)
        {
            Monster m = Instantiate(monsterSO.prefab, (Vector3)spawnPos, Quaternion.identity, transform);
            m.Init(monsterSO);

            Monsters.Add(m);
            BeastaryManager.Instance.BufferMonster(monsterSO);

            if (m.TryGetComponent(out Health health))
                health.onDie += OnMonsterDie;
        }

        private bool TrySpawnOne(MonsterSpawn spawn)
        {
            var points = weightMap.WeightAndMapPoints(spawn.range);
            if (points == null || points.Count == 0)
                return false;

            Vector3 heroPos = HeroManager.Instance.ActiveHero != null
                ? HeroManager.Instance.ActiveHero.transform.position
                : Vector3.zero;

            Vector2Int hero = new Vector2Int(Mathf.FloorToInt(heroPos.x), Mathf.FloorToInt(heroPos.y));

            return spawn.monster.movementType == MonsterMovementType.Direct
                ? TrySpawnDirect(spawn, points, hero)
                : TrySpawnStationary(spawn, points, hero);
        }


        private void MarkOccupied(Vector2Int origin, Vector2 size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                    occupiedSpots.Add(origin + new Vector2Int(x, y));
            }
        }

        private bool IsWithinSpawnDistance(MonsterSpawn spawn, Vector2Int point, Vector2Int hero)
        {
            float dist = Vector2.Distance(point, hero);
            return dist >= spawn.minSpawnDistance && dist <= spawn.maxSpawnDistance;
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

        private void BeginSpawning(BossMonsterSpawn? boss)
        {
            DestroyExistingMonsters();

            aliveCountByMonster.Clear();
            nextSpawnTimeByMonster.Clear();

            foreach (var spawn in monsterSpawnSettings.rangeAndMonster)
                EnsureMonsterTrackingExists(spawn.monster);

            if (boss.HasValue)
                SpawnBoss(boss.Value);

            PreSpawnStationaryMonsters();

            levelStartTime = Time.time;
            nextWaveIndex = 0;
            isSpawning = true;
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

        private void ResetState()
        {
            isSpawning = false;

            occupiedSpots.Clear();
            Monsters.DestroyAndClearList();

            aliveCountByMonster.Clear();
            nextSpawnTimeByMonster.Clear();

            nextWaveIndex = 0;
            hordeActive = false;
        }

        private void DestroyExistingMonsters()
        {
            foreach (var m in Monsters)
            {
                if (m == null) continue;

#if UNITY_EDITOR
                if (Application.isPlaying)
                    Destroy(m.gameObject);
                else
                    DestroyImmediate(m.gameObject);
#else
                Destroy(m.gameObject);
#endif
            }

            Monsters.Clear();
            occupiedSpots.Clear();
        }

        

    }
}