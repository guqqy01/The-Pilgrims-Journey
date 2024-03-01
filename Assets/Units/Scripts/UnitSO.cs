using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    [CreateAssetMenu()]
    public class UnitSO : ScriptableObject
    {
        [Header("Spawning")]
        public float BaseSpawnRate = 1f;

        [Header("Movement")]
        public float MovementSpeed = 1f;
        public float SlowResist = 0.2f;

        [Header("Health")]
        public int baseHealth = 5;

        [Header("Shield")]
        public int Shield = 5;
        /// <summary>
        /// Amount of time to regenerate one shield point
        /// </summary>
        public float ShieldRegeneration = 1f;
        /// <summary>
        /// Amount of time since taken damage to start regeneration 
        /// </summary>
        public float ShieldCooldown = 2.25f;

        [Header("Objects")]
        public Unit unitPrefab;

        public Sprite unitSprite;
    }

    public enum Rarity
    {
        common = 1,
        uncommon = 2,
        rare = 3,
        epic = 4,
        legendary = 5,
    }

    public struct RarityCalc
    {
        public Rarity rarity;
        public float spawnRate;
    }
}

