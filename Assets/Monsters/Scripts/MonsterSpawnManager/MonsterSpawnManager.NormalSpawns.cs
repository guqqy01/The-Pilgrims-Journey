using System;
using UnityEngine;

namespace Monsters
{
    /// <summary>
    /// Normal Spawns
    /// </summary>
    public partial class MonsterSpawnManager
    {
        private void ProcessNormalSpawns(float now)
        {
            foreach(var (monsterSpawn, size) in spawns)
            {
                var monster = monsterSpawn.monster;
                EnsureMonsterTrackingExists(monster);

                if (now < nextSpawnTimeByMonster[monster])
                    continue;

                TrySpawnOne(monsterSpawn);
                nextSpawnTimeByMonster[monster] = now + GetNextSpawnDelay(monsterSpawn, size);
            }

            /*foreach (var (settings, blendDirection) in _allActiveSpawnSettings)
            {
                foreach (var spawn in settings.monsterSpawns)
                {
                    var monster = spawn.monster;
                    EnsureMonsterTrackingExists(monster);

                    if (now < nextSpawnTimeByMonster[monster])
                        continue;

                    TrySpawnOne(spawn, blendDirection);
                    nextSpawnTimeByMonster[monster] = now + GetNextSpawnDelay(spawn);
                }
            }*/
        }

        private float GetNextSpawnDelay(MonsterSpawn spawn)
        {
            float baseRate = Mathf.Max(
                0.01f,
                spawn.spawnRateVSLevel.Evaluate(GameManager.Instance.Level)
            );

            return baseRate * UnityEngine.Random.Range(0.75f, 1.25f);
        }

        private float GetNextSpawnDelay(MonsterSpawn spawn, int tileSize)
        {
            float baseRate = Mathf.Max(
                0.01f,
                spawn.spawnRateVSLevel.Evaluate(GameManager.Instance.Level) / tileSize
            );

            return baseRate * UnityEngine.Random.Range(0.75f, 1.25f);
        }
    }


}