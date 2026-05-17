using UnityEngine;


namespace Monsters
{
    /// <summary>
    /// Boss Spawning
    /// </summary>
    public partial class MonsterSpawnManager
    {
        public Monster BossMonster { get; private set; }

        public bool? HasBossMonsterBeenKilled {  get; private set; }

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

        private void HandleSpawnBossMinions(float now)
        {
            // Only run if this level has a boss spawn and boss is present
            if (!currentBossSpawn.HasValue || BossMonster == null)
                return;

            var minionSpawns = currentBossSpawn.Value.bossMinionSpawns;
            if (minionSpawns == null)
                return;

            foreach (var spawn in minionSpawns)
            {
                // safety: skip invalid entries
                if (spawn.monster == null)
                    continue;

                var monster = spawn.monster;

                EnsureMonsterTrackingExists(monster);

                if (now < nextSpawnTimeByMonster[monster])
                    continue;

                TrySpawnOne(spawn);

                nextSpawnTimeByMonster[monster] =
                    now + GetNextSpawnDelay(spawn);
            }
        }

        private void OnBossDie(Health health)
        {
            BossMonster.HealthAndShieldManager.Health.onDie -= OnBossDie;
            HasBossMonsterBeenKilled = true;
            BossMonster = null;
        }
    }

}