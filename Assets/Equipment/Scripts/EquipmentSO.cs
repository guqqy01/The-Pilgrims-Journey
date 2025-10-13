using System.Collections.Generic;
using UnityEngine;
using Units;
using System;
using System.Linq;
using static UnityEngine.ParticleSystem;

namespace Equipment
{
    [CreateAssetMenu()]
    public class EquipmentSO : ScriptableObject, IInfo
    {
        public Sprite sprite;

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

                if (equipmentBuffs != null)
                    foreach (Buff buff in equipmentBuffs)
                    {
                        value += buff.GetBuffGoldValue();
                    }
                int specialValue = (EquipmentSpecial != null) ? EquipmentSpecial.GoldValue() : 0;
                value += specialValue;

                return value;
            }
        }

        public List<Buff> equipmentBuffs;
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

        private Dictionary<UnitStatsEnum, Buff> equipmentStats;

        public Dictionary<UnitStatsEnum, Buff> EquipmentStatsDictionary
        {
            get
            {
                if (equipmentStats != null)
                    return equipmentStats;

                equipmentStats = new Dictionary<UnitStatsEnum, Buff>();

                foreach (Buff equipmentBuff in equipmentBuffs)
                {
                    if (!equipmentStats.ContainsKey(equipmentBuff.Stat))
                        equipmentStats.Add(equipmentBuff.Stat, equipmentBuff);
                    else
                        throw new Exception("two or more of the same stat type buffs on one equipment");
                }

                return equipmentStats;
            }
        }

        public string Title => name;

        public string Description => description;

        public string GoldAmount => goldCost.ToString();

        [Header("Shop Settings")]
        // X position is normalized to 0-1 so end of the map will be at t = 1
        public MinMaxCurve ChanceOfBeginInShopAtXMapPosition;

        public Vector2Int IfInShopHowManyRange;

        public WeightedChanceList<float> discounts;
    }
}

