using System.Collections.Generic;
using UnityEngine;

namespace Monsters
{
    /// <summary>
    /// Normal Spawns - which types are active, how fast and how many are all decided by the SpawnDirector.
    /// </summary>
    public partial class MonsterSpawnManager
    {
        private readonly List<NormalMonsterSpawn> activeNormalSpawns = new List<NormalMonsterSpawn>();
        private float spawnIntensity;
        private int maxMonstersAlive = int.MaxValue;
        private float spawnChaos = 0.25f / 0.9f; // reproduces the old +/-25% jitter if never configured

        public void ConfigureNormalSpawns(IEnumerable<NormalMonsterSpawn> spawns, int maxAlive, float chaos)
        {
            activeNormalSpawns.Clear();
            activeNormalSpawns.AddRange(spawns);
            maxMonstersAlive = maxAlive;
            spawnChaos = chaos;
        }

        /// <summary>1 = the monster's authored rate. Called every frame by the SpawnDirector.</summary>
        public void SetSpawnIntensity(float intensity) => spawnIntensity = Mathf.Max(0f, intensity);

        private void ProcessNormalSpawns(float now, float difficulty)
        {
            if (spawnIntensity <= 0f || Monsters.Count >= maxMonstersAlive)
                return;

            foreach (NormalMonsterSpawn spawn in activeNormalSpawns)
            {
                if (spawn == null || spawn.monster == null)
                    continue;

                EnsureMonsterTrackingExists(spawn.monster);

                if (!nextSpawnTimeByMonster.TryGetValue(spawn.monster, out float next) || now < next)
                    continue;

                TrySpawnOne(spawn, difficulty);
                nextSpawnTimeByMonster[spawn.monster] = now + GetNextSpawnDelay(spawn, spawnIntensity);
            }
        }

        // Used by boss minions and respawn delays (unchanged behaviour).
        private float GetNextSpawnDelay(NormalMonsterSpawn spawn)
        {
            float baseRate = Mathf.Max(0.01f, spawn.SpawnRate);
            return baseRate * UnityEngine.Random.Range(0.75f, 1.25f);
        }

        private float GetNextSpawnDelay(NormalMonsterSpawn spawn, float intensity)
        {
            float baseRate = Mathf.Max(0.01f, spawn.SpawnRate / Mathf.Max(0.01f, intensity));
            float jitter = 1f + UnityEngine.Random.Range(-1f, 1f) * Mathf.Clamp01(spawnChaos) * 0.9f;
            return baseRate * jitter;
        }
    }
}