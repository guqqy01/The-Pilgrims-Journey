using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Monsters
{
    [CreateAssetMenu()]
    public class MonsterSpawnSettings : ScriptableObject
    {
        [AbstractSelector]
        [SerializeReference]
        public List<NormalMonsterSpawn> normalMonsterSpawns;

        [AbstractSelector]
        [SerializeReference]
        public List<WaveMonsterSpawn> waveMonsterSpawns;

        public List<BossMonsterSpawn> bossMonsters;

        public IReadOnlyList<MonsterSpawn> MonsterSpawns
        {
            get
            {
                List<MonsterSpawn> monsterSpawns = new List<MonsterSpawn>();

                monsterSpawns.AddRange(normalMonsterSpawns);
                monsterSpawns.AddRange(waveMonsterSpawns);

                return monsterSpawns;
            }
        }
    }

    public interface IMonsterSpawn
    {
        public MonsterSO MonsterSO { get; }
    }

    public interface INormalMonsterSpawn: IMonsterSpawn
    {
        public float SpawnRate
        {
            get;
        }
        
    }

    public interface IWaveMonsterSpawn : IMonsterSpawn
    {
        public float Count
        {
            get;
        }
    }

    public interface IDirectMonsterSpawn : IMonsterSpawn
    {
        public float MinSpawnDistance { get; }
        public float MaxSpawnDistance { get; }
    }

    public interface IStationaryMonsterSpawn : IMonsterSpawn
    {
        public int Range { get; }
    }

    public class NormalDirectMonsterSpawn : NormalMonsterSpawn, IDirectMonsterSpawn
    {
        [SerializeField]
        private float minSpawnDistance;
        [SerializeField]
        private float maxSpawnDistance;

        public float MinSpawnDistance => minSpawnDistance;

        public float MaxSpawnDistance => maxSpawnDistance;
    }

    public class NormalStationaryMonsterSpawn : NormalMonsterSpawn, IStationaryMonsterSpawn
    {
        [SerializeField]
        private int range;
        public int Range => range;
    }

    [Serializable]
    public abstract class NormalMonsterSpawn : MonsterSpawn, INormalMonsterSpawn
    {
        [SerializeField]
        private AnimationCurve spawnRateVSLevel;

        public float SpawnRate
        {
            get
            {
                return  1f / spawnRateVSLevel.Evaluate(GameManager.Instance.Level);
            }
        }
    }

    [Serializable]
    public abstract class MonsterSpawn: IMonsterSpawn
    {
        public MonsterSO monster;
        public float Difficulty => monster.powerLevel;

        public MonsterSO MonsterSO => monster;
    }

    [Serializable]
    public abstract class WaveMonsterSpawn : MonsterSpawn, IWaveMonsterSpawn
    {
        [SerializeField]
        private int count;

        public float Count => count;
    }

    public class WaveDirectMonsterSpawn : WaveMonsterSpawn, IDirectMonsterSpawn
    {
        [SerializeField]
        private float minSpawnDistance;
        [SerializeField]
        private float maxSpawnDistance;

        public float MinSpawnDistance => minSpawnDistance;
        public float MaxSpawnDistance => maxSpawnDistance;
    }

    public class WaveStationaryMonsterSpawn : WaveMonsterSpawn, IStationaryMonsterSpawn
    {
        [SerializeField]
        private int range;
        public int Range => range;
    }

    [Serializable]
    public struct BossMonsterSpawn
    {
        public int range;
        public MonsterSO boss;

        [SerializeReference]
        [AbstractSelector]
        public List<NormalMonsterSpawn> bossMinionSpawns;
    }

    [Serializable]
    public struct WaveEntry
    {
        public MonsterSO monster;
        public int count;
    }

    [Serializable]
    public struct Wave
    {
        public float triggerTime; // seconds since level start
        public List<WaveEntry> entries;
    }
}
