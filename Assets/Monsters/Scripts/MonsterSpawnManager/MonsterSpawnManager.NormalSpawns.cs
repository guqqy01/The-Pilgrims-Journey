using Heros;
using LocalMap;
using System.Collections.Generic;
using System.Linq;
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
            foreach (var spawn in monsterSpawnSettings.rangeAndMonster)
            {
                var monster = spawn.monster;

                EnsureMonsterTrackingExists(monster);

                if (!CanSpawnMore(monster, spawn))
                    continue;

                if (now < nextSpawnTimeByMonster[monster])
                    continue;

                TrySpawnOne(spawn);

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


}