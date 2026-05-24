using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Codice.CM.WorkspaceServer.Tree;

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


        private void ProcessWaveTriggers()
        {
            foreach(var waveSpawn in waveSpawns)
            {
                BeginSpawningWave(waveSpawn.Item1, waveSpawn.Item2);
            }
        }

        private void BeginSpawningWave(WaveMonsterSpawn wave, int multiplier)
        {
            StartCoroutine(SpawnWave(wave, multiplier));
        }

        private IEnumerator SpawnWave(WaveMonsterSpawn wave, int multiplier)
        {
            int currentCount = 0;
            float startTime = Time.time;
            int totalToSpawn = Mathf.CeilToInt(wave.Count * multiplier);

            while (currentCount < totalToSpawn)
            {
                int amountShouldHaveSpawnedByNow = Mathf.CeilToInt((Time.time - startTime) * waveSpawnRate);
                int amountToSpawnThisIteration = amountShouldHaveSpawnedByNow - currentCount;

                for (int i = 0; i < amountToSpawnThisIteration; i++)
                {
                    if (currentCount >= totalToSpawn)
                        break;

                    if (TrySpawnOne(wave))
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


    }
}