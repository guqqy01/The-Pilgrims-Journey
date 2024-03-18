using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrizeWindow : MonoBehaviour
{
    public static PrizeWindow instance;

    public List<PrizeContainer> prizeContainers;

    [SerializeField]
    private Transform prizeContainersHolder;

    [SerializeField]
    private PrizeContainer prizeContainerPrefab;

    [SerializeField]
    private List<Tile> tilePrefabs;

    [SerializeField]
    private List<EquipmentSO> equipment;

    public int test = 3;

    private void Awake()
    {
        instance = this;
    }

    public void LoadPrize(int numberOfEquipment)
    {
        SpawnPrizeContainers(numberOfEquipment);

        foreach (PrizeContainer container in prizeContainers)
        {
            if (equipment.Count == 0)
                break;

            int random = Random.Range(0, equipment.Count);

            container.EquipmentSO = equipment[random];
        }
    }

    private void SpawnPrizeContainers(int numberOfPrizes)
    {
        for (int i = 0; i < prizeContainers.Count; i++)
        {
            Destroy(prizeContainers[i].gameObject);
        }

        prizeContainers.Clear();

        for (int i = 0; i < numberOfPrizes; i++)
        {
            PrizeContainer container = Instantiate(prizeContainerPrefab, prizeContainersHolder);
            container.onEquipmentChange += OnEquipmentChanged;
            prizeContainers.Add(container);
        }
        
    }

    private void OnEquipmentChanged(EquipmentSO from, EquipmentSO to)
    {
        if (to == null)
        {
            foreach (PrizeContainer equipmentContainer in prizeContainers)
            {
                equipmentContainer.Active = equipmentContainer.EquipmentSO == null;
            }
        }
        else
        {
            foreach (PrizeContainer equipmentContainer in prizeContainers)
            {
                equipmentContainer.Active = true;
            }
        }
    }
}
