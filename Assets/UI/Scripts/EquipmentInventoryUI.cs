using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Equipment.EquipmentInventory;

namespace Equipment
{
    public class EquipmentInventoryUI : MonoBehaviour
    {
        [SerializeField]
        protected EquipmentInventory equipmentInventory;

        private EquipmentContainerUI containerUIPrefab;

        protected List<EquipmentContainerUI> containerUIs;

        private void Start()
        {
            SetupNumberOfContainers();
        }

        private void SetupNumberOfContainers()
        {
            containerUIs = GetComponentsInChildren<EquipmentContainerUI>().ToList();

            if(containerUIs.Count > equipmentInventory.Containers.Length)
            {
                for (int i = equipmentInventory.Containers.Length; i < containerUIs.Count; i++)
                {
                    Destroy(containerUIs[i].gameObject);
                }
            }
            else if(containerUIs.Count < equipmentInventory.Containers.Length)
            {
                EquipmentContainerUI containerUI = Instantiate(containerUIPrefab, transform);
                containerUIs.Add(containerUI);
            }

            containerUIs = containerUIs.Where(x => x != null).ToList();

            for (int i = 0; i < equipmentInventory.Containers.Length; i++)
            {
                EquipmentContainer equipmentContainer = equipmentInventory.Containers[i];
                EquipmentContainerUI containerUI = containerUIs[i];

                containerUI.Init(equipmentContainer, this);
            }
        }

        public void Clear()
        {
            if (containerUIs == null)
                return;

            for (int i = 0; i < containerUIs.Count; i++)
            {
                containerUIs[i].Clear();
            }
        }
    }

    
}
