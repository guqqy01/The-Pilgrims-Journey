using System;
using System.Collections;
using System.Collections.Generic;
using Units;
using UnityEngine;


namespace Equipment
{
    public class EquipmentManager : MonoBehaviour
    {
        public static EquipmentManager Instance { private set; get; }

        [SerializeField]
        private List<EquipmentAndState> initialEquipmentStates;

        public Dictionary<EquipmentSO, LockedLevelState> EquipmentStates { private set; get; } = new Dictionary<EquipmentSO, LockedLevelState>();


        public List<EquipmentInventory> EquipmentInventories = new();

        public EquipmentInventory GetPriorityInventory(EquipmentInventory exclusion, EquipmentSO equipment)
        {
            EquipmentInventories.RemoveAll(x => x == null);

            EquipmentInventories.Sort();

            foreach (EquipmentInventory inv in EquipmentInventories)
            {
                if(inv != exclusion && inv.gameObject.activeInHierarchy && inv.HasFreeSpot(equipment))
                {
                    return inv;
                }
            }

            return null;
        }

        public void ChangeUnitState(EquipmentSO equipment, LockedLevelState state)
        {
            EquipmentStates[equipment] = state;
        }

        private void Awake()
        {
            Instance = this;

            EquipmentStates.Clear();

            foreach (var equip in initialEquipmentStates)
            {
                if (!EquipmentStates.ContainsKey(equip.equipment))
                {
                    EquipmentStates.Add(equip.equipment, equip.state);
                }
            }
        }
    }

    [Serializable]
    public class EquipmentAndState
    {
        public EquipmentSO equipment;
        public LockedLevelState state;
    }
}
