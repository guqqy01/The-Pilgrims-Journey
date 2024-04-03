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

        [Header("Health")]
        public int baseHealth = 5;

        [Header("Shield")]
        public int Shield = 0;
        /// <summary>
        /// Amount of regeneration over 1 second
        /// </summary>
        public float ShieldRegeneration = 1f;
        /// <summary>
        /// Amount of time since taken damage to start regeneration 
        /// </summary>
        public float ShieldCooldown = 2.25f;

        [Header("Drops")]
        public float coin1Percent = 0.5f;
        public float coin5Percent = 0.1f;
        public float coint10Percent = 0.01f;

        [Header("Objects")]
        public Unit unitPrefab;

        public Sprite unitSprite;
    }
}

