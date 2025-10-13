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
    public class EquipmentInventory : MonoBehaviour, ISaveLoad, IComparable<EquipmentInventory>, ISetup
    {
        private int inventorySize = 6;

        private EquipmentContainer[] containers;

        public EquipmentContainer[] Containers
        {
            get
            {
                if (containers == null)
                {
                    containers = new EquipmentContainer[inventorySize];
                }

                return containers;
            }
        }

        [SerializeField]
        private List<EquipmentSO> startEquipment;

        [SerializeField]
        private int priority;

        public string SaveFileName => $"EquipmentInventory_{name}_Save";

        public delegate void OnEquipmentChanged();
        public OnEquipmentChanged onEquipmentChanged;

        public List<EquipmentSO> NonNullEquipment
        {
            get
            {
                List<EquipmentSO> returnList = new List<EquipmentSO>();

                foreach (EquipmentContainer equipmentItem in Containers)
                {
                    if(equipmentItem != null && equipmentItem.EquipmentSO != null)
                        returnList.Add(equipmentItem.EquipmentSO);
                }

                return returnList;
            }
        }

        protected virtual void Awake()
        {
            GameManager.onSave += Save;
            GameManager.onLoad += Load;
            GameManager.onSetup += Setup;
            GameManager.onClear += Clear;
        }

        protected virtual void OnDestroy()
        {
            GameManager.onSave -= Save;
            GameManager.onLoad -= Load;
            GameManager.onSetup -= Setup;
            GameManager.onClear -= Clear;
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

        public void Clear()
        {
            if (Containers == null)
                return;

            for (int i = 0; i < Containers.Length; i++)
            {
                Containers[i] = null;
            }

            onEquipmentChanged?.Invoke();
        }

        public void Setup()
        {
            AddThisToEquipmentManagerInventoriesAfterCommonSceneLoaded();

            Setup(startEquipment);

            onEquipmentChanged?.Invoke();
        }

        public void Save()
        {
            List<ContainerSave> saves = new List<ContainerSave>();

            for (int i = 0; i < Containers.Length; i++)
            {
                saves.Add(Containers[i].GetSave());
            }

            SaveData saveData = new SaveData(saves);
            SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
            SaveManager.SaveOver(saveProfile);
        }

        public void Load()
        {
            SaveData saveData = SaveManager.Load<SaveData>(SaveFileName).saveData;

            Setup(saveData.equipment);
        }

        public void Setup(List<ContainerSave> containerSaves)
        {
            for (int i = 0; i < containerSaves.Count; i++)
            {
                Containers[i].AllowedEquipment = containerSaves[i].allowedEquipment;

                Containers[i].Active = containerSaves[i].active;
            }

            onEquipmentChanged?.Invoke();
        }

        public void Setup(List<EquipmentSO> equipmentToSetup)
        {
            Setup(equipmentToSetup.ToArray());
        }

        public void Setup(EquipmentSO[] equipmentToSetup)
        {
            Clear();

            for (int i = 0; i < equipmentToSetup.Length; i++)
            {
                if (equipmentToSetup[i] == null)
                    continue;
                

            }

            onEquipmentChanged?.Invoke();
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
                    onEquipmentChanged?.Invoke();
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
            return - priority.CompareTo(other.priority);
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
