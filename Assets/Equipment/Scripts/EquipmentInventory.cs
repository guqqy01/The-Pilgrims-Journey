using GameManagerCalls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Equipment.EquipmentInventory;

namespace Equipment
{
    public class EquipmentInventory : MonoBehaviour, ISaveLoad, IComparable<EquipmentInventory>, IOnSetupNewGame, IOnSave, IOnLoadAfterMapPointSceneLoad, IOnClear
    {
        [SerializeField]
        private int inventorySize = 6;

        public int InventorySize
        {
            get { return inventorySize; }
            set
            {
                if (value < 0)
                    inventorySize = 0;
                else
                    inventorySize = value;

                SetupAmountOfContainers();
            }
        }

        private EquipmentContainer[] containers;

        public EquipmentContainer[] Containers
        {
            get
            {
                SetupAmountOfContainers();

                return containers;
            }
        }

        [SerializeField]
        private List<EquipmentSO> startEquipment;

        [SerializeField]
        private int priority;

        public string SaveFileName => $"EquipmentInventory_{name}_Save";

        public delegate void OnContainerSizeChanged(int size);
        public OnContainerSizeChanged onContainerSizeChanged;

        public List<EquipmentSO> NonNullEquipment =>
                Containers.Where(c => c?.EquipmentSO != null)
                            .Select(c => c.EquipmentSO)
                            .ToList();

        public delegate void OnEquipmentChanged(EquipmentContainer container, EquipmentSO from, EquipmentSO to);
        public OnEquipmentChanged onEquipmentChanged;

        [SerializeField]
        private bool saveLoadInventory = true;

        protected virtual void OnDestroy()
        {
            foreach (var kvp in containerChangeHandlers)
            {
                if (kvp.Key != null)
                    kvp.Key.onEquipmentChanged -= kvp.Value;
            }
            containerChangeHandlers.Clear();
        }


        private Dictionary<EquipmentContainer, EquipmentContainer.OnEquipmentChanged> containerChangeHandlers = new Dictionary<EquipmentContainer, EquipmentContainer.OnEquipmentChanged>();

        private void SetupAmountOfContainers()
        {
            bool containersSizeChanged = false;

            if (containers == null)
            {
                containers = new EquipmentContainer[inventorySize];

                for (int i = 0; i < inventorySize; i++)
                {
                    containers[i] = new EquipmentContainer(this);
                    RegisterContainer(containers[i]);
                }

                containersSizeChanged = true;
            }

            if (containers.Length != inventorySize)
            {
                if (containers.Length > inventorySize)
                {
                    for (int i = inventorySize; i < containers.Length; i++)
                    {
                        if (containers[i] != null && containerChangeHandlers.TryGetValue(containers[i], out var handler))
                        {
                            containers[i].onEquipmentChanged -= handler;
                            containerChangeHandlers.Remove(containers[i]);
                        }
                    }
                }

                Array.Resize(ref containers, inventorySize);

                for (int i = 0; i < inventorySize; i++)
                {
                    if (containers[i] == null)
                    {
                        containers[i] = new EquipmentContainer(this);
                        RegisterContainer(containers[i]);
                    }
                }

                containersSizeChanged = true;
            }
            

            if (containersSizeChanged)
                onContainerSizeChanged?.Invoke(containers.Length);
        }

        public void Lock()
        {
            foreach(var container in containers)
            {
                container.Active = false;
            }
        }

        public void Unlock()
        {
            foreach (var container in containers)
            {
                container.Active = true;
            }
        }

        private void RegisterContainer(EquipmentContainer container)
        {
            if (container == null || containerChangeHandlers.ContainsKey(container))
                return;

            EquipmentContainer.OnEquipmentChanged handler = (from, to) =>
                onEquipmentChanged?.Invoke(container, from, to);

            containerChangeHandlers[container] = handler;
            container.onEquipmentChanged += handler;
        }

        private void Start()
        {
            AddThisToEquipmentManagerInventoriesAfterCommonSceneLoaded();
        }

        private void AddThisToEquipmentManagerInventoriesAfterCommonSceneLoaded()
        {
            SceneRoot.DoActionAfterCommonSceneLoaded(AddToEquipmentManagerInventories);
        }

        private void AddToEquipmentManagerInventories()
        {
            EquipmentManager.Instance.EquipmentInventories.Add(this);
        }

        void IOnClear.Clear()
        {
            Clear();
        }

        public virtual void Clear()
        {
            if (Containers == null)
                return;

            for (int i = 0; i < Containers.Length; i++)
            {
                Containers[i]?.Clear();
            }
        }

        public void SetupNewGame()
        {
            AddThisToEquipmentManagerInventoriesAfterCommonSceneLoaded();

            Setup(startEquipment);
        }

        public void Save()
        {
            if (!saveLoadInventory)
                return;

            List<ContainerSave> saves = new List<ContainerSave>();

            for (int i = 0; i < Containers.Length; i++)
            {
                saves.Add(Containers[i].GetSave());
            }

            SaveData saveData = new SaveData(saves);
            SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
            SaveManager.SaveOver(saveProfile);
        }

        public void LoadAfterMapPointSceneLoad()
        {
            if (!saveLoadInventory)
                return;

            Clear();

            if (SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> outSaveProfile))
            {
                SaveData saveData = outSaveProfile.saveData;

                Setup(saveData.equipment);
            }
            else
            {
                Debug.LogWarning($"No save data found for {SaveFileName}");
                return;
            }
        }

        public void Setup(List<ContainerSave> containerSaves)
        {
            InventorySize = containerSaves.Count;

            Setup(containerSaves.Select(x => x.equipment).ToArray());
        }

        public void Setup(List<EquipmentSO> equipmentToSetup)
        {
            Setup(equipmentToSetup.ToArray());
        }

        public void Setup(EquipmentSO[] equipmentToSetup)
        {
            for (int i = 0; i < equipmentToSetup.Length; i++)
            {
                if (equipmentToSetup[i] == null)
                    continue;

                if (!TryAddEquipment(equipmentToSetup[i]))
                {
                    Debug.Log("Attempted to add equipment failed: " + equipmentToSetup[i]);
                }
            }
        }

        public bool CanAddEquipment(EquipmentSO equipmentToAdd)
        {
            if (equipmentToAdd == null)
                return false;

            for (int i = 0; i < Containers.Length; i++)
            {
                if (Containers[i].EquipmentSO == null && Containers[i].CanChangeEquipment(equipmentToAdd))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasFreeSpot()
        {
            foreach(EquipmentContainer container in Containers)
            {
                if (container.AllowedEquipment == null && container.EquipmentSO == null && container.Active)
                    return true;
            }

            return false;
        }

        public bool HasFreeSpot(EquipmentSO equipment)
        {
            foreach (EquipmentContainer container in Containers)
            {
                if ((container.AllowedEquipment == null || container.AllowedEquipment == equipment) && container.EquipmentSO == null && container.Active)
                    return true;
            }

            return false;
        }

        public bool GetFreeSpot(EquipmentSO equipment, out EquipmentContainer returnContainer)
        {
            returnContainer = null;

            foreach (EquipmentContainer container in Containers)
            {
                if ((container.AllowedEquipment == null || container.AllowedEquipment == equipment) && container.EquipmentSO == null && container.Active)
                {
                    returnContainer = container;
                    return true;
                }
            }

            return false;
        }

        public bool TryAddEquipment(EquipmentSO equipmentToAdd)
        {
            if (equipmentToAdd == null)
                return false;

            if (CanAddEquipment(equipmentToAdd))
            {
                GetFreeSpot(equipmentToAdd, out EquipmentContainer container);

                if (container.TryChangeEquipment(equipmentToAdd))
                {
                    return true;
                }

                return false;
            }
            else
            {
                return false;
            }
        }


        [Serializable]
        private record SaveData : SaveProfileData
        {
            public List<ContainerSave> equipment;
            public SaveData(List<ContainerSave> equipment)
            {
                this.equipment = equipment;
            }
        }

        public EquipmentInventorySave GetEquipmentInventorySave()
        {
            List<ContainerSave> containerSaves = new List<ContainerSave>();

            foreach(EquipmentContainer container in Containers)
            {
                containerSaves.Add(container.GetSave());
            }

            return new EquipmentInventorySave(containerSaves);
        }

        public int CompareTo(EquipmentInventory other)
        {
            return other.priority.CompareTo(priority);
        }
    }


    [Serializable]
    public class EquipmentInventorySave
    {
        public List<ContainerSave> equipmentSaves;

        public EquipmentInventorySave(List<ContainerSave> equipmentSaves)
        {
            this.equipmentSaves = equipmentSaves;
        }
    }

    [Serializable]
    public class ContainerSave
    {
        public EquipmentSO equipment;
        public EquipmentSO allowedEquipment;
        public bool active;

        public ContainerSave(EquipmentSO equipment, EquipmentSO allowedEquipment, bool active)
        {
            this.equipment = equipment;
            this.allowedEquipment = allowedEquipment;
            this.active = active;
        }
    }

    
}
