using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GlobalMap;

namespace Monsters
{
    [CreateAssetMenu()]
    public class MonsterSpawnSettings : ScriptableObject
    {
        public List<MonsterSpawn> rangeAndMonster;
        public List<BossMonsterSpawn> bossMonsters;

        public List<Wave> waves = new List<Wave>(); // define waves in inspector
    }

    [Serializable]
    public struct MonsterSpawn
    {
        public int range;
        public AnimationCurve maxAmountVSLevel;
        public AnimationCurve spawnRateVSLevel;
        public MonsterSO monster;

        // NEW: spawn-distance constraints for Direct movers (grid units)
        // If minSpawnDistance == 0 and maxSpawnDistance == 0 => not used
        public float minSpawnDistance;
        public float maxSpawnDistance;
    }

    [Serializable]
    public struct BossMonsterSpawn
    {
        public int range;
        public MonsterSO boss;

        public List<MonsterSpawn> bossMinionSpawns;
    }

    [Serializable]
    public struct WaveEntry
    {
        public MonsterSO monster;
        public int count;
        public int range; // where to pull candidates from
    }

    [Serializable]
    public struct Wave
    {
        public float triggerTime; // seconds since level start
        public List<WaveEntry> entries;
    }
}
