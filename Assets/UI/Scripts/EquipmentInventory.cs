using GameManagerCalls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Equipment
{
    public class EquipmentInventory : MonoBehaviour, ISaveLoad, IComparable<EquipmentInventory>, ISetup
    {
        protected List<EquipmentContainer> containers;

        [SerializeField]
        private List<EquipmentSO> startEquipment;

        [SerializeField]
        private int priority;

        public string SaveFileName => $"EquipmentInventory_{name}_Save";

        [SerializeField]
        protected DragableEquipmentUI dragableEquipmentUIPrefab;

        public delegate void OnEquipmentChanged();
        public OnEquipmentChanged onEquipmentChanged;

        public List<EquipmentSO> Equipment
        {
            get
            {
                List<EquipmentSO> returnList = new List<EquipmentSO>();

                foreach (EquipmentContainer container in containers)
                {
                    if(container.EquipmentSO != null)
                        returnList.Add(container.EquipmentSO);
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
            StartSetup();
        }

        private void StartSetup()
        {
            if (containers != null)
                return;

            containers = GetComponentsInChildren<EquipmentContainer>().ToList();

            foreach (EquipmentContainer container in containers)
            {
                container.onEquipmentChange += (EquipmentSO from, EquipmentSO to) => onEquipmentChanged?.Invoke();
                container.EquipmentInventory = this;
            }

            SceneRoot.DoActionAfterCommonSceneLoaded(AddToEquipmentManagerInventories);
        }

        private void AddToEquipmentManagerInventories()
        {
            EquipmentManager.Instance.EquipmentInventories.Add(this);
        }

        /*protected virtual void OnDisable()
        {
            GameManager.onSave -= Save;
            GameManager.onLoad -= Load;
            GameManager.onSetup -= Setup;
            GameManager.onClear -= Clear;
        }*/

        public void Clear()
        {
            if (containers == null)
                return;

            for (int i = 0; i < containers.Count; i++)
            {
                containers[i].Clear();
            }

            onEquipmentChanged?.Invoke();
        }

        public void Setup()
        {
            StartSetup();

            Setup(startEquipment);

            onEquipmentChanged?.Invoke();
        }

        public void Save()
        {
            List<ContainerSave> saves = new List<ContainerSave>();

            for (int i = 0; i < containers.Count; i++)
            {
                saves.Add(containers[i].GetSave());
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
                containers[i].AllowedEquipment = containerSaves[i].allowedEquipment;

                if (containerSaves[i].equipment != null)
                {
                    DragableEquipmentUI dragableEquipmentUI = Instantiate(dragableEquipmentUIPrefab);

                    if (containers[i].CanChangeEquipment(containerSaves[i].equipment))
                    {
                        dragableEquipmentUI.Init(containerSaves[i].equipment, containers[i]);
                    }

                    if (!containers[i].TryChangeEquipment(dragableEquipmentUI))
                    {
                        Destroy(dragableEquipmentUI.gameObject);
                        Debug.LogWarning("Dragable equipment not able to be placed in container");
                    }
                }
                containers[i].Active = containerSaves[i].active;
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
                
                DragableEquipmentUI dragableEquipmentUI = Instantiate(dragableEquipmentUIPrefab);
                
                if (containers[i].CanChangeEquipment(equipmentToSetup[i]))
                {
                    dragableEquipmentUI.Init(equipmentToSetup[i], containers[i]);
                }

                if (!containers[i].TryChangeEquipment(dragableEquipmentUI))
                {
                    Destroy(dragableEquipmentUI.gameObject);
                    Debug.LogWarning("Dragable equipment not able to be placed in container");
                }
            }

            onEquipmentChanged?.Invoke();
        }

        public bool CanAddEquipment(EquipmentSO equipmentToAdd)
        {
            if (equipmentToAdd == null)
                return false;

            for (int i = 0; i < containers.Count; i++)
            {
                if (containers[i].EquipmentSO == null && containers[i].CanChangeEquipment(equipmentToAdd))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasFreeSpot()
        {
            foreach(EquipmentContainer container in containers)
            {
                if (container.AllowedEquipment == null && container.EquipmentSO == null && container.Active)
                    return true;
            }

            return false;
        }

        public bool HasFreeSpot(EquipmentSO equipment)
        {
            foreach (EquipmentContainer container in containers)
            {
                if ((container.AllowedEquipment == null || container.AllowedEquipment == equipment) && container.EquipmentSO == null && container.Active)
                    return true;
            }

            return false;
        }

        public bool GetFreeSpot(EquipmentSO equipment, out EquipmentContainer returnContainer)
        {
            returnContainer = null;

            foreach (EquipmentContainer container in containers)
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

            DragableEquipmentUI dragableEquipmentUI = Instantiate(dragableEquipmentUIPrefab);

            for (int i = 0; i < containers.Count; i++)
            {
                if (containers[i].EquipmentSO == null && containers[i].CanChangeEquipment(equipmentToAdd))
                {
                    dragableEquipmentUI.Init(equipmentToAdd, containers[i]);

                    if (containers[i].TryChangeEquipment(dragableEquipmentUI))
                    {
                        onEquipmentChanged?.Invoke();
                        return true;
                    }

                }
            }

            Destroy(dragableEquipmentUI.gameObject);

            return false;
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

            foreach(EquipmentContainer container in containers)
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
