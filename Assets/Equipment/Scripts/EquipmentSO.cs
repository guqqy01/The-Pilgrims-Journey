using System.Collections.Generic;
using UnityEngine;
using Units;
using System;
using UnityEditor;
using System.Linq;

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

        public bool TryGetBuff(UnitStatsEnum stat, out Buff outBuff)
        {
            outBuff = null;

            if (!EquipmentStatsDictionary.ContainsKey(stat))
                return false;


            outBuff = EquipmentStatsDictionary[stat];

            return true;
        }
    }

    [CustomEditor(typeof(EquipmentSO))]
    public class EquipmentSOEditor : Editor
    {
        public static Color CommonColor = Color.white;
        public static Color UncommonColor = Color.green;
        public static Color RareColor = Color.blue;
        public static Color EpicColor = new Color(0.64f, 0.21f, 0.93f);
        public static Color LegendaryColor = new Color(1f, 0.5f, 0f);

        public override void OnInspectorGUI()
        {
            EquipmentSO equipmentSO = (EquipmentSO)target;

            base.OnInspectorGUI();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel("Total Gold Value");

            EditorGUILayout.LabelField(equipmentSO.goldCost.ToString());

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel("Rarity");

            GUIStyle gUIStyle = new GUIStyle(EditorStyles.boldLabel);
            switch (equipmentSO.Rarity)
            {
                case Rarity.Common:
                    gUIStyle.normal.textColor = CommonColor;
                    break;
                case Rarity.Uncommon:
                    gUIStyle.normal.textColor = UncommonColor;
                    break;
                case Rarity.Rare:
                    gUIStyle.normal.textColor = RareColor;
                    break;
                case Rarity.Epic:
                    gUIStyle.normal.textColor = EpicColor;
                    break;
                case Rarity.Legendary:
                    gUIStyle.normal.textColor = LegendaryColor;
                    break;
                default:
                    break;
            }

            EditorGUILayout.LabelField(equipmentSO.Rarity.ToString(), gUIStyle);

            EditorGUILayout.EndHorizontal();
        }
    }
}

