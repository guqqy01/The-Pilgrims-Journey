using Equipment;
using Map;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using Random = UnityEngine.Random;
using static UnityEngine.ParticleSystem;

public class ShopManager : MonoBehaviour, ISingletonToBeInitiatedFromRoot
{
    public static ShopManager Instance;

    [SerializeField]
    private EquipmentInventory equipmentInventory;

    [SerializeField]
    private int minAmountOfDifferentItems = 3;

    [SerializeField]
    private List<EquipmentSO> equipmentAbleToBeSold;

    private ShopItem selectedShopItem;

    public Dictionary<EquipmentSO, ShopItem> EquipmentAndTheirShopItems { private set; get; } = new Dictionary<EquipmentSO, ShopItem>();

    public Dictionary<EquipmentSO, int> BoughtEquipment { private set; get; } = new Dictionary<EquipmentSO, int>();
    public Dictionary<EquipmentSO, int> SoldEquipment { private set; get; } = new Dictionary<EquipmentSO, int>();

    
    public delegate void OnShopSetup(Dictionary<EquipmentSO, ShopItem> equipmentAndTheirShopItems);
    public OnShopSetup onShopSetup;

    public delegate void OnPurchaseOrSell(ShopItem selectedShopItem);
    public OnPurchaseOrSell onPurchase;
    public OnPurchaseOrSell onSell;

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

    public void SetupShop()
    {
        Random.InitState(GlobalMapGenerator.Instance.CurrentMapPoint.Seed);

        Dictionary<EquipmentSO, ShopItem> equipmentAndTheirShopItems = new Dictionary<EquipmentSO, ShopItem>();

        while (equipmentAndTheirShopItems.Count < minAmountOfDifferentItems)
        {
            equipmentAndTheirShopItems.Clear();

            foreach (EquipmentSO equipment in equipmentAbleToBeSold)
            {
                float evaluateTime = GlobalMapGenerator.Instance.CurrentPosition.x / GlobalMapGenerator.Instance.GenerationSettings.MapSize.x;
                float lerpFactor = Random.Range(0.0f, 1.0f);

                if (Random.Range(0f, 1f) <= equipment.ChanceOfBeginInShopAtXMapPosition.Evaluate(evaluateTime, lerpFactor))
                {
                    //Item Is in shop

                    int amountOfItems = equipment.IfInShopHowManyRange.RandomBetweenTwoValuesInclusive();

                    if (equipmentAndTheirShopItems.ContainsKey(equipment))
                    {
                        equipmentAndTheirShopItems[equipment].StockAmount += amountOfItems;
                    }
                    else
                    {
                        equipmentAndTheirShopItems.Add(equipment, new ShopItem(equipment, amountOfItems));
                    }
                }
            }
        }

        Clear();

        EquipmentAndTheirShopItems = equipmentAndTheirShopItems;

        onShopSetup?.Invoke(EquipmentAndTheirShopItems);
    }

    private void Clear()
    {
        EquipmentAndTheirShopItems.Clear();
        BoughtEquipment.Clear();
        SoldEquipment.Clear();
    }

    public void Purchase(EquipmentSO equipment)
    {
        if (equipment == null)
            return;

        if (!EquipmentAndTheirShopItems.ContainsKey(equipment))
            return;

        if (EquipmentAndTheirShopItems[equipment].StockAmount <= 0)
            return;

        if (ResourceManager.Instance.Gold < EquipmentAndTheirShopItems[equipment].FinalGoldCost)
            return;

        if (!equipmentInventory.CanAddEquipment(equipment))
            return;

        if (SoldEquipment.ContainsKey(equipment))
        {
            ResourceManager.Instance.ModifyGold(-Mathf.RoundToInt(selectedShopItem.FinalGoldCost / 2f));
            SoldEquipment[equipment]--;

            if (SoldEquipment[equipment] <= 0)
                SoldEquipment.Remove(equipment);
        }
        else
        {
            ResourceManager.Instance.ModifyGold(-selectedShopItem.FinalGoldCost);

            BoughtEquipment.AddOrUpdateByOne(equipment);
        }

        if (equipmentInventory.TryAddEquipment(equipment))
        {
            EquipmentAndTheirShopItems[equipment].StockAmount--;
        }

        onPurchase?.Invoke(selectedShopItem);
    }

    public void Sell()
    {
        float sellGold = 0f;

        foreach (EquipmentSO equipment in equipmentInventory.NonNullEquipment)
        {
            ShopItem shopItem;

            if (EquipmentAndTheirShopItems.ContainsKey(equipment))
            {
                shopItem = EquipmentAndTheirShopItems[equipment];

                shopItem.StockAmount++;
            }
            else
            {
                shopItem = new ShopItem(equipment, 1); 

                EquipmentAndTheirShopItems.Add(equipment, shopItem);
            }

            if (BoughtEquipment.ContainsKey(equipment))
            {
                if (EquipmentAndTheirShopItems.ContainsKey(equipment))
                {
                    sellGold += EquipmentAndTheirShopItems[equipment].FinalGoldCost;
                }
                else
                {
                    sellGold += equipment.goldCost;
                }

                BoughtEquipment[equipment]--;

                if (BoughtEquipment[equipment] <= 0)
                    BoughtEquipment.Remove(equipment);
            }
            else
            {
                if (EquipmentAndTheirShopItems.ContainsKey(equipment))
                {
                    sellGold += EquipmentAndTheirShopItems[equipment].FinalGoldCost / 2f;
                }
                else
                {
                    sellGold += equipment.goldCost / 2f;
                }

                SoldEquipment.AddOrUpdateByOne(equipment);
            }

            onSell?.Invoke(shopItem);
        }

        equipmentInventory.Clear();

        ResourceManager.Instance.ModifyGold(Mathf.RoundToInt(sellGold));
    }

    private int GetSellGoldAmount()
    {
        float sellGold = 0f;

        Dictionary<EquipmentSO, int> copyOfBoughtEquipment = new Dictionary<EquipmentSO, int>();

        foreach (KeyValuePair<EquipmentSO, int> pair in BoughtEquipment)
        {
            copyOfBoughtEquipment.Add(pair);
        }

        foreach (EquipmentSO equipment in equipmentInventory.NonNullEquipment)
        {
            if (copyOfBoughtEquipment.ContainsKey(equipment))
            {
                if (EquipmentAndTheirShopItems.ContainsKey(equipment))
                {
                    sellGold += EquipmentAndTheirShopItems[equipment].FinalGoldCost;
                }
                else
                {
                    sellGold += equipment.goldCost;
                }

                copyOfBoughtEquipment[equipment]--;

                if (copyOfBoughtEquipment[equipment] <= 0)
                    copyOfBoughtEquipment.Remove(equipment);
            }
            else
            {
                if (EquipmentAndTheirShopItems.ContainsKey(equipment))
                {
                    sellGold += EquipmentAndTheirShopItems[equipment].FinalGoldCost / 2f;
                }
                else
                {
                    sellGold += equipment.goldCost / 2f;
                }
            }
        }

        return Mathf.RoundToInt(sellGold);
    }
    


}

