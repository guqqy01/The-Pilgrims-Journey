using System.Collections.Generic;
using UnityEngine;
using Units;
using System;
using UnityEditor;
using System.Linq;

namespace Equipment
{
    [CreateAssetMenu()]
    public class EquipmentSO : ScriptableObject
    {
        public Sprite sprite;

        [TextArea(4, 10)]
        public string description;

        [SerializeField]
        private Rarity rarity;
        public Rarity Rarity => rarity;
        public int goldCost = 100;

        public List<Buff> equipmentBuffs;

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

        public bool TryGetBuff(UnitStatsEnum stat, out Buff outBuff)
        {
            outBuff = null;

            if (!EquipmentStatsDictionary.ContainsKey(stat))
                return false;


            outBuff = EquipmentStatsDictionary[stat];

            return true;
        }
    }

}

