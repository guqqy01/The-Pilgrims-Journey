using System;
using System.Collections.Generic;
using UnityEngine;
using Relics;
namespace Units
{
    [CreateAssetMenu()]
    public class UnitSO : ScriptableObject
    {
        [TextArea()]
        public string Description;

        public Sprite sprite;

        [Header("Objects")]
        public Unit unitPrefab;

        [SerializeField]
        private Rarity rarity;
        public Rarity Rarity => rarity;

        [Header("Leveling and Health")]
        public int numberOfUnitsEscapedToLevelUp = 5;
        public int numberOfUnitsAbleToSpawn = 15 ;

        [Header("Stats")]
        [SerializeField]
        private float spawnRate = 0.5f;
        public float SpawnRate => spawnRate;
        [SerializeField]
        private float movementSpeed = 0.5f;
        public float MovementSpeed => movementSpeed;
        [SerializeField]
        private int health = 10;
        public int Health => health;
        [SerializeField]
        private int shield = 0;
        public int Shield => shield;
        /// <summary>
        /// Amount of regeneration over 1 second
        /// </summary>
        [SerializeField] private float shieldRegeneration = 1f;
        public float ShieldRegeneration => shieldRegeneration;
        /// <summary>
        /// Amount of time since taken damage to start regeneration 
        /// </summary>
        [SerializeField] private float shieldCooldown = 6f;
        public float ShieldCooldown => shieldCooldown;

        public SpellSO unitsSpecialSpell; 

        [Header("Drops")]
        public float Coin1Percent = 0.5f;
        public float Coin5Percent = 0.1f;
        public float Coin10Percent = 0.01f;

        public object GetStat(UnitStatsEnum unitStat)
        {
            switch (unitStat)
            {
                case UnitStatsEnum.Health:
                    return Health;
                case UnitStatsEnum.Shield:
                    return Shield;
                case UnitStatsEnum.ShieldCooldown:
                    return ShieldCooldown;
                case UnitStatsEnum.ShieldRegeneration:
                    return ShieldRegeneration;
                case UnitStatsEnum.SpawnRate:
                    return SpawnRate;
                case UnitStatsEnum.MovementSpeed:
                    return MovementSpeed;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}

