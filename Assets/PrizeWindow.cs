using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Units;

public class PrizeWindow : MonoBehaviour
{
    public static PrizeWindow instance;

    private List<EquipmentContainer> prizeContainers = new List<EquipmentContainer>();

    [SerializeField]
    private Transform prizeContainersHolder;

    [SerializeField]
    private EquipmentContainer prizeContainerPrefab;

    [SerializeField]
    private List<Tile> tilePrefabs;

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

    public bool TileTaken = false;

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
        SpawnPrizeContainers(Random.Range(1, 4));

        foreach (EquipmentContainer container in prizeContainers)
        {
            if (equipment.Count == 0)
                break;

            container.TryChangeEquipment(equipment.GetRandom());
            container.AllowedEquipment = container.EquipmentSO;
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
        }
        else
        {
            foreach (EquipmentContainer equipmentContainer in prizeContainers)
            {
                equipmentContainer.Active = true;
            }
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

        backButton.interactable = currentPage != 0;
    }
}

public enum PrizeType
{
    Equipment,
    Tile,
}