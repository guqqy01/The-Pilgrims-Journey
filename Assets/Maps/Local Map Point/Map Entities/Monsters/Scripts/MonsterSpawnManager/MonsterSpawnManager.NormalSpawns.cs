using System;
using UnityEngine;

namespace Monsters
{
    /// <summary>
    /// Normal Spawns
    /// </summary>
    public partial class MonsterSpawnManager
    {
        private void ProcessNormalSpawns(float now, float difficulty)
        {
            foreach(var (monsterSpawn, size) in normalSpawns)
            {
                if (monsterSpawn is not NormalMonsterSpawn normalMonsterSpawn)
                    continue;

                var monster = monsterSpawn.monster;
                EnsureMonsterTrackingExists(monster);

                if (now < nextSpawnTimeByMonster[monster])
                    continue;

                TrySpawnOne(normalMonsterSpawn, difficulty);
                nextSpawnTimeByMonster[monster] = now + GetNextSpawnDelay(normalMonsterSpawn, size);
            }
        }

        private float GetNextSpawnDelay(NormalMonsterSpawn spawn)
        {
            float baseRate = Mathf.Max(
                0.01f,
                spawn.SpawnRate
            );

            return baseRate * UnityEngine.Random.Range(0.75f, 1.25f);
        }

        private float GetNextSpawnDelay(NormalMonsterSpawn spawn, int tileSize)
        {
            float baseRate = Mathf.Max(
                0.01f,
                spawn.SpawnRate / tileSize
            );

            return baseRate * UnityEngine.Random.Range(0.75f, 1.25f);
        }
    }


}