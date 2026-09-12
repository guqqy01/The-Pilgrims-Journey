using Equipment;
using UnityEngine;
using GameManagerCalls;
using System.Linq;
using System.Collections.Generic;
namespace Heros
{
    [RequireComponent(typeof(EquipmentInventory))]
    public class HeroEquipmentManager : Singleton<HeroEquipmentManager>, IOnAfterCreateLocalMap, IOnCompleteLocalMapLevel, IOnPlayLocalLevel, IOnClear, IOnSetupNewGame
    {
        private EquipmentInventory inventory;
        public EquipmentInventory Inventory => inventory;
        private void Awake()
        {
            inventory = GetComponent<EquipmentInventory>();
            inventory.onEquipmentChanged -= OnEquipChange;
            inventory.onEquipmentChanged += OnEquipChange;
        }

        protected override void CleanUp()
        {
            base.CleanUp();
            if (inventory != null)
                inventory.onEquipmentChanged -= OnEquipChange;
        }

        public void SetupBeginningEquipment(List<EquipmentSO> equipmentSOs)
        {
            if (equipmentSOs == null)
                return;

            if (inventory == null)
            {
                inventory = GetComponent<EquipmentInventory>();
                inventory.onEquipmentChanged -= OnEquipChange;
                inventory.onEquipmentChanged += OnEquipChange;
            }

            foreach(EquipmentSO equipmentSO in equipmentSOs)
            {
                inventory.TryAddEquipment(equipmentSO);
            }
        }

        private void OnEquipChange(EquipmentContainer equipmentContainer, EquipmentSO from, EquipmentSO to)
        {
            RemoveEquipmentBuffs(equipmentContainer, from);
            ApplyEquipmentBuffs(equipmentContainer, to);
        }

        private void ApplyEquipmentBuffs(EquipmentContainer equipmentContainer, EquipmentSO equipment)
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
                StatTypes stat = kvp.Key;
                Buff buff = kvp.Value;

                int index = containers.IndexOf(equipmentContainer);

                HeroStatBuffsManager.Instance.AddBuff($"{equipment.name}: {index}", stat, buff);
            }
        }

        private void RemoveEquipmentBuffs(EquipmentContainer equipmentContainer, EquipmentSO equipment)
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
                StatTypes stat = kvp.Key;
                Buff buff = kvp.Value;

                int index = containers.IndexOf(equipmentContainer);

                HeroStatBuffsManager.Instance.RemoveBuff($"{equipment.name}: {index}", stat);
            }
        }

        public void ApplyAllEquipmentBuffs()
        {
            if (inventory == null)
                inventory = GetComponent<EquipmentInventory>();

            if (inventory == null)
                return;

            foreach(var equipmentContainer in inventory.Containers)
            {
                if (equipmentContainer.EquipmentSO == null)
                    continue;

                ApplyEquipmentBuffs(equipmentContainer, equipmentContainer.EquipmentSO);
            }
        }

        public void AfterCreateLocalMap()
        {
            if (inventory != null)
                inventory.Unlock();
        }

        public void OnCompleteLocalMapLevel()
        {
            if (inventory != null)
                inventory.Unlock();
        }

        public void OnPlayLocalLevel()
        {
            if (inventory != null)
                inventory.Lock();
        }

        public void Clear()
        {
            if (inventory != null)
                inventory.Unlock();
        }

        public void OnSetupNewGame()
        {
            if(inventory != null)
                inventory.Unlock();
        }
    }
}

