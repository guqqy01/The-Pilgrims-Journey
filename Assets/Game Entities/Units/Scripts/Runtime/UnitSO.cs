using System;
using UnityEngine;
using Relics;
using System.Collections.Generic;
using Spells;
using Tiles;

namespace Units
{
    [CreateAssetMenu(menuName = "Units/UnitSO")]
    public class UnitSO : BuffableEntitySO, IDescriptable, ITileable
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

        [Header("Unit Only Stats")]
        [BuffType(false)]
        public Buff SpawnRate;

        protected override Buff InheritGetBuff(StatTypes stat)
        {
            return stat switch
            {
                StatTypes.SpawnRate => SpawnRate,
                _ => null,
            };
        }

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

        public List<ISubDescriptionData> GetSubDescriptionData => new List<ISubDescriptionData>();

        public string ShortDescription => Description;

        public SpellSO unitsSpecialSpell;

        [Header("Drops")]
        public float Coin1Percent = 0.5f;
        public float Coin5Percent = 0.1f;
        public float Coin10Percent = 0.01f;

        public IntOrFloatObject GetStat(StatTypes unitStat)
        {
            return unitStat switch
            {
                StatTypes.Health => Health,
                StatTypes.Shield => Shield,
                StatTypes.ShieldCooldown => ShieldCooldown,
                StatTypes.ShieldRegeneration => ShieldRegeneration,
                StatTypes.SpawnRate => SpawnRate,
                StatTypes.MovementSpeed => MovementSpeed,
                _ => throw new NotImplementedException($"Stat {unitStat} not implemented in UnitSO")
            };
        }
    }
}