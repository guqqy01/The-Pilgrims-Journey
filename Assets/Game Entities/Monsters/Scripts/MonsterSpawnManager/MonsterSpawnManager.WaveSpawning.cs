using System.Collections;
using UnityEngine;

namespace Monsters
{
    /// <summary>
    /// Wave Spawning
    /// </summary>
    public partial class MonsterSpawnManager
    {
        [SerializeField] private float spawnRetryDelay = 0.5f;
        /// <summary>
        /// Amount to spawn within one second
        /// </summary>
        [SerializeField] private float waveSpawnRate = 60f;

        // ProcessWaveTriggers is gone: the SpawnDirector now schedules waves.

        public void BeginSpawningWave(WaveMonsterSpawn wave, int multiplier, float difficulty)
        {
            StartCoroutine(SpawnWave(wave, multiplier, difficulty));
        }

        private IEnumerator SpawnWave(WaveMonsterSpawn wave, int multiplier, float difficulty)
        {
            int currentCount = 0;
            float startTime = Time.time;
            int totalToSpawn = Mathf.CeilToInt(wave.Count * multiplier);

            while (currentCount < totalToSpawn && isSpawning)
            {
                int amountShouldHaveSpawnedByNow = Mathf.CeilToInt((Time.time - startTime) * waveSpawnRate);
                int amountToSpawnThisIteration = amountShouldHaveSpawnedByNow - currentCount;

                for (int i = 0; i < amountToSpawnThisIteration; i++)
                {
                    if (currentCount >= totalToSpawn)
                        break;

                    if (TrySpawnOne(wave, difficulty))
                    {
                        currentCount++;
                    }
                    else
                    {
                        yield return new WaitForSeconds(spawnRetryDelay);
                    }
                }

                yield return null;
            }
        }

        // -----------------------------------------------------------------
        //  Quest waves: a one-off swarm of one monster type, not tied to a MonsterSpawn asset.
        // -----------------------------------------------------------------

        public void SpawnQuestWave(MonsterSO monster, int count, float spawnSeconds, float minDistance, float maxDistance, float difficulty)
        {
            StartCoroutine(SpawnQuestWaveRoutine(
                new AdHocDirectSpawn(monster, minDistance, maxDistance), count, spawnSeconds, difficulty));
        }

        private IEnumerator SpawnQuestWaveRoutine(IDirectMonsterSpawn spawn, int count, float spawnSeconds, float difficulty)
        {
            float startTime = Time.time;
            float rate = count / Mathf.Max(0.1f, spawnSeconds);
            int spawned = 0;

            while (spawned < count && isSpawning)
            {
                int due = Mathf.Min(count, Mathf.CeilToInt((Time.time - startTime) * rate));

                while (spawned < due)
                {
                    if (!TrySpawnOneDirect(spawn, difficulty))
                    {
                        yield return new WaitForSeconds(spawnRetryDelay);
                        break;
                    }

                    spawned++;
                }

                yield return null;
            }
        }

        private sealed class AdHocDirectSpawn : IDirectMonsterSpawn
        {
            public MonsterSO MonsterSO { get; }
            public float MinSpawnDistance { get; }
            public float MaxSpawnDistance { get; }

            public AdHocDirectSpawn(MonsterSO monster, float minDistance, float maxDistance)
            {
                MonsterSO = monster;
                MinSpawnDistance = minDistance;
                MaxSpawnDistance = maxDistance;
            }
        }
    }
}