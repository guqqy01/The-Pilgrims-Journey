using UnityEngine;


namespace Monsters
{
    /// <summary>
    /// Boss Spawning
    /// </summary>
    public partial class MonsterSpawnManager
    {
        private void SpawnBoss(BossMonsterSpawn boss)
        {
            int index = 0;
            int limit = Map.MapSizeX * Map.MapSizeY;

            Vector2Int p = weightMap[boss.range, limit - 1];
            while (SpotOccupied(p, boss.boss.size, out Vector2 spawnPos) && limit-- > 0)
            {
                index++;
                p = weightMap[boss.range, Map.MapSizeX * Map.MapSizeY - 1 - index];
            }

            if (limit <= 0)
            {
                Debug.LogWarning("Could not find boss spawn position.");
                return;
            }

            SpawnMonster(boss.boss, p);
            MarkOccupied(p, boss.boss.size);
        }

    }

}