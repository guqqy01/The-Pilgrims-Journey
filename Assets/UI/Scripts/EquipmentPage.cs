using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Equipment;
using System.Linq;
using UnityEngine.UIElements;

public class EquipmentPage : EquipmentInventory, IPrizePage
{
    public void Init(List<ContainerSave> saves)
    {
        SpawnPrizeContainers(saves);

        gameObject.SetActive(false);
    }


    private void SpawnPrizeContainers(List<ContainerSave> containerSaves)
    {
        InventorySize = containerSaves.Count;

        Setup(containerSaves);

        for (int i = 0; i < Containers.Length; i++)
        {
            Containers[i].AllowedEquipment = Containers[i].EquipmentSO;
        }

        onEquipmentChanged += EquipmentChanged;
    }

    private void EquipmentChanged(EquipmentContainer container, EquipmentSO from, EquipmentSO to)
    {
        if (to == null)
        {
            for (int i = 0; i < Containers.Length; i++)
            {
                Containers[i].Active = Containers[i].EquipmentSO == null;
            }
        }
        else
        {
            foreach (EquipmentContainer equipmentContainer in Containers)
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
