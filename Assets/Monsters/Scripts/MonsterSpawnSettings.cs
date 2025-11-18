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
    }

    [Serializable]
    public struct MonsterSpawn
    {
        public int range;
        public AnimationCurve maxAmountVSLevel;
        public AnimationCurve spawnRateVSLevel;
        public MonsterSO monster;
    }

    [Serializable]
    public struct BossMonsterSpawn
    {
        public int range;
        public MonsterSO boss;
    }
}
