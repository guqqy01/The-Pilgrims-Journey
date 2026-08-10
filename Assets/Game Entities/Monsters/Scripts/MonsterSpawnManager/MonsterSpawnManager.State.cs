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

        private readonly System.Random seededRandom = new();

        // current boss spawn definition for this level (if any)
        private BossMonsterSpawn? currentBossSpawn;

        private void ResetState()
        {
            isSpawning = false;

            occupiedSpots.Clear();

            DestroyExistingMonsters();

            Monsters.DestroyAndClearList();

            aliveCountByMonster.Clear();
            nextSpawnTimeByMonster.Clear();

            hordeActive = false;

            currentBossSpawn = null;
        }
    }

}