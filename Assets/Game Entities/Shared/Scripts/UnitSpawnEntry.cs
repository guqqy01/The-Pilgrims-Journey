using System;
using Units;
using UnityEngine;

namespace Spawning
{
    [Serializable]
    public class UnitSpawnEntry
    {
        public UnitSO unit;
        public float weight = 1f;
        [Tooltip("Only eligible once the level difficulty reaches this value.")]
        public float minDifficulty = 1f;
    }
}