using System;
using UnityEngine;
using Relics;
using System.Collections.Generic;
using Spells;

namespace Units
{
    [CreateAssetMenu(menuName = "Units/UnitSO")]
    public class UnitSO : ScriptableObject, IDescriptable
    {
        [TextArea] public string Description;
        public Sprite sprite;

        [Header("Objects")]
        public Unit unitPrefab;

        [SerializeField] private Rarity rarity;
        public Rarity Rarity => rarity;

        [Header("Leveling and Health")]
        public int numberOfUnitsEscapedToLevelUp = 50;
        public int numberOfUnitsAbleToSpawn = 15;

        [Header("Stats")]
        [SerializeField] private float spawnRate = 0.5f;
        public float SpawnRate => spawnRate;

        [SerializeField] private float movementSpeed = 0.5f;
        public float MovementSpeed => movementSpeed;

        [SerializeField] private int health = 10;
        public int Health => health;

        [SerializeField] private int shield = 0;
        public int Shield => shield;

        /// <summary>Amount of regeneration over 1 second</summary>
        [SerializeField] private float shieldRegeneration = 1f;
        public float ShieldRegeneration => shieldRegeneration;

        /// <summary>Time since damage before regeneration starts</summary>
        [SerializeField] private float shieldCooldown = 6f;
        public float ShieldCooldown => shieldCooldown;

        public string Title => name.AddSpacesBeforeCapsRegex();

        public string MainDescription
        {
            get
            {
                string returnString = Description + "\r\n\r\n";

                returnString += $"Spawn Rate: {SpawnRate}\r\n";
                returnString += $"Movement Speed: {MovementSpeed}\r\n";
                returnString += $"Health: {Health}\r\n";
                returnString += $"Shield: {Shield}\r\n";
                returnString += $"Shield Regeneration: {ShieldRegeneration}\r\n";
                returnString += $"Shield Cooldown: {ShieldCooldown}";

                return returnString;
            }
        }

        public Sprite Sprite => sprite;

        public List<SubDescriptionUIAndParams> SubDescriptionPrefabsAndParams => new List<SubDescriptionUIAndParams>()
        {

        };

        public SpellSO unitsSpecialSpell;

        [Header("Drops")]
        public float Coin1Percent = 0.5f;
        public float Coin5Percent = 0.1f;
        public float Coin10Percent = 0.01f;

        public IntOrFloatObject GetStat(UnitStatsEnum unitStat)
        {
            return unitStat switch
            {
                UnitStatsEnum.Health => new IntOrFloatObject(Health),
                UnitStatsEnum.Shield => new IntOrFloatObject(Shield),
                UnitStatsEnum.ShieldCooldown => new IntOrFloatObject(ShieldCooldown),
                UnitStatsEnum.ShieldRegeneration => new IntOrFloatObject(ShieldRegeneration),
                UnitStatsEnum.SpawnRate => new IntOrFloatObject(SpawnRate),
                UnitStatsEnum.MovementSpeed => new IntOrFloatObject(MovementSpeed),
                _ => throw new NotImplementedException($"Stat {unitStat} not implemented in UnitSO")
            };
        }
    }
}