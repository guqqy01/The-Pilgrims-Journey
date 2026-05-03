using UnityEngine;
using System;
using Random = UnityEngine.Random;
namespace LocalMap
{
    [Serializable]
    public class DynamicMapObject
    {
        public string name;

        public DynamicMapObjectSpawnMethod dynamicMapObjectSpawnMethod;

        public float spawnRate;

        public float spawnRateRandomVariationMultiplier;

        public GameObject prefab;

        public float GetNextSpawnTime()
        {
            return Time.time + 1f / (spawnRate * (1f + Random.Range(-spawnRateRandomVariationMultiplier, spawnRateRandomVariationMultiplier)));
        }

        public virtual Vector2 GetPosition()
        {
            return Vector2.zero;
        }
    }
}


