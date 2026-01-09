using LocalMap;
using Monsters;
using System.Collections.Generic;
using UnityEngine;

namespace Monsters
{

    /// <summary>
    /// State
    /// </summary>
    public partial class MonsterSpawnManager
    {
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

        
    }

}