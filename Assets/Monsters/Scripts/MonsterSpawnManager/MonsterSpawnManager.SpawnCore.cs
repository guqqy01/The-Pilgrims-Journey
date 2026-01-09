using Heros;
using LocalMap;
using System;
using System.Linq;
using UnityEngine;

namespace Monsters
{


    /// <summary>
    /// Spawn Core
    /// </summary>
    public partial class MonsterSpawnManager
    {
        private WeightMap weightMap;

        private void SpawnMonster(MonsterSO monsterSO, Vector2 spawnPos)
        {
            Monster m = Instantiate(monsterSO.prefab, (Vector3)spawnPos, Quaternion.identity, transform);
            m.Init(monsterSO);

            Monsters.Add(m);
            BeastaryManager.Instance.BufferMonster(monsterSO);

            if (m.TryGetComponent(out Health health))
                health.onDie += OnMonsterDie;

                aliveCountByMonster[monsterSO] =
                    aliveCountByMonster.GetValueOrDefault(monsterSO) + 1;
        }

        private bool TrySpawnOne(MonsterSpawn spawn)
        {
            var points = weightMap.WeightAndMapPoints(spawn.range);
            if (points == null || points.Count == 0)
                return false;

            Vector3 heroPos = HeroManager.Instance.ActiveHero != null
                ? HeroManager.Instance.ActiveHero.transform.position
                : Vector3.zero;

            Vector2Int heroPosOnGrid = new Vector2Int(Mathf.FloorToInt(heroPos.x), Mathf.FloorToInt(heroPos.y));

            bool success = spawn.monster.movementType == MonsterMovementType.Direct
                ? TrySpawnDirect(spawn, points, heroPosOnGrid)
                : TrySpawnStationary(spawn, points);

            

            return success;
        }

        private bool CanSpawnMore(MonsterSO monster, MonsterSpawn spawn)
        {
            int max = Mathf.FloorToInt(
                spawn.maxAmountVSLevel.Evaluate(GameManager.Instance.Level)
            );

            return aliveCountByMonster[monster] < max;
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
