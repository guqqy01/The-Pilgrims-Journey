using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Equipment;
using System.Linq;
using UnityEngine.UIElements;

public class EquipmentPage : EquipmentInventoryUI, IPrizePage
{
    [SerializeField]
    private EquipmentContainerUI prizeContainerPrefab;

    [SerializeField]
    private Transform prizeContainersHolder;

    public int Value => (int)Reward.Equipment;

    private const int maxNumOfEquipment = 3;

    public void Init(List<ContainerSave> saves)
    {
        SpawnPrizeContainers(saves);

        gameObject.SetActive(false);
    }

    private void DestroyPrizeContainers()
    {
        if (containerUIs == null)
            containerUIs = new List<EquipmentContainerUI>();

        for (int i = 0; i < containerUIs.Count; i++)
        {
            Destroy(containerUIs[i].gameObject);
        }

        containerUIs.Clear();
    }


    private void SpawnPrizeContainers(List<ContainerSave> containerSaves)
    {
        DestroyPrizeContainers();

        foreach (ContainerSave save in containerSaves)
        {
            EquipmentContainerUI container = Instantiate(prizeContainerPrefab, prizeContainersHolder);
            containerUIs.Add(container);
        }

        Setup(containerSaves);

        foreach (EquipmentContainer container in equipmentInventory.Containers)
        {
            container.onEquipmentChanged += OnEquipmentChanged;
        }
    }

    private new void OnEquipmentChanged(EquipmentSO from, EquipmentSO to)
    {
        if (to == null)
        {
            int? index = null;

            for (int i = 0; i < equipmentInventory.Containers.Length; i++)
            {
                equipmentInventory.Containers[i].Active = equipmentInventory.Containers[i].EquipmentSO == null;

                if (equipmentInventory.Containers[i].EquipmentSO == null)
                {
                    index = i;
                }
            }
        }
        else
        {
            foreach (EquipmentContainer equipmentContainer in equipmentInventory.Containers)
            {
                equipmentContainer.Active = true;
            }
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
