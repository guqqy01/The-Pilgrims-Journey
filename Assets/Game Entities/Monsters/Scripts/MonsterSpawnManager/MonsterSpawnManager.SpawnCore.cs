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

        public Monster SpawnMonster(MonsterSO monsterSO, Vector2 spawnPos, float difficulty)
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
                var spawn = monsterSpawnSettings.normalMonsterSpawns
                    .Find(s => s.monster == monster);

                float delay = 0f;

                if (spawn == null)
                    return;

                if(spawn.monster != null)
                {
                    delay = Random.Range(0f, spawn.SpawnRate);
                }

                nextSpawnTimeByMonster[monster] = Time.time + delay;
            }
        }

        private bool TrySpawnOne(MonsterSpawn spawn, float difficulty)
        {
            if(spawn is IStationaryMonsterSpawn stationarySpawn)
                return TrySpawnOneStationary(stationarySpawn, null, difficulty);
            else if (spawn is IDirectMonsterSpawn directSpawn)
                return TrySpawnOneDirect(directSpawn, difficulty);
            else return false;
        }

        private bool TrySpawnOneStationary(IStationaryMonsterSpawn stationarySpawn, List<KeyValuePair<int, List<Vector2Int>>> weightedPoints, float difficulty)
        {
            var weightedStationaryPoints = weightMap.WeightAndMapPointsConnectedToPath(stationarySpawn.Range);
            if (weightedStationaryPoints == null || weightedStationaryPoints.Count == 0)
                return false;

            weightedStationaryPoints = FilterPointsToBaseDominantZone(weightedStationaryPoints);
            if (weightedStationaryPoints.Count == 0)
                return false;

            return TrySpawnStationary(stationarySpawn, weightedStationaryPoints, difficulty);
        }

        private bool TrySpawnOneDirect(IDirectMonsterSpawn directSpawn, float difficulty)
        {
            Vector3 heroPos = HeroManager.Instance.ActiveHero != null
                ? HeroManager.Instance.ActiveHero.transform.position
                : Vector3.zero;

            Vector2Int heroPosOnGrid = new Vector2Int(
                Mathf.FloorToInt(heroPos.x),
                Mathf.FloorToInt(heroPos.y));

            List<Vector2Int> points = weightMap.GetAllPointsConnectedToPath();

            // Base biome — only spawn where base is dominant
            points = FilterPointsToBaseDominantZone(points);
            if (points.Count == 0)
                return false;

            return TrySpawnDirect(directSpawn, points, heroPosOnGrid, difficulty);
        }

        private List<KeyValuePair<int, List<Vector2Int>>> FilterPointsToBaseDominantZone(List<KeyValuePair<int, List<Vector2Int>>> points)
        {
            var result = new List<KeyValuePair<int, List<Vector2Int>>>();
            foreach (var (weight, positions) in points)
            {
                var filtered = positions.Where(p => LocalMapGenerator.Instance.GetDominantBlendDirection(p) == null).ToList();
                if (filtered.Count > 0)
                    result.Add(new KeyValuePair<int, List<Vector2Int>>(weight, filtered));
            }
            return result;
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

        private bool IsWithinSpawnDistance(IDirectMonsterSpawn spawn, Vector2Int point, Vector2Int hero)
        {
            float dist = Vector2.Distance(point, hero);
            return dist >= spawn.MinSpawnDistance && dist <= spawn.MaxSpawnDistance;
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
                        Map.Pathway.ContainsStep(cell))
                        return true;

                    if (cell.x < 0 || cell.y < 0 ||
                        cell.x >= Map.MapSizeX || cell.y >= Map.MapSizeY)
                        return true;
                }
            }

            spawnPosition = (Vector2)point + size / 2f;
            return false;
        }

        private void BeginSpawning(BossMonsterSpawn? boss, float difficulty)
        {
            DestroyExistingMonsters();

            aliveCountByMonster.Clear();
            nextSpawnTimeByMonster.Clear();

            foreach (var spawn in monsterSpawnSettings.MonsterSpawns)
                EnsureMonsterTrackingExists(spawn.monster);

            if (boss.HasValue)
            {
                currentBossSpawn = boss.Value;
                SpawnBoss(boss.Value, difficulty);
                HasBossMonsterBeenKilled = false;
            }
            else
            {
                currentBossSpawn = null;
                HasBossMonsterBeenKilled = null;
            }

            //PreSpawnStationaryMonsters();

            levelStartTime = Time.time;
            isSpawning = true;
        }

        



    }
}
