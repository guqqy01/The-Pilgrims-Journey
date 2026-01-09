using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Monsters
{
    /// <summary>
    /// Wave Spawning
    /// </summary>
    public partial class MonsterSpawnManager
    {
        [SerializeField] private float randomWaveSpawnDelay = 2f;
        [SerializeField] private float spawnRetryDelay = 0.5f;
        [SerializeField] private float maxSpawnRetryTime = 5f;

        private struct WaveSpawnRequest
        {
            public MonsterSpawn spawn;
            public float scheduledTime;
            public float expiryTime;
        }

        private void ProcessWaveTriggers(float now)
        {
            if (nextWaveIndex >= monsterSpawnSettings.waves.Count)
                return;

            var wave = monsterSpawnSettings.waves[nextWaveIndex];
            if (now - levelStartTime < wave.triggerTime)
                return;

            BeginSpawningWave(wave);
            nextWaveIndex++;
        }

        private void BeginSpawningWave(Wave wave)
        {
            var spawnQueue = new List<WaveSpawnRequest>();

            foreach (var entry in wave.entries)
            {
                MonsterSpawn spawnDef = monsterSpawnSettings.rangeAndMonster
                    .FirstOrDefault(s => s.monster == entry.monster);

                if (spawnDef.monster == null)
                {
                    Debug.LogWarning($"No spawn definition for monster {entry.monster.name}");
                    continue;
                }

                for (int i = 0; i < entry.count; i++)
                {
                    spawnQueue.Add(new WaveSpawnRequest
                    {
                        spawn = spawnDef,
                        scheduledTime = Time.time + Random.Range(0f, randomWaveSpawnDelay),
                        expiryTime = Time.time + maxSpawnRetryTime
                    });
                }
            }

            StartCoroutine(SpawnWave(spawnQueue));
        }



        private IEnumerator SpawnWave(List<WaveSpawnRequest> spawnQueue)
        {
            while (spawnQueue.Count > 0)
            {
                float now = Time.time;

                for (int i = spawnQueue.Count - 1; i >= 0; i--)
                {
                    var request = spawnQueue[i];

                    if (now < request.scheduledTime)
                        continue;

                    // Give up if we've retried too long
                    if (now > request.expiryTime)
                    {
                        spawnQueue.RemoveAt(i);
                        continue;
                    }

                    if (TrySpawnOne(request.spawn))
                    {
                        spawnQueue.RemoveAt(i);
                    }
                    else
                    {
                        // Retry later instead of hammering every frame
                        request.scheduledTime = now + spawnRetryDelay;
                        spawnQueue[i] = request;
                    }
                }

                yield return null;
            }
        }


    }
}