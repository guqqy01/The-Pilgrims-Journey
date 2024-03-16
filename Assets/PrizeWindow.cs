using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrizeWindow : MonoBehaviour
{
    public PrizeWindow instance;

    public List<EquipmentContainer> equipmentContainers;

    private EquipmentSO takenEquipment;

    [SerializeField]
    private Transform equipmentContainersHolder;

    [SerializeField]
    private EquipmentContainer equipmentContainerPrefab;

    [SerializeField]
    private List<Tile> tilePrefabs;

    private void Awake()
    {
        instance = this;
    }

    public void LoadPrize(int numberOfEquipment)
    {
        GetRightAmountOfEquipmentContainers(numberOfEquipment);


         

    }

    private void GetRightAmountOfEquipmentContainers(int numberOfEquipment)
    {
        int difference = numberOfEquipment - equipmentContainers.Count;

        if (difference == 0)
            return;

        if(difference > 0)
        {
            for (int i = 0; i < difference; i++)
            {
                EquipmentContainer container = Instantiate(equipmentContainerPrefab, equipmentContainersHolder);
                container.onSelect += OnContainerSelected;
                equipmentContainers.Add(container);
            }
        }
        else
        {
            for (int i = 0; i < - difference; i++)
            {
                Destroy(equipmentContainers[equipmentContainers.Count - 1 - i].gameObject);
            }

            equipmentContainers.RemoveAll(null);
        }
    }

    private void OnContainerSelected(EquipmentContainer selected, bool isSelected)
    {
        if (isSelected)
        {
            takenEquipment = selected.EquipmentSO;
            foreach (EquipmentContainer equipmentContainer in equipmentContainers)
            {
                equipmentContainer.Active = equipmentContainer == selected;
            }
        }
        else
        {
            if (selected.EquipmentSO != null)
            {

            }
        }
    }
}
