using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Units;
using UnityEngine;
using UnityEngine.UI;

namespace Equipment
{
    public class EquipmentCatalogue : MonoBehaviour
    {
        [SerializeField]
        private Button leftButton;
        [SerializeField]
        private Button rightButton;

        [SerializeField]
        private EquipmentCatalogueItem catalogueItem;

        private EquipmentManager equipmentManager;

        private void Start()
        {
            equipmentManager = EquipmentManager.Instance;

            List<EquipmentSO> equipment = equipmentManager.EquipmentStates.Keys.ToList();

            catalogueItem.Setup(equipment[0]);

            leftButton.interactable = false;
            rightButton.interactable = 0 >= 0 && 0 < equipment.Count - 1;
        }

        public void RightButtonPress()
        {
            List<EquipmentSO> units = equipmentManager.EquipmentStates.Keys.ToList();
            int index = units.IndexOf(catalogueItem.Equipment);

            if (index >= 0 && index < units.Count - 1)
                catalogueItem.Setup(units[index + 1]);

            leftButton.interactable = index + 1 > 0 && index + 1 <= units.Count - 1;
            rightButton.interactable = index + 1 >= 0 && index + 1 < units.Count - 1;
        }

        public void LeftButtonPress()
        {
            List<EquipmentSO> units = equipmentManager.EquipmentStates.Keys.ToList();
            int index = units.IndexOf(catalogueItem.Equipment);

            if (index > 0 && index <= units.Count - 1)
                catalogueItem.Setup(units[index - 1]);

            leftButton.interactable = index - 1 > 0 && index - 1 <= units.Count - 1;
            rightButton.interactable = index - 1 >= 0 && index - 1 < units.Count - 1;
        }
    }

}
