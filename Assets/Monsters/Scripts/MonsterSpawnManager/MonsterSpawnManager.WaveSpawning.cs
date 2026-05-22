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

        private void ProcessWaveTriggers()
        {
            foreach(var waveSpawn in waveSpawns)
            {
                BeginSpawningWave(waveSpawn.Item1, waveSpawn.Item2);
            }
        }

        private void BeginSpawningWave(WaveMonsterSpawn wave, int multiplier)
        {
            StartCoroutine(SpawnWave(wave));
        }

        private IEnumerator SpawnWave(WaveMonsterSpawn wave)
        {
            int currentCount = 0;

            while (currentCount < wave.Count)
            {
                float now = Time.time;

                if (TrySpawnOne(wave))
                {
                    currentCount++;
                }
                else
                {
                    yield return new WaitForSeconds(spawnRetryDelay);
                }

                yield return null;
            }
        }


    }
}