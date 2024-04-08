using System.Collections.Generic;
using System;
using UnityEngine;

namespace Units
{
    [Serializable]
    public struct UnitEquipmentRequirements
    {
        public UnitSO resultedUnit;

        [SerializeField]
        private List<EquipmentSO> requiredEquipment;

        public int GetChanceWeight(List<EquipmentSO> equipment)
        {
            int returnInt = 0;

            Dictionary<EquipmentSO, int> equipmentAndAmount = new Dictionary<EquipmentSO, int>();

            for (int i = 0; i < requiredEquipment.Count; i++)
            {
                if (equipmentAndAmount.ContainsKey(requiredEquipment[i]))
                    equipmentAndAmount[requiredEquipment[i]]++;
                else
                    equipmentAndAmount.Add(requiredEquipment[i], 1);
            }

            foreach (EquipmentSO equip in equipment)
            {
                if (equipmentAndAmount.ContainsKey(equip))
                {
                    returnInt++;

                    if(equipmentAndAmount[equip] <= 1)
                    {
                        equipmentAndAmount.Remove(equip);
                    }
                    else
                    {
                        equipmentAndAmount[equip]--;
                    }
                }
            }

            return returnInt;
        }

        public bool EquipmentMatches(List<EquipmentSO> equipment)
        {
            if (equipment.Count != requiredEquipment.Count)
                return false;

            Dictionary<EquipmentSO, int> equipmentAndAmount = new Dictionary<EquipmentSO, int>();

            for (int i = 0; i < requiredEquipment.Count; i++)
            {
                if (equipmentAndAmount.ContainsKey(requiredEquipment[i]))
                    equipmentAndAmount[requiredEquipment[i]]++;
                else
                    equipmentAndAmount.Add(requiredEquipment[i], 1);
            }

            List<EquipmentSO> toRemove = new List<EquipmentSO>();

            foreach (EquipmentSO equip in equipment)
            {
                if (equipmentAndAmount.ContainsKey(equip))
                {
                    if (equipmentAndAmount[equip] <= 1 && !toRemove.Contains(equip))
                    {
                        toRemove.Add(equip);
                    }
                    else
                    {
                        equipmentAndAmount[equip]--;
                    }
                }
                else return false;
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                equipmentAndAmount.Remove(toRemove[i]);
            }

            return equipmentAndAmount.Count == 0;
        }
    }

    [Serializable]
    public struct UnitFromAndUnitTos
    {
        [SerializeField]
        private UnitSO startUnit;
        public UnitSO StartUnit => startUnit;

        
    }
}

