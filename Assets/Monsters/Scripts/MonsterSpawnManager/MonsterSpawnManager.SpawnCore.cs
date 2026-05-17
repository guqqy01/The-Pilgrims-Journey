using Heros;
using LocalMap;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Monsters
{
    /// <summary>
    /// Spawn Core
    /// </summary>
    public partial class MonsterSpawnManager
    {
        private EnemySpawnWeightMap weightMap;

        public Monster SpawnMonster(MonsterSO monsterSO, Vector2 spawnPos)
        {
            if (monsterSO == null || monsterSO.prefab == null)
                return null;

            EnsureMonsterTrackingExists(monsterSO);

            Monster m = Instantiate(monsterSO.prefab, (Vector3)spawnPos, Quaternion.identity, transform);
            m.Init(monsterSO);

            Monsters.Add(m);
            BeastaryManager.Instance.BufferMonster(monsterSO);

            if (m.TryGetComponent(out Health health))
                health.onDie += OnMonsterDie;

                aliveCountByMonster[monsterSO] =
                    aliveCountByMonster.GetValueOrDefault(monsterSO) + 1;

            return m;
        }

        private void EnsureMonsterTrackingExists(MonsterSO monster)
        {
            if (!aliveCountByMonster.ContainsKey(monster))
                aliveCountByMonster[monster] = 0;

            if (!nextSpawnTimeByMonster.ContainsKey(monster))
            {
                var spawn = monsterSpawnSettings.rangeAndMonster
                    .Find(s => s.monster == monster);

                float delay = 0f;

                if(spawn.monster != null)
                {
                    delay = UnityEngine.Random.Range(
                    0f,
                    spawn.spawnRateVSLevel.Evaluate(GameManager.Instance.Level)
                );
                }

                nextSpawnTimeByMonster[monster] = Time.time + delay;
            }
        }

        private bool TrySpawnOne(MonsterSpawn spawn, Direction? biomeDirection = null)
        {
            if(spawn is StationaryMonsterSpawn stationarySpawn)
                return TrySpawnOneStationary(stationarySpawn, null, biomeDirection);
            else if (spawn is DirectMonsterSpawn directSpawn)
                return TrySpawnOneDirect(directSpawn, biomeDirection);
            else return false;
        }

        private bool TrySpawnOneStationary(StationaryMonsterSpawn stationarySpawn, List<KeyValuePair<int, List<Vector2Int>>> weightedPoints, Direction? biomeDirection = null)
        {
            var weightedStationaryPoints = weightMap.WeightAndMapPointsConnectedToPath(stationarySpawn.range);
            if (weightedStationaryPoints == null || weightedStationaryPoints.Count == 0)
                return false;

            // Filter points to the biome's dominant zone
            if (biomeDirection.HasValue)
            {
                weightedStationaryPoints = FilterPointsToBiomeZone(weightedStationaryPoints, biomeDirection.Value);
                if (weightedStationaryPoints.Count == 0)
                    return false;
            }
            else
            {
                // Base biome — only spawn where base is dominant
                weightedStationaryPoints = FilterPointsToBaseDominantZone(weightedStationaryPoints);
                if (weightedStationaryPoints.Count == 0)
                    return false;
            }

            return TrySpawnStationary(stationarySpawn, weightedStationaryPoints);
        }

        private bool TrySpawnOneDirect(DirectMonsterSpawn directSpawn, Direction? biomeDirection = null)
        {
            Vector3 heroPos = HeroManager.Instance.ActiveHero != null
                ? HeroManager.Instance.ActiveHero.transform.position
                : Vector3.zero;

            Vector2Int heroPosOnGrid = new Vector2Int(
                Mathf.FloorToInt(heroPos.x),
                Mathf.FloorToInt(heroPos.y));

            List<Vector2Int> points = weightMap.GetAllPointsConnectedToPath();

            if (biomeDirection.HasValue)
            {
                points = FilterPointsToBiomeZone(points, biomeDirection.Value);
                if (points.Count == 0)
                    return false;
            }
            else
            {
                // Base biome — only spawn where base is dominant
                points = FilterPointsToBaseDominantZone(points);
                if (points.Count == 0)
                    return false;
            }

            return TrySpawnDirect(directSpawn, points, heroPosOnGrid);
        }

        private List<KeyValuePair<int, List<Vector2Int>>> FilterPointsToBiomeZone(
            List<KeyValuePair<int, List<Vector2Int>>> points,
            Direction blendDirection)
        {
            var result = new List<KeyValuePair<int, List<Vector2Int>>>();
            foreach (var (weight, positions) in points)
            {
                var filtered = positions.Where(p =>
                    LocalMapGenerator.Instance.GetDominantBlendDirection(p) == blendDirection).ToList();
                if (filtered.Count > 0)
                    result.Add(new KeyValuePair<int, List<Vector2Int>>(weight, filtered));
            }
            return result;
        }

        private List<KeyValuePair<int, List<Vector2Int>>> FilterPointsToBaseDominantZone(
            List<KeyValuePair<int, List<Vector2Int>>> points)
        {
            var result = new List<KeyValuePair<int, List<Vector2Int>>>();
            foreach (var (weight, positions) in points)
            {
                var filtered = positions.Where(p =>
                    LocalMapGenerator.Instance.GetDominantBlendDirection(p) == null).ToList();
                if (filtered.Count > 0)
                    result.Add(new KeyValuePair<int, List<Vector2Int>>(weight, filtered));
            }
            return result;
        }

        private List<Vector2Int> FilterPointsToBiomeZone(
            List<Vector2Int> points,
            Direction blendDirection)
        {
             return points.Where(point => LocalMapGenerator.Instance.GetDominantBlendDirection(point) == blendDirection).ToList();
        }

        private List<Vector2Int> FilterPointsToBaseDominantZone(
            List<Vector2Int> points)
        {
                return points.Where(point => LocalMapGenerator.Instance.GetDominantBlendDirection(point) == null).ToList();
        }

        private void MarkOccupied(Vector2Int origin, Vector2 size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                    occupiedSpots.Add(origin + new Vector2Int(x, y));
            }
        }

        private bool IsWithinSpawnDistance(DirectMonsterSpawn spawn, Vector2Int point, Vector2Int hero)
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
            {
                currentBossSpawn = boss.Value;
                SpawnBoss(boss.Value);
                HasBossMonsterBeenKilled = false;
            }
            else
            {
                currentBossSpawn = null;
                HasBossMonsterBeenKilled = null;
            }

            //PreSpawnStationaryMonsters();

            levelStartTime = Time.time;
            nextWaveIndex = 0;
            isSpawning = true;
        }

        



    }
}
