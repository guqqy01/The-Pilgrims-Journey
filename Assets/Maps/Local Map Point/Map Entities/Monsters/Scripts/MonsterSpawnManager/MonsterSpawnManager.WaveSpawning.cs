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


        private void ProcessWaveTriggers(float difficulty)
        {
            foreach(var waveSpawn in waveSpawns)
            {
                BeginSpawningWave(waveSpawn.Item1, waveSpawn.Item2, difficulty);
            }
        }

        public void BeginSpawningWave(WaveMonsterSpawn wave, int multiplier, float difficulty)
        {
            StartCoroutine(SpawnWave(wave, multiplier, difficulty));
        }

        private IEnumerator SpawnWave(WaveMonsterSpawn wave, int multiplier, float difficulty)
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


    }
}