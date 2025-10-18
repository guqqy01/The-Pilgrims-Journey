using Equipment;
using Map;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using Random = UnityEngine.Random;
using static UnityEngine.ParticleSystem;
using GameManagerCalls;
using System.Linq;

public class ShopManager : MonoBehaviour, ISingletonToBeInitiatedFromRoot, ISaveLoad, IOnSave, IOnLoadAfterMapPointSceneLoad
{
    public static ShopManager Instance;

    [SerializeField]
    private EquipmentInventory equipmentInventory;
    public EquipmentInventory EquipmentInventory => equipmentInventory;

    [SerializeField]
    private int minAmountOfDifferentItems = 3;

    [SerializeField]
    private List<EquipmentSO> equipmentAbleToBeSold;

    public Dictionary<EquipmentSO, ShopItem> EquipmentAndTheirShopItems { private set; get; } = new Dictionary<EquipmentSO, ShopItem>();

    public string SaveFileName => "ShopManager";

    public delegate void OnShopSetup(Dictionary<EquipmentSO, ShopItem> equipmentAndTheirShopItems);
    public OnShopSetup onShopSetup;

    public delegate void OnPurchaseOrSell(ShopItem selectedShopItem);
    public OnPurchaseOrSell onPurchase;
    public OnPurchaseOrSell onSell;

    public delegate void OnUpdateSellGold();
    public OnUpdateSellGold onUpdateSellGold;


    public void InitiateSingleton()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        equipmentInventory.onEquipmentChanged -= UpdateSellGoldAmount;
        equipmentInventory.onEquipmentChanged += UpdateSellGoldAmount;
    }

    private void UpdateSellGoldAmount(EquipmentContainer equipmentContainer, EquipmentSO from, EquipmentSO to)
    {
        onUpdateSellGold?.Invoke();
    }

    public void SetupShop()
    {
        Random.InitState(GlobalMapGenerator.Instance.CurrentMapPoint.Seed);

        Dictionary<EquipmentSO, ShopItem> equipmentAndTheirShopItems = new Dictionary<EquipmentSO, ShopItem>();

        int attempts = 0;
        const int maxAttempts = 50;

        while (equipmentAndTheirShopItems.Count < minAmountOfDifferentItems && attempts < maxAttempts)
        {
            attempts++;
            equipmentAndTheirShopItems.Clear();

            foreach (EquipmentSO equipment in equipmentAbleToBeSold)
            {
                float evaluateTime = GlobalMapGenerator.Instance.CurrentPosition.x / GlobalMapGenerator.Instance.GenerationSettings.MapSize.x;
                float lerpFactor = Random.Range(0.0f, 1.0f);

                if (Random.Range(0f, 1f) <= equipment.ChanceOfBeginInShopAtXMapPosition.Evaluate(evaluateTime, lerpFactor))
                {
                    //Item Is in shop

                    int amountOfItems = equipment.IfInShopHowManyRange.RandomBetweenTwoValuesInclusive();

                    if (equipmentAndTheirShopItems.TryGetValue(equipment, out var item))
                        item.StockAmount += amountOfItems;
                    else
                    {
                        float discount = equipment.discounts.GetRandom();
                        ShopItem shopItem = new ShopItem(equipment, amountOfItems, discount);
                        equipmentAndTheirShopItems[equipment] = shopItem;
                    }
                }
            }
        }

        if (equipmentAndTheirShopItems.Count == 0)
            Debug.LogWarning("Shop generation failed to populate any items.");

        Clear();

        EquipmentAndTheirShopItems = equipmentAndTheirShopItems;

        onShopSetup?.Invoke(EquipmentAndTheirShopItems);
    }

    private void Clear()
    {
        EquipmentAndTheirShopItems.Clear();
    }

    public void Purchase(ShopItem shopItem)
    {
        if (shopItem?.Equipment == null)
            return;

        if (!EquipmentAndTheirShopItems.TryGetValue(shopItem.Equipment, out var item))
            return;

        if (item.StockAmount <= 0)
            return;

        int cost = item.GetPurchaseGoldAmount();
        if (ResourceManager.Instance.Gold < cost)
            return;

        if (!equipmentInventory.TryAddEquipment(shopItem.Equipment))
            return;

        ResourceManager.Instance.ModifyGold(-cost);

        item.FromOriginalStockDifference--;
        item.StockAmount--;
        onPurchase?.Invoke(item);
    }

    public bool CanPurchase(ShopItem item)
    {
        if (item == null || item.Equipment == null)
            return false;

        if (!EquipmentAndTheirShopItems.TryGetValue(item.Equipment, out var shop))
            return false;

        return shop.StockAmount > 0 &&
               ResourceManager.Instance.Gold >= shop.GetPurchaseGoldAmount() &&
               equipmentInventory.HasFreeSpot();
    }

    public void Sell()
    {
        float sellGold = 0f;

        bool updateShopItems = false;

        foreach (EquipmentSO equipment in equipmentInventory.NonNullEquipment)
        {
            if (EquipmentAndTheirShopItems.TryGetValue(equipment, out ShopItem shopItem))
            {
                shopItem.StockAmount++;
            }
            else
            {
                shopItem = new ShopItem(equipment, 1);

                EquipmentAndTheirShopItems.Add(equipment, shopItem);

                updateShopItems = true;
            }

            sellGold += shopItem.GetSellGoldAmount();

            shopItem.FromOriginalStockDifference++;

            onSell?.Invoke(shopItem);
        }

        if(updateShopItems)
            onShopSetup?.Invoke(EquipmentAndTheirShopItems);

        equipmentInventory.Clear();

        ResourceManager.Instance.ModifyGold(Mathf.RoundToInt(sellGold));
    }

    public int GetSellGoldAmount()
    {
        float sellGoldTotal = 0f;

        Dictionary<EquipmentSO, int> equipmentAndSold = new Dictionary<EquipmentSO, int>();

        foreach (KeyValuePair<EquipmentSO, ShopItem> equipmentAndShopItem in EquipmentAndTheirShopItems)
        {
            equipmentAndSold.Add(equipmentAndShopItem.Key, equipmentAndShopItem.Value.FromOriginalStockDifference);
        }

        foreach (EquipmentSO equipment in equipmentInventory.NonNullEquipment)
        {
            if (!EquipmentAndTheirShopItems.TryGetValue(equipment, out ShopItem shopItem))
            {
                sellGoldTotal += equipment.goldCost / 2f;
                continue;
            }

            bool wasRecentlyBought = equipmentAndSold.TryGetValue(equipment, out int diff) && diff < 0;

            sellGoldTotal += wasRecentlyBought
                ? shopItem.GetSellGoldAmount()
                : shopItem.GetNormalPriceWithoutMultiplier() / 2f;

            if (wasRecentlyBought)
                equipmentAndSold[equipment]++;
        }

        return Mathf.RoundToInt(sellGoldTotal);
    }


    public void Save()
    {
        SaveManager.IfFileExistsDelete(SaveFileName);
        SaveData saveData = new SaveData(EquipmentAndTheirShopItems.Values.ToList());
        SaveProfile<SaveData> saveProfile = new SaveProfile<SaveData>(SaveFileName, saveData);
        SaveManager.SaveOver(saveProfile);
    }

    public void LoadAfterMapPointSceneLoad()
    {
        if (SaveManager.TryLoad(SaveFileName, out SaveProfile<SaveData> saveProfile))
        {
            SaveData saveData = saveProfile.saveData;

            List<ShopItem.ShopItemSave> shopItemSaves = saveData.shopItemSaves;

            Dictionary<EquipmentSO, ShopItem> shopItems = new Dictionary<EquipmentSO, ShopItem>();

            foreach (var shopItemSave in shopItemSaves)
            {
                ShopItem shopItem = shopItemSave.GetShopItem();
                if (shopItem.Equipment != null && !shopItems.ContainsKey(shopItem.Equipment))
                    shopItems.Add(shopItem.Equipment, shopItem);
            }

            EquipmentAndTheirShopItems = shopItems;

            onShopSetup?.Invoke(EquipmentAndTheirShopItems);
        }
    }

    [Serializable]
    private record SaveData : SaveProfileData
    {
        public List<ShopItem.ShopItemSave> shopItemSaves;

        public SaveData(List<ShopItem> shopItems)
        {
            List<ShopItem.ShopItemSave> shopItemSaves = new List<ShopItem.ShopItemSave>();

            foreach (ShopItem shopItem in shopItems)
            {
                shopItemSaves.Add(new ShopItem.ShopItemSave(shopItem));
            }

            this.shopItemSaves = shopItemSaves;
        }
    }
}

