using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Equipment;
using Tiles.UI;
using Units;
using UnityEngine.UI;
using Tiles;
public class RewardWindowUI : Singleton<RewardWindowUI>
{
    [SerializeField] private RewardWindowItemUI rewardItemUIPrefab;

    [SerializeField] private readonly List<RewardWindowItemUI> rewardItemUIs = new List<RewardWindowItemUI>();

    [SerializeField] private RectTransform itemHolder;

    [SerializeField] private DescriptionUI descriptionText;

    [SerializeField]
    private Image tileImage;

    private IDescriptable currentSelected;

    [SerializeField]
    private EquipmentInventory equipmentInventory;

    private TileSO tileSO;

    [SerializeField]
    private TileGridUI playerGrid;

    [SerializeField]
    private TileGridUI equipmentGrid;
    public override void InitiateFromRootAtAwake()
    {
        base.InitiateFromRootAtAwake();

        gameObject.SetActive(false);
    }

    private void Start()
    {
        RewardManager.WhenReady(Subscribe);
    }

    private void Subscribe(RewardManager rewardManager)
    {
        rewardManager.OnGiveEquipmentReward += Setup;
        rewardManager.OnGiveTileReward += Setup;
    }
    private void UnSubscribe(RewardManager rewardManager)
    {
        rewardManager.OnGiveEquipmentReward -= Setup;
        rewardManager.OnGiveTileReward -= Setup;
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();

        RewardManager.IfReady(UnSubscribe);
    }

    private void Clear()
    {
        tileImage.gameObject.SetActive(false);

        rewardItemUIs.DestroyAndClearList();
    }

    public void Setup(List<IDescriptable> descriptables)
    {
        gameObject.SetActive(true);

        foreach(var item in rewardItemUIs)
        {
            item.onSelectItem -= ShowItemDescription;
        }

        Clear();

        foreach (IDescriptable descriptable in descriptables)
        {
            RewardWindowItemUI rewardWindowItemUI = Instantiate(rewardItemUIPrefab, itemHolder);
            rewardWindowItemUI.Setup(descriptable);
            rewardWindowItemUI.onSelectItem += ShowItemDescription;
            rewardItemUIs.Add(rewardWindowItemUI);
        }

        if(rewardItemUIs.Count > 0)
            ShowItemDescription(rewardItemUIs[0].descriptableObject);
    }

    public void Setup(TileSO tile, List<UnitSO> unitOptions)
    {
        tileSO = tile;
        Setup(unitOptions.Cast<IDescriptable>().ToList());
        PlaceTile(tile.TileSprite);
    }

    public void PlaceTile(Sprite tileSprite)
    {
        tileImage.gameObject.SetActive(true);
        tileImage.sprite = tileSprite;
        tileImage.NativeRatioScaleHeightToSize(50f);
    }

    public void ShowItemDescription(IDescriptable descriptableObject)
    {
        if (descriptableObject == null)
            return;

        foreach(RewardWindowItemUI item in rewardItemUIs)
        {
            item.Select(item.descriptableObject == descriptableObject);
        }

        currentSelected = descriptableObject;

        descriptionText.UpdateUI(descriptableObject);
    }

   /* public void Select()
    {
        gameObject.SetActive(false);
        //RewardGainedUI.Instance.Setup(currentSelected);
        
        if(currentSelected is EquipmentSO equipment)
        {
            if (!equipmentInventory.TryAddEquipment(equipment))
            {
                //Couldnt Place Equipment.
            }
        }

        if(currentSelected is UnitSO unit)
        {
            if (!playerGrid.TryPlaceTile(new TileAndUnit(tileSO, unit)))
            {
                if(!equipmentGrid.TryPlaceTile(new TileAndUnit(tileSO, unit)))
                {
                    //Couldnt Place Tile.
                }
            }
        }
    }*/
}
