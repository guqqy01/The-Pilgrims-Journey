using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Units;
using UnityEngine.UI;

public class EvolutionWindow : MonoBehaviour
{
    [SerializeField]
    private Image unitImage;

    [SerializeField]
    private List<EquipmentContainer> equipmentContainers;

    [SerializeField]
    private UnitPercentageEvolution unitPercentageEvolutionPrefab;

    [SerializeField]
    private Transform unitPerEvoHolder;
    
    private List<UnitPercentageEvolution> unitPercentageEvolutions = new List<UnitPercentageEvolution>();

    private Tile tile;

    private UnitManager unitManager;

    private void Awake()
    {
        foreach (EquipmentContainer container in equipmentContainers)
        {
            container.onEquipmentChange += OnEquipmentChanged;
        }
    }

    private void Start()
    {
        unitManager = UnitManager.Instance;
    }

    public void Init(Tile tile)
    {
        unitImage.sprite = tile.Unit.sprite;
        this.tile = tile;
    }

    private void OnEquipmentChanged(EquipmentSO from, EquipmentSO to)
    {
        List<EquipmentSO> equipment = new List<EquipmentSO>();
        for (int i = 0; i < equipmentContainers.Count; i++)
        {
            if (equipmentContainers[i].EquipmentSO != null)
                equipment.Add(equipmentContainers[i].EquipmentSO);
        }

        WeightedChanceList<UnitSO> weightedList = tile.Unit.GetEvolutionUnitsAndPercentage(equipment);
        weightedList.RemoveZeros();
        SetupUnitPercentage(weightedList);
    }

    private void SetupUnitPercentage(WeightedChanceList<UnitSO>  weightedList)
    {
        weightedList.RemoveZeros();

        foreach (UnitPercentageEvolution upe in unitPercentageEvolutions)
        {
            Destroy(upe.gameObject);
        }

        unitPercentageEvolutions = new List<UnitPercentageEvolution>();

        Dictionary<UnitSO, float> unitsAndPercentages = weightedList.GetItemAndPercentages();

        foreach(KeyValuePair<UnitSO, float> kvp in unitsAndPercentages)
        {
            UnitPercentageEvolution item = Instantiate(unitPercentageEvolutionPrefab, unitPerEvoHolder);
            item.Init(kvp.Key, (kvp.Value * 100f).ToString());
            unitPercentageEvolutions.Add(item);
        }
    }

    public void Evolve()
    {
        List<EquipmentSO> equipment = new List<EquipmentSO>();
        for (int i = 0; i < equipmentContainers.Count; i++)
        {
            if (equipmentContainers[i].EquipmentSO != null)
                equipment.Add(equipmentContainers[i].EquipmentSO);
        }

        WeightedChanceList<UnitSO> weightedList = tile.Unit.GetEvolutionUnitsAndPercentage(equipment);

        tile.Unit = weightedList.GetRandom();

        for (int i = 0; i < equipmentContainers.Count; i++)
        {
            equipmentContainers[i].TryChangeEquipment(null);
        }
    }
}
