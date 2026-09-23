using System.Collections.Generic;
using UnityEngine;
using Units;
using static UnityEngine.ParticleSystem;
using AYellowpaper.SerializedCollections;
using Heros;
using UnityEngine.Serialization;
using TPJ.Gameplay;

namespace Equipment
{
    [CreateAssetMenu()] 
    public class EquipmentSO : ScriptableObject, IDescriptable, ICardable
    {
        public Sprite sprite;
        public Sprite Sprite => sprite;

        [TextArea(4, 10)]
        public string description;

        public Rarity Rarity
        {
            get
            {
                if (goldCost <= (int)Rarity.Common)
                    return Rarity.Common;
                if (goldCost <= (int)Rarity.Uncommon)
                    return Rarity.Uncommon;
                if (goldCost <= (int)Rarity.Rare)
                    return Rarity.Rare;
                if (goldCost <= (int)Rarity.Epic)
                    return Rarity.Epic;

                else return Rarity.Legendary;
            }
        }

        public int goldCost
        {
            get
            {
                int value = 0;

                if (equipmentStats != null)
                    foreach (var kvp in equipmentStats)
                    {
                        value += BuffsGoldValueCalculator.GetValue(kvp.Value, kvp.Key.ConvertToAllStats(), out _, out _, out _);
                    }
                int specialValue = (EquipmentSpecial != null) ? EquipmentSpecial.GoldValue() : 0;
                value += specialValue;

                return value;
            }
        }

        public bool TryGetBuff(StatTypes stat, out Buff outBuff)
        {
            outBuff = null;

            if (!EquipmentStats.ContainsKey(stat))
                return false;


            outBuff = EquipmentStats[stat];

            return true;
        }

        [SerializeReference]
        [AbstractSelector]
        public EquipmentSpecial EquipmentSpecial;

        [SerializeField]
        private SerializedDictionary<StatTypes, Buff> equipmentStats;

        public Dictionary<StatTypes, Buff> EquipmentStats
        {
            get
            {
                equipmentStats ??= new SerializedDictionary<StatTypes, Buff>();

                return equipmentStats;
            }
        }

        public string Title => name;

        public string GoldAmount => goldCost.ToString();

        public string MainDescription => description;
        public string ShortDescription => description;

        public List<ISubDescriptionData> GetSubDescriptionData
        {
            get
            {
                var list = new List<ISubDescriptionData>();

                foreach(var kvp in equipmentStats)
                {
                    list.Add(new BuffSubDescriptionData(kvp.Value, kvp.Key.ToString().AddSpacesBeforeCapsRegex()));
                }

                return list;
            }
        }

        public int ActionPointCost => actionCost;
        [SerializeField]
        private int actionCost = 1;

        [Header("Shop Settings")]
        // X position is normalized to 0-1 so end of the map will be at t = 1
        public MinMaxCurve ChanceOfBeginInShopAtXMapPosition;

        public Vector2Int IfInShopHowManyRange;

        public WeightedChanceList<float> discounts;

        public CardBase CreateCard() => new EquipmentCard(this);
    }
}