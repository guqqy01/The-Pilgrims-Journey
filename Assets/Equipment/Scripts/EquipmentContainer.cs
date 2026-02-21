using GameManagerCalls;
using System;
using UnityEngine;

namespace Equipment
{
    [Serializable]
    public class EquipmentContainer
    {
        public EquipmentInventory ParentInventory { get; private set; }

        private EquipmentSO equipmentSO = null;
        public EquipmentSO EquipmentSO
        {
            get { return equipmentSO; }
            set
            {
                EquipmentSO previousEquipment = equipmentSO;
                equipmentSO = value;
                onEquipmentChanged?.Invoke(previousEquipment, value);
                onEquipmentChangeUI?.Invoke();
            }
        }

        private EquipmentSO allowedEquipment = null;
        public EquipmentSO AllowedEquipment
        {
            get { return allowedEquipment; }
            set
            {
                allowedEquipment = value;

                if (EquipmentSO != null && EquipmentSO != allowedEquipment)
                {
                    EquipmentSO = null;
                }

                onAllowedEquipmentChangeUI?.Invoke();
            }
        }

        private bool active = true;
        public bool Active
        {
            get { return active; }
            set
            {
                active = value;
                onActiveChangeUI?.Invoke();
            }
        }

        public delegate void OnEquipmentChanged(EquipmentSO fromEquipment, EquipmentSO toEquipment);
        public OnEquipmentChanged onEquipmentChanged;

        public delegate void OnUIUpdate();
        public OnUIUpdate onActiveChangeUI;
        public OnUIUpdate onAllowedEquipmentChangeUI;
        public OnUIUpdate onEquipmentChangeUI;

        public EquipmentContainer(ContainerSave containerSave, EquipmentInventory parentInventory)
        {
            ParentInventory = parentInventory;
            EquipmentSO = containerSave.equipment;
            AllowedEquipment = containerSave.allowedEquipment;
            Active = containerSave.active;
        }

        public EquipmentContainer(EquipmentInventory parentInventory)
        {
            ParentInventory = parentInventory;
            EquipmentSO = null;
            AllowedEquipment = null;
            Active = true;
        }

        public void SetupEquipment(EquipmentSO equipment)
        {
            EquipmentSO = equipment;

            onEquipmentChanged?.Invoke(null, equipment);
        }

        public bool TryChangeEquipment(EquipmentSO equipment)
        {
            if (CanChangeEquipment(equipment))
            {
                EquipmentSO previousEquipment = EquipmentSO;

                EquipmentSO = equipment;

                onEquipmentChanged?.Invoke(previousEquipment, equipment);

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CanChangeEquipment(EquipmentSO equipment)
        {
            if (equipment == null)
                return true;

            if (!Active)
                return false;

            if (AllowedEquipment != null && AllowedEquipment != equipment)
                return false;

            return true;
        }

        public void Deactivate()
        {
            Active = false;
            ClearEquipment();
        }

        public void ClearEquipment()
        {
            TryChangeEquipment(null);
        }

        public bool TrySwapEquipment(EquipmentContainer otherContainer)
        {
            EquipmentSO otherEquipment = otherContainer.EquipmentSO;
            EquipmentSO previousEquipment = EquipmentSO;


            if (CanChangeEquipment(otherEquipment) && otherContainer.CanChangeEquipment(previousEquipment))
            {
                if (!TryChangeEquipment(otherEquipment))
                {
                    return false;
                }
                if (!otherContainer.TryChangeEquipment(previousEquipment))
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        public ContainerSave GetSave()
        {
            return new ContainerSave(EquipmentSO, AllowedEquipment, Active);
        }
    }

}
