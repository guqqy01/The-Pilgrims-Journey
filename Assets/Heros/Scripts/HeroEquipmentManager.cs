using Equipment;
using UnityEngine;
using GameManagerCalls;
using System.Linq;
using System.Collections.Generic;
namespace Heros
{
    [RequireComponent(typeof(EquipmentInventory))]
    public class HeroEquipmentManager : Singleton<HeroEquipmentManager>, IOnAfterCreateLocalMap, IOnCompleteLocalMapLevel, IOnPlayLocalLevel
    {
        private EquipmentInventory inventory;
        public EquipmentInventory Inventory => inventory;
        private void Awake()
        {
            inventory = GetComponent<EquipmentInventory>();

            inventory.onEquipmentChanged += OnEquipChange;
        }

        private void OnEquipChange(EquipmentContainer equipmentContainer, EquipmentSO from, EquipmentSO to)
        {
            RemoveEquipment(equipmentContainer, from);
            AddEquipment(equipmentContainer, to);
        }

        private void AddEquipment(EquipmentContainer equipmentContainer, EquipmentSO equipment)
        {
            if (equipment == null)
                return;

            List<EquipmentContainer> containers = inventory.Containers.ToList();

            if (!containers.Contains(equipmentContainer))
            {
                Debug.LogWarning($"Equipment container: {equipmentContainer}, is not in the inventory list");
                return;
            }

            foreach (var kvp in equipment.HeroEquipmentStats)
            {
                HeroStats stat = kvp.Key;
                Buff buff = kvp.Value;

                int index = containers.IndexOf(equipmentContainer);

                HeroStatBuffsManager.Instance.AddBuff($"{equipment.name}: {index}", stat, buff);
            }
        }

        private void RemoveEquipment(EquipmentContainer equipmentContainer, EquipmentSO equipment)
        {
            if(equipment == null) 
                return;

            List<EquipmentContainer> containers = inventory.Containers.ToList();

            if (!containers.Contains(equipmentContainer))
            {
                Debug.LogWarning($"Equipment container: {equipmentContainer}, is not in the inventory list");
                return;
            }

            foreach (var kvp in equipment.HeroEquipmentStats)
            {
                HeroStats stat = kvp.Key;
                Buff buff = kvp.Value;

                int index = containers.IndexOf(equipmentContainer);

                HeroStatBuffsManager.Instance.RemoveBuff($"{equipment.name}: {index}", stat);
            }
        }

        public void ApplyEquipmentBuffs()
        {
            if (inventory == null)
                inventory = GetComponent<EquipmentInventory>();

            if (inventory == null)
                return;

            foreach(var equipmentContainer in inventory.Containers)
            {
                if (equipmentContainer.EquipmentSO == null)
                    continue;

                AddEquipment(equipmentContainer, equipmentContainer.EquipmentSO);
            }
        }

        public void AfterCreateLocalMap()
        {
            inventory.Unlock();
        }

        public void OnCompleteLocalMapLevel()
        {
            inventory.Unlock();
        }

        public void OnPlayLocalLevel()
        {
            inventory.Lock();
        }
    }
}

