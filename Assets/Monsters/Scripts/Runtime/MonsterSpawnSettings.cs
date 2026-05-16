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
        [AbstractSelector]
        [SerializeReference]
        public List<MonsterSpawn> rangeAndMonster;
        public List<BossMonsterSpawn> bossMonsters;

        public List<Wave> waves = new List<Wave>(); // define waves in inspector
    }

    public class DirectMonsterSpawn : MonsterSpawn
    {
        public float minSpawnDistance;
        public float maxSpawnDistance;
    }

    public class StationaryMonsterSpawn : MonsterSpawn
    {
        public int range;
    }

    [Serializable]
    public abstract class MonsterSpawn
    {
        public AnimationCurve maxAmountVSLevel;
        public AnimationCurve spawnRateVSLevel;
        public MonsterSO monster;
    }

    [Serializable]
    public struct BossMonsterSpawn
    {
        public int range;
        public MonsterSO boss;

        [SerializeReference]
        [AbstractSelector]
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
