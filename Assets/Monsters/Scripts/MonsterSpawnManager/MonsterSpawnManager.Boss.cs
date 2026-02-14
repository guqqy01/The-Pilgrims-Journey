using UnityEngine;


namespace Monsters
{
    /// <summary>
    /// Boss Spawning
    /// </summary>
    public partial class MonsterSpawnManager
    {
        public Monster BossMonster { get; private set; }

        private void SpawnBoss(BossMonsterSpawn bossSpawn)
        {
            int index = 0;
            int limit = Map.MapSizeX * Map.MapSizeY;

            Vector2Int p = weightMap[bossSpawn.range, limit - 1];
            while (SpotOccupied(p, bossSpawn.boss.size, out Vector2 spawnPos) && limit-- > 0)
            {
                index++;
                p = weightMap[bossSpawn.range, Map.MapSizeX * Map.MapSizeY - 1 - index];
            }

            if (limit <= 0)
            {
                Debug.LogWarning("Could not find boss spawn position.");
                return;
            }

            Monster bossMonster = SpawnMonster(bossSpawn.boss, p);
            BossMonster = bossMonster;
            BossMonster.HealthAndShieldManager.Health.onDie += OnBossDie;
            MarkOccupied(p, bossSpawn.boss.size);
        }

        private void HandleSpawnBossMinions()
        {

        }

        private void OnBossDie(Health health)
        {
            BossMonster.HealthAndShieldManager.Health.onDie -= OnBossDie;
            BossMonster = null;
        }
    }

}