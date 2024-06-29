using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Equipment;
using System.Linq;
using UnityEngine.UIElements;

public class EquipmentPage : EquipmentInventory, IPrizePage
{
    [SerializeField]
    private EquipmentContainer prizeContainerPrefab;

    [SerializeField]
    private Transform prizeContainersHolder;

    public int Value => (int)Reward.Equipment;

    private const int maxNumOfEquipment = 3;

    public void Init(List<ContainerSave> saves, DragableEquipmentUI dragableEquipmentUIPrefab)
    {
        this.dragableEquipmentUIPrefab = dragableEquipmentUIPrefab;

        SpawnPrizeContainers(saves);

        gameObject.SetActive(false);
    }

    private void DestroyPrizeContainers()
    {
        if (containers == null)
            containers = new List<EquipmentContainer>();

        for (int i = 0; i < containers.Count; i++)
        {
            Destroy(containers[i].gameObject);
        }

        containers.Clear();
    }


    private void SpawnPrizeContainers(List<ContainerSave> containerSaves)
    {
        DestroyPrizeContainers();

        foreach (ContainerSave save in containerSaves)
        {
            EquipmentContainer container = Instantiate(prizeContainerPrefab, prizeContainersHolder);
            containers.Add(container);
        }

        Setup(containerSaves);

        foreach (EquipmentContainer container in containers)
        {
            container.onEquipmentChange += OnEquipmentChanged;
        }
    }

    private void OnEquipmentChanged(EquipmentSO from, EquipmentSO to)
    {
        if (to == null)
        {
            int? index = null;

            for (int i = 0; i < containers.Count; i++)
            {
                containers[i].Active = containers[i].EquipmentSO == null;

                if (containers[i].EquipmentSO == null)
                {
                    index = i;
                }
            }
        }
        else
        {
            foreach (EquipmentContainer equipmentContainer in containers)
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
