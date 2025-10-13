using UnityEngine;
using static Equipment.EquipmentContainerUI;

namespace Equipment
{
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

                onAllowedEquipmentChange?.Invoke();
            }
        }

        private bool active = true;
        public bool Active
        {
            get { return active; }
            set
            {
                active = value;
                onActiveChange?.Invoke();
            }
        }

        public delegate void OnEquipmentChanged(EquipmentSO fromEquipment, EquipmentSO toEquipment);
        public OnEquipmentChanged onEquipmentChanged;

        public delegate void OnUIUpdate();
        public OnUIUpdate onActiveChange;
        public OnUIUpdate onAllowedEquipmentChange;

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
            onEquipmentChanged = null;
            Active = false;
            Clear();
            TryChangeEquipment(null);
        }

        public void Clear()
        {
            TryChangeEquipment(null);
        }

        public bool TrySwapEquipment(EquipmentContainer otherContainer)
        {
            EquipmentSO otherEquipment = otherContainer.EquipmentSO;
            EquipmentSO previousEquipment = EquipmentSO;

            if (CanChangeEquipment(otherEquipment) && otherContainer.CanChangeEquipment(previousEquipment))
            {
                TryChangeEquipment(otherEquipment);
                otherContainer.TryChangeEquipment(previousEquipment);

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
