using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu()]
public class MonsterSpawnSettings : ScriptableObject
{
    public List<MonsterSpawn> rangeAndMonster;
}

[Serializable]
public struct MonsterSpawn
{
    public int range;
    public AnimationCurve spawnAmountVSLevel;
    public GameObject monster;
}
