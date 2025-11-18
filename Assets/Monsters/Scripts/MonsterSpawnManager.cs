using GameManagerCalls;
using GlobalMap;
using LocalMap;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Monsters
{
    public class MonsterSpawnManager : MonoBehaviour, IOnPlayLocalLevel, IOnAfterCreateLocalMap
    {
        [SerializeField]
        private MonsterSpawnSettings monsterSpawnSettings;

        private WeightMap weightMap;

        [SerializeField, HideInInspector]
        private List<Monster> monsters = new List<Monster>();

        LocalMapGenerator mapGenerator => LocalMapGenerator.Instance;

        private List<Vector2Int> occupiedSpots = new List<Vector2Int>();

        private bool spawningMonsters = false;

        // Tracks how many of each MonsterSO are currently alive
        Dictionary<MonsterSO, int> monstersSpawned = new Dictionary<MonsterSO, int>();

        // Tracks next allowed spawn time per MonsterSO
        Dictionary<MonsterSO, float> monsterAndNextSpawntime = new Dictionary<MonsterSO, float>();

        private void Awake()
        {
            // defensive init
            if (monsterSpawnSettings == null)
                Debug.LogWarning("MonsterSpawnSettings not assigned on MonsterSpawnManager.");
        }

        public void AfterCreateLocalMap()
        {
            spawningMonsters = false;
            CreateEnemyWeightMap();
        }

        public void OnPlayLocalLevel()
        {
            Random.InitState(GlobalMapGenerator.Instance.CurrentMapPoint.Seed);

            BossMonsterSpawn? bossSpawn = null;

            if (GlobalMapGenerator.Instance.CurrentMapPoint.Type == GlobalMapPointType.Relic)
                bossSpawn = monsterSpawnSettings.bossMonsters.GetRandom();

            BeginSpawnEnemys(bossSpawn);
        }

        private void Update()
        {
            if (!spawningMonsters)
                return;

            float now = Time.time;

            // For each monster spawn config, check timers and spawn if allowed
            foreach (var monsterSpawn in monsterSpawnSettings.rangeAndMonster)
            {
                MonsterSO monsterSO = monsterSpawn.monster;

                // ensure dictionary entries exist
                if (!monstersSpawned.ContainsKey(monsterSO))
                    monstersSpawned[monsterSO] = 0;

                if (!monsterAndNextSpawntime.ContainsKey(monsterSO))
                    monsterAndNextSpawntime[monsterSO] = now + Random.Range(0f, monsterSpawn.spawnRateVSLevel.Evaluate(GameManager.Instance.Level));

                // How many allowed alive
                int maxAllowed = Mathf.FloorToInt(monsterSpawn.maxAmountVSLevel.Evaluate(GameManager.Instance.Level));

                // If we already have >= max allowed, skip
                if (monstersSpawned[monsterSO] >= maxAllowed)
                    continue;

                // If not yet time to spawn, skip
                if (now < monsterAndNextSpawntime[monsterSO])
                    continue;

                // It's time to spawn one of this type.
                bool spawned = TrySpawnOne(monsterSpawn);

                // Set next spawn time regardless of success to avoid tight loops.
                float baseRate = Mathf.Max(0.01f, monsterSpawn.spawnRateVSLevel.Evaluate(GameManager.Instance.Level));
                // Add a bit of jitter so spawns aren't perfectly rhythmic
                float jitter = Random.Range(0.75f, 1.25f);
                monsterAndNextSpawntime[monsterSO] = now + baseRate * jitter;

                // If we spawned, increment count
                if (spawned)
                {
                    monstersSpawned[monsterSO]++;
                }
            }
        }

        private bool TrySpawnOne(MonsterSpawn spawn)
        {
            // Use weight map to find spawn point
            var weightsAndPoints = weightMap.WeightAndMapPoints(spawn.range);
            if (weightsAndPoints == null || weightsAndPoints.Count == 0)
                return false;

            // Iterate weight groups from highest to lowest (WeightAndMapPoints already orders descending).
            // For each weight group, pick randomly from its points until one is not occupied.
            foreach (var kv in weightsAndPoints)
            {
                int weight = kv.Key;
                List<Vector2Int> points = kv.Value;

                if (weight <= 0 || points == null || points.Count == 0)
                    continue;

                // Make a local copy so we can remove tried points without affecting iteration
                var remaining = new List<Vector2Int>(points);

                // Try each candidate in a random order
                while (remaining.Count > 0)
                {
                    int idx = Random.Range(0, remaining.Count);
                    Vector2Int candidate = remaining[idx];
                    remaining.RemoveAt(idx);

                    Vector2 spawnPosition;
                    if (SpotOccupied(candidate, spawn.monster.size, out spawnPosition))
                    {
                        // occupied - try next
                        continue;
                    }

                    // Found an available spot — instantiate
                    Monster monster = Instantiate(spawn.monster.prefab, new Vector3(spawnPosition.x, spawnPosition.y), Quaternion.identity, transform);
                    monster.Init(spawn.monster);

                    // Hook death event
                    if (monster.TryGetComponent(out Health health))
                    {
                        health.onDie += OnMonsterDie;
                    }

                    monsters.Add(monster);
                    BeastaryManager.Instance.BufferMonster(spawn.monster);

                    // Occupy cells if this is a stationary monster or otherwise sized occupant.
                    // Behavior matches your existing code: for multi-cell sized monsters, add all positions.
                    Vector2Int basePoint = candidate;
                    for (int i = 0; i < spawn.monster.size.x; i++)
                    {
                        for (int j = 0; j < spawn.monster.size.y; j++)
                        {
                            occupiedSpots.Add(basePoint + new Vector2Int(i, j));
                        }
                    }

                    // If monster movement is stationary we might want to set its StationaryPosition if MonsterMovement expects it.
                    // In your original code you just instantiate at the integer coordinate and MonsterMovement probably stores it itself.
                    // If needed, set it here:
                    if (monster.TryGetComponent(out MonsterMovement movement) && spawn.monster.movementType == MonsterMovementType.Stationary)
                    {
                        movement.StationaryPosition = candidate;
                    }

                    return true;
                }

                // If we've exhausted all points for this weight, continue to next-lower weight
            }

            // None of the weight groups yielded a free point
            return false;
        }

        private void CreateEnemyWeightMap()
        {
            int maxRange = 0;

            for (int i = 0; i < monsterSpawnSettings.rangeAndMonster.Count; i++)
            {
                if (monsterSpawnSettings.rangeAndMonster[i].range > maxRange)
                    maxRange = monsterSpawnSettings.rangeAndMonster[i].range;
            }

            foreach (BossMonsterSpawn bossSpawn in monsterSpawnSettings.bossMonsters)
            {
                if (bossSpawn.range > maxRange)
                    maxRange = bossSpawn.range;
            }

            weightMap = new WeightMap(maxRange, mapGenerator.MapSizeX, mapGenerator.MapSizeY);

            for (int i = 0; i < mapGenerator.MapSizeX; i++)
            {
                for (int j = 0; j < mapGenerator.MapSizeY; j++)
                {

                    for (int k = 1; k <= maxRange; k++)
                    {
                        weightMap[k, i, j] = 0;
                    }

                    if (mapGenerator.pathway.ContainsStep(i, j))
                        continue;

                    for (int x = -maxRange; x <= maxRange; x++)
                    {
                        for (int y = -maxRange; y <= maxRange; y++)
                        {
                            if (mapGenerator.pathway.ContainsStep(i + x, j + y))
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

        private void DestoryEnemys()
        {
            for (int i = 0; i < monsters.Count; i++)
            {
                if (Application.isPlaying)
                {
                    if (monsters[i] != null)
                        Destroy(monsters[i].gameObject);
                }
                else
                {
                    if (monsters[i] != null)
                        DestroyImmediate(monsters[i].gameObject);
                }
            }

            monsters.Clear();
            occupiedSpots.Clear();
            monstersSpawned.Clear();
            monsterAndNextSpawntime.Clear();
        }

        public bool SpotOccupied(Vector2Int point, Vector2 size, out Vector2 spawnPosition)
        {
            spawnPosition = point;

            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    if (occupiedSpots.Contains(point + new Vector2Int(i, j)) || mapGenerator.pathway.ContainsStep(point + new Vector2Int(i, j)))
                        return true;

                    // bounds check - note: mapGenerator.MapSizeX/Y are sizes; valid indices are 0..MapSizeX-1 etc.
                    if (point.x + i < 0 || point.y + j < 0 || point.x + i >= mapGenerator.MapSizeX || point.y + j >= mapGenerator.MapSizeY)
                        return true;
                }
            }

            spawnPosition = (Vector2)point + ((Vector2)size / 2f);

            return false;
        }

        private void SpawnBoss(BossMonsterSpawn bossMonsterSpawn)
        {
            int spawnIndex = 0;

            Vector2Int point = weightMap[bossMonsterSpawn.range, mapGenerator.MapSizeX * mapGenerator.MapSizeY - 1 - spawnIndex];

            int breaker = mapGenerator.MapSizeX * mapGenerator.MapSizeY;

            Vector2 spawnPosition;

            while (SpotOccupied(point, bossMonsterSpawn.boss.size, out spawnPosition) && breaker > 0)
            {
                spawnIndex++;
                point = weightMap[bossMonsterSpawn.range, mapGenerator.MapSizeX * mapGenerator.MapSizeY - 1 - spawnIndex];
                breaker--;
            }

            if (breaker <= 0)
            {
                Debug.LogWarning("Couldn't find spawn spot for boss.");
                return;
            }

            Monster monster = Instantiate(bossMonsterSpawn.boss.prefab, spawnPosition, Quaternion.identity, transform);
            monster.Init(bossMonsterSpawn.boss);
            monsters.Add(monster);
            BeastaryManager.Instance.BufferMonster(bossMonsterSpawn.boss);

            for (int i = 0; i < bossMonsterSpawn.boss.size.x; i++)
            {
                for (int j = 0; j < bossMonsterSpawn.boss.size.y; j++)
                {
                    occupiedSpots.Add(point + new Vector2Int(i, j));
                }
            }

            // count boss in monstersSpawned dictionary so it contributes to limits (optional)
            if (!monstersSpawned.ContainsKey(bossMonsterSpawn.boss))
                monstersSpawned[bossMonsterSpawn.boss] = 0;
            monstersSpawned[bossMonsterSpawn.boss]++;
        }

        private void BeginSpawnEnemys(BossMonsterSpawn? boss)
        {
            DestoryEnemys();

            // initialize counters + timers for each monster type
            monstersSpawned.Clear();
            monsterAndNextSpawntime.Clear();

            // initialize per-monster dictionaries
            foreach (var spawn in monsterSpawnSettings.rangeAndMonster)
            {
                if (!monstersSpawned.ContainsKey(spawn.monster))
                    monstersSpawned[spawn.monster] = 0;

                float initialDelay = Random.Range(0f, Mathf.Max(0.01f, spawn.spawnRateVSLevel.Evaluate(GameManager.Instance.Level)));
                monsterAndNextSpawntime[spawn.monster] = Time.time + initialDelay;
            }

            if (boss.HasValue)
                SpawnBoss(boss.Value);

            // Spawn all stationary monsters up-front (keeps your original behavior)
            foreach (MonsterSpawn spawn in monsterSpawnSettings.rangeAndMonster.Where(x => x.monster.movementType == MonsterMovementType.Stationary))
            {
                int initialAmount = Mathf.FloorToInt(spawn.maxAmountVSLevel.Evaluate(GameManager.Instance.Level));

                for (int i = 0; i < initialAmount; i++)
                {
                    int spawnIndex = i;

                    Vector2Int point = weightMap[spawn.range, mapGenerator.MapSizeX * mapGenerator.MapSizeY - 1 - spawnIndex];

                    int breaker = mapGenerator.MapSizeX * mapGenerator.MapSizeY;

                    Vector2 spawnPosition;

                    while (SpotOccupied(point, spawn.monster.size, out spawnPosition) && breaker > 0)
                    {
                        spawnIndex++;
                        point = weightMap[spawn.range, mapGenerator.MapSizeX * mapGenerator.MapSizeY - 1 - spawnIndex];
                        breaker--;
                    }

                    if (breaker <= 0)
                        break; // couldn't find more spots

                    Monster monster = Instantiate(spawn.monster.prefab, new Vector3(point.x, point.y), Quaternion.identity, transform);
                    monster.Init(spawn.monster);

                    if (monster.TryGetComponent(out Health health))
                    {
                        health.onDie += OnMonsterDie;
                    }

                    monsters.Add(monster);
                    BeastaryManager.Instance.BufferMonster(spawn.monster);

                    // occupy spots for sized monster
                    for (int x = 0; x < spawn.monster.size.x; x++)
                    {
                        for (int j = 0; j < spawn.monster.size.y; j++)
                        {
                            occupiedSpots.Add(point + new Vector2Int(x, j));
                        }
                    }

                    // increment spawn counters
                    monstersSpawned[spawn.monster] = monstersSpawned.GetValueOrDefault(spawn.monster) + 1;
                }
            }

            spawningMonsters = true;
        }

        private void OnMonsterDie(Health health)
        {
            if (health.TryGetComponent(out Monster monster))
            {
                if (monster.TryGetComponent(out MonsterMovement movement))
                {
                    // if the monster had a stationary position, free those spots
                    Vector2Int pos = movement.StationaryPosition;
                    if (pos != default)
                    {
                        for (int i = 0; i < monster.MonsterSO.size.x; i++)
                        {
                            for (int j = 0; j < monster.MonsterSO.size.y; j++)
                            {
                                occupiedSpots.Remove(pos + new Vector2Int(i, j));
                            }
                        }
                    }
                }

                NotifyMonsterDied(monster.MonsterSO);

                monsters.Remove(monster);
            }

            health.onDie -= OnMonsterDie;
        }

        public void NotifyMonsterDied(MonsterSO monster)
        {
            // Decrement count
            if (!monstersSpawned.ContainsKey(monster))
                return;

            int previousCount = monstersSpawned[monster];
            monstersSpawned[monster]--;

            // Only trigger a delay if we *were* at the limit
            int maxAllowed = Mathf.FloorToInt(
                monsterSpawnSettings.rangeAndMonster
                .Find(m => m.monster == monster)
                .maxAmountVSLevel
                .Evaluate(GameManager.Instance.Level)
            );

            bool wasAtMax = previousCount >= maxAllowed;

            if (wasAtMax)
            {
                float now = Time.time;
                var spawnData = monsterSpawnSettings.rangeAndMonster
                    .Find(m => m.monster == monster);

                float baseRate = Mathf.Max(
                    0.01f,
                    spawnData.spawnRateVSLevel.Evaluate(GameManager.Instance.Level)
                );

                // jitter to prevent patterns
                float jitter = Random.Range(0.75f, 1.25f);

                // delay next spawn
                monsterAndNextSpawntime[monster] = now + baseRate * jitter;
            }
        }
    }

    // convenience extension used in above code (if you don't have it)
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            if (dict.TryGetValue(key, out TValue v))
                return v;
            return default;
        }
    }
}
