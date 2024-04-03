using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Units;
using System;

public class PrizeWindow : MonoBehaviour
{
    public static PrizeWindow instance;

    private List<EquipmentContainer> prizeContainers = new List<EquipmentContainer>();

    [SerializeField]
    private Transform prizeContainersHolder;

    [SerializeField]
    private EquipmentContainer prizeContainerPrefab;

    [SerializeField]
    private WeightedChanceList<Tile> tilePrefabs;

    [SerializeField]
    private List<EquipmentSO> equipment;

    public int test = 3;

    [SerializeField]
    private Text prizeText;

    private int currentPage = 0;
    private int lastPage;

    private List<PrizeType> prizes = new List<PrizeType>();

    [SerializeField]
    private GameObject equipmentPage;
    [SerializeField]
    private GameObject tilePage;

    public Tile prizeTile;

    public bool EquipmentTaken { private set; get; } = false;
    public bool TileTaken = false;

    [SerializeField]
    private Text skipText;

    [SerializeField]
    private Button skipButton;
    [SerializeField]
    private Button backButton;

    [SerializeField]
    private List<UnitSO> units;

    private void Awake()
    {
        instance = this;
    }

    public void LoadPrize()
    {
        SpawnPrizeContainers(UnityEngine.Random.Range(1, 4));

        List<EquipmentSO> currentEquipment = new List<EquipmentSO>();

        foreach (EquipmentContainer container in prizeContainers)
        {
            if (equipment.Count == 0)
                break;

            int breaker = 10;

            EquipmentSO equip = equipment.GetRandom();

            while (breaker > 0 && currentEquipment.Contains(equip))
            {
                equip = equipment.GetRandom();
                breaker--;
            }

            container.TryChangeEquipment(equip);
            container.AllowedEquipment = container.EquipmentSO;

            currentEquipment.Add(equip);
        }

        if (prizeTile != null && TileTaken == false)
        {
            Destroy(prizeTile.gameObject);
        }

        TileTaken = false;

        prizeTile = Instantiate(tilePrefabs.GetRandom(), tilePage.transform);
        prizeTile.baseUnit = units.GetRandom();

        prizes = new List<PrizeType>() { PrizeType.Equipment, PrizeType.Tile };
        currentPage = 0;
        lastPage = prizes.Count - 1;

        SetPages();
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
            EquipmentContainer container = Instantiate(prizeContainerPrefab, prizeContainersHolder);
            container.onEquipmentChange += OnEquipmentChanged;
            prizeContainers.Add(container);
        }
        
    }

    private void OnEquipmentChanged(EquipmentSO from, EquipmentSO to)
    {
        if (to == null)
        {
            foreach (EquipmentContainer equipmentContainer in prizeContainers)
            {
                equipmentContainer.Active = equipmentContainer.EquipmentSO == null;
            }
            EquipmentTaken = true;
        }
        else
        {
            foreach (EquipmentContainer equipmentContainer in prizeContainers)
            {
                equipmentContainer.Active = true;
            }
            EquipmentTaken = false;
        }
    }

    public void GoNext()
    {
        if(currentPage == lastPage)
        {
            gameObject.SetActive(false);
        }

        currentPage = Mathf.Clamp(currentPage + 1, 0, lastPage);
        SetPages();
    }

    public void GoPrevious()
    {
        currentPage = Mathf.Clamp(currentPage - 1, 0, lastPage);
        SetPages();
    }

    private void SetPages()
    {
        prizeText.text = "Prize " + (currentPage + 1) + "/" + (lastPage + 1);

        switch (prizes[currentPage])
        {
            case PrizeType.Equipment:
                equipmentPage.SetActive(true);
                tilePage.SetActive(false);
                break;
            case PrizeType.Tile:
                equipmentPage.SetActive(false);
                tilePage.SetActive(true);
                break;
        }

        skipText.text = (currentPage == lastPage) ? "Exit X" : "Skip >>";

        backButton.interactable = currentPage != 0;
    }
}

public enum PrizeType
{
    Equipment,
    Tile,
}

[Serializable]
public struct WeightedChanceObject<T>
{
    public T item;
    public int weight;
}

[Serializable]
public struct WeightedChanceList<T>
{
    public List<WeightedChanceObject<T>> list;

    public T GetRandom()
    {
        if (list.Count <= 0)
        {
            throw new Exception("List not populated");
        }

        int total = 0;
        foreach(WeightedChanceObject<T> obj in list)
        {
            total += obj.weight;
        }

        int index = UnityEngine.Random.Range(1, total);

        foreach (WeightedChanceObject<T> obj in list)
        {
            index -= obj.weight;
            if (index <= 0)
                return obj.item;
        }

        return list[0].item;
    }
}