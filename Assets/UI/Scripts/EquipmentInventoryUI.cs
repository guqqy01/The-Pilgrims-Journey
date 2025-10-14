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

        [SerializeField]
        private EquipmentContainerUI containerUIPrefab;

        protected List<EquipmentContainerUI> containerUIs;

        [SerializeField]
        private Transform manualContainerParentTransform;

        public Transform containerParentTransform => manualContainerParentTransform != null ? manualContainerParentTransform : transform;

        private void Start()
        {
            equipmentInventory.onContainerSizeChanged += SetupNumberOfContainers;

            SetupNumberOfContainers(equipmentInventory.Containers.Length);
        }

        private void SetupNumberOfContainers(int numberOfContainers)
        {
            containerUIs = containerParentTransform.GetComponentsInChildren<EquipmentContainerUI>().ToList();

            if(containerUIs.Count > numberOfContainers)
            {
                for (int i = numberOfContainers; i < containerUIs.Count; i++)
                {
                    Destroy(containerUIs[i].gameObject);
                }
            }
            else if(containerUIs.Count < numberOfContainers)
            {
                for (int i = containerUIs.Count; i < numberOfContainers; i++)
                {
                    EquipmentContainerUI containerUI = Instantiate(containerUIPrefab, containerParentTransform);
                    containerUIs.Add(containerUI);
                }
            }

            containerUIs = containerUIs.Where(x => x != null).ToList();

            for (int i = 0; i < numberOfContainers; i++)
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
