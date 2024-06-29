using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Units;
using System;
using Equipment;
using Tiles;

public class PrizeWindow : MonoBehaviour, ISaveLoad, ISingletonToBeInitiatedFromRoot
{
    public static PrizeWindow Instance;

    public string SaveFileName => "PrizeWindowSave";

    [SerializeField]
    private DragableEquipmentUI dragableEquipmentUIPrefab;

    [SerializeField]
    private Text prizeText;

    private int currentPageIndex = 0;
    private int LastPageIndex => PrizePages.Count - 1;

    [SerializeField]
    private TileGrid tileGrid;

    [SerializeField]
    private Transform equipmentPagesTransfrom;

    [SerializeField]
    private EquipmentPage equipmentPagePrefab;
    private readonly List<EquipmentPage> equipmentPages = new List<EquipmentPage>();

    private List<List<ContainerSave>> containerSaves = new List<List<ContainerSave>>();

    [SerializeField]
    private Text skipText;

    [SerializeField]
    private Button skipButton;
    [SerializeField]
    private Button backButton;



    public void InitiateSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void TileGridInit(List<TileAndUnit> tiles)
    {
        tileGrid.Clear();

        tileGrid.PlaceTiles(tiles);
    }

    List<IPrizePage> PrizePages
    {
        get
        {
            List<IPrizePage> returnPages = new List<IPrizePage>();

            returnPages.AddRange(equipmentPages);
            return returnPages;
        }
    }

    private IPrizePage currentPage 
    {
        get 
        {
            if(PrizePages.Count == 0 || currentPageIndex >= PrizePages.Count)
                return null;

            return PrizePages[currentPageIndex];
        }
    }

    public void DestroyEquipmentPages()
    {
        foreach(EquipmentPage page in equipmentPages)
        {
            Destroy(page.gameObject);
        }

        equipmentPages.Clear();
    }

    public void LoadPrize(List<List<EquipmentSO>> equipmentLists, List<TileAndUnit> tiles)
    {
        containerSaves = new List<List<ContainerSave>>();

        foreach (List<EquipmentSO> equipmentList in equipmentLists)
        {
           List<ContainerSave> listOfContainerSaves = new List<ContainerSave>();

            foreach (EquipmentSO equipmentSO in equipmentList)
            {
                ContainerSave save = new ContainerSave(equipmentSO, equipmentSO, true);
                listOfContainerSaves.Add(save);
            }

            containerSaves.Add(listOfContainerSaves);
        }

        LoadEquipmentPrize();
        LoadTilePrize(tiles);
        
        currentPageIndex = 0;

        RefreshUI();
    }


    private void LoadEquipmentPrize()
    {
        DestroyEquipmentPages();

        if (containerSaves == null)
            return;

        for (int i = 0; i < containerSaves.Count; i++)
        {
            EquipmentPage equipmentPage = Instantiate(equipmentPagePrefab, equipmentPagesTransfrom);
            equipmentPage.Init(containerSaves[i], dragableEquipmentUIPrefab);
            equipmentPages.Add(equipmentPage);
        }

        currentPageIndex = 0;

        RefreshUI();
    }

    private void LoadTilePrize(List<TileAndUnit> tiles)
    {
        tileGrid.Clear();

        TileGridInit(tiles);

        RefreshUI();
    }


    public void GoNext()
    {
        currentPage?.Close();

        currentPageIndex = Mathf.Clamp(currentPageIndex + 1, 0, LastPageIndex);

        RefreshUI();
    }

    public void GoPrevious()
    {
        currentPage?.Close();

        currentPageIndex = Mathf.Clamp(currentPageIndex - 1, 0, LastPageIndex);

        RefreshUI();
    }

    private void RefreshUI()
    {
        currentPage?.Open();

        prizeText.text = "Prize" + ((LastPageIndex > 1) ? " " + (currentPageIndex + 1) + "/" + (LastPageIndex + 1) : "");

        if (PrizePages.Count <= 0)
            return;

        backButton.interactable = currentPageIndex != 0;
        skipButton.interactable = currentPageIndex != LastPageIndex;
    }

    public void Save()
    {
        SaveManager.IfFileExistsDelete(SaveFileName);

        List<EquipmentInventorySave> saves = new List<EquipmentInventorySave>();

        foreach(EquipmentPage page in equipmentPages)
        {
            saves.Add(page.GetEquipmentInventorySave());
        }

        SaveData saveData = new SaveData(saves);
        SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
        SaveManager.SaveOver(saveProfile);
    }

    public void Load()
    {
        SaveData saveData = SaveManager.Load<SaveData>(SaveFileName).saveData;

        List<List<ContainerSave>> saves = new List<List<ContainerSave>>();

        foreach (EquipmentInventorySave save in saveData.containerSaves)
        {
            saves.Add(save.equipmentSaves);
        }

        containerSaves = saves;

        LoadEquipmentPrize();

        foreach (IPrizePage page in PrizePages)
        {
            page.Close();
        }

        currentPageIndex = 0;

        RefreshUI();
    }

    

    [Serializable]
    private record SaveData : SaveProfileData
    {
        public List<EquipmentInventorySave> containerSaves;
        public SaveData(List<EquipmentInventorySave> containerSaves)
        {
            this.containerSaves = containerSaves;
        }
    }
}

[Serializable]
public class TileAndUnit: IComparable<TileAndUnit>
{
    public TileSO tile;
    public UnitSO unit;

    public TileAndUnit(TileSO tile, UnitSO unit)
    {
        this.tile = tile;
        this.unit = unit;
    }
    

    public int CompareTo(TileAndUnit other)
    {
        return - tile.Shape.Count.CompareTo(other.tile.Shape.Count);
    }
}