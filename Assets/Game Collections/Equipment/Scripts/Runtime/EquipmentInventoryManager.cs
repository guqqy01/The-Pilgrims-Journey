using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Equipment
{
    public class EquipmentInventoryManager : Singleton<EquipmentInventoryManager>
    {
        [SerializeField]
        private List<EquipmentAndState> initialEquipmentStates;

        public Dictionary<EquipmentSO, LockedLevelState> EquipmentStates { private set; get; } = new Dictionary<EquipmentSO, LockedLevelState>();


        public List<EquipmentInventory> EquipmentInventories = new();

        public void ChangeUnitState(EquipmentSO equipment, LockedLevelState state)
        {
            EquipmentStates[equipment] = state;
        }

        private void Awake()
        {
            EquipmentStates.Clear();

            foreach (var equip in initialEquipmentStates)
            {
                if (!EquipmentStates.ContainsKey(equip.equipment))
                {
                    EquipmentStates.Add(equip.equipment, equip.state);
                }
            }

            base.InitiateFromRootAtAwake();
        }
    }

    [Serializable]
    public class EquipmentAndState
    {
        public EquipmentSO equipment;
        public LockedLevelState state;
    }
}
