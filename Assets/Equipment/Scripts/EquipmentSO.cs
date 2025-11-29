using System.Collections.Generic;
using UnityEngine;
using Units;
using System;
using System.Linq;
using static UnityEngine.ParticleSystem;
using AYellowpaper.SerializedCollections;
using Units.UI;

namespace Equipment
{
    [CreateAssetMenu()] 
    public class EquipmentSO : ScriptableObject, IDescriptable
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
                        value += UnitBuffsGoldValueCalculator.GetValue(kvp.Value, kvp.Key, out _, out _, out _);
                    }
                int specialValue = (EquipmentSpecial != null) ? EquipmentSpecial.GoldValue() : 0;
                value += specialValue;

                return value;
            }
        }

        public bool TryGetBuff(UnitStatsEnum stat, out Buff outBuff)
        {
            outBuff = null;

            if (!EquipmentStatsDictionary.ContainsKey(stat))
                return false;


            outBuff = EquipmentStatsDictionary[stat];

            return true;
        }

        [SerializeReference]
        [AbstractSelector]
        public EquipmentSpecial EquipmentSpecial;

        [SerializeField]
        private SerializedDictionary<UnitStatsEnum, Buff> equipmentStats;

        public Dictionary<UnitStatsEnum, Buff> EquipmentStatsDictionary
        {
            get
            {
                if (equipmentStats != null)
                    return equipmentStats;

                equipmentStats = new SerializedDictionary<UnitStatsEnum, Buff>();

                return equipmentStats;
            }
        }

        public string Title => name;

        public string Description => description;

        public string GoldAmount => goldCost.ToString();

        public string MainDescription => description;

        [Header("UI Description")]
        [SerializeField]
        private UnitStatSubDescriptionUI descriptionStatUIPrefab;


        public List<SubDescriptionUIAndParams> SubDescriptionPrefabsAndParams
        {
            get
            {
                List<SubDescriptionUIAndParams> returnList = new List<SubDescriptionUIAndParams>();

                foreach(var kvp in equipmentStats)
                {
                    object[] paramObjects = new object[2];
                    paramObjects[0] = kvp.Value;
                    paramObjects[1] = kvp.Key;
                    returnList.Add(new SubDescriptionUIAndParams(descriptionStatUIPrefab, paramObjects));
                }

                return returnList;
            }
        }

       

        [Header("Shop Settings")]
        // X position is normalized to 0-1 so end of the map will be at t = 1
        public MinMaxCurve ChanceOfBeginInShopAtXMapPosition;

        public Vector2Int IfInShopHowManyRange;

        public WeightedChanceList<float> discounts;
    }
}

