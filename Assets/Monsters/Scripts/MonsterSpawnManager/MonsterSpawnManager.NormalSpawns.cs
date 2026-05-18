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
            foreach (var (settings, blendDirection) in _allActiveSpawnSettings)
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