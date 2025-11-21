using Equipment;
using GlobalMap;
using System.Collections;
using System.Collections.Generic;
using Tiles;
using Units;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UI;
using AYellowpaper.SerializedCollections;
using Unity.VisualScripting;
using System.Linq;
using TMPro;



[RequireComponent(typeof(EquipmentInventory))]
public class ShopUI : Singleton<ShopUI>
{
    [SerializeField]
    private ShopItemUI ShopItemPrefab;

    private Dictionary<EquipmentSO, ShopItemUI> shopItemsUIs = new Dictionary<EquipmentSO, ShopItemUI>();

    [SerializeField]
    private Transform shopItemsContentTransform;

    [SerializeField]
    private TextMeshProUGUI purchaseGoldAmount;

    [SerializeField]
    private TextMeshProUGUI sellGoldAmount;

    private ShopItemUI selectedShopItem;

    [SerializeField]
    private Button buyButton;

    [SerializeField]
    private Button sellButton;

    private void Start()
    {
        ShopManager.Instance.onShopSetup -= Setup;
        ShopManager.Instance.onUpdateSellGold -= UpdateSellGold;

        ShopManager.Instance.onShopSetup += Setup;
        ShopManager.Instance.onUpdateSellGold += UpdateSellGold;
        Clear();
        Setup(ShopManager.Instance.EquipmentAndTheirShopItems);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.onShopSetup -= Setup;
            ShopManager.Instance.onUpdateSellGold -= UpdateSellGold;
        }
    }

    private void RefreshUI()
    {
        UpdatePurchaseGoldAmount();
        UpdateSellGold();
        UpdateButtonStates();
    }

    private void Clear()
    {
        foreach (var item in shopItemsUIs.Values)
        {
            item.OnButtonPress -= ShopItemButtonPress;
            Destroy(item.gameObject);
        }
        shopItemsUIs.Clear();
    }

    public void Setup(Dictionary<EquipmentSO, ShopItem> equipmentAndStock)
    {
        // Remove UI for items no longer in the shop
        var keysToRemove = shopItemsUIs.Keys.Except(equipmentAndStock.Keys).ToList();
        foreach (var key in keysToRemove)
        {
            var shopItemUI = shopItemsUIs[key];
            shopItemUI.OnButtonPress -= ShopItemButtonPress;
            Destroy(shopItemUI.gameObject);
            shopItemsUIs.Remove(key);
        }

        // Add or update UI for current shop items
        foreach (KeyValuePair<EquipmentSO, ShopItem> kvp in equipmentAndStock)
        {
            EquipmentSO equipment = kvp.Key;
            ShopItem shopItem = kvp.Value;  

            if (!shopItemsUIs.ContainsKey(equipment))
            {
                ShopItemUI item = Instantiate(ShopItemPrefab, shopItemsContentTransform);
                item.Setup(shopItem);
                shopItemsUIs.Add(equipment, item);
                item.OnButtonPress += ShopItemButtonPress;
                continue;
            }

            if (shopItemsUIs[equipment].ShopItem == shopItem)
                continue;

            shopItemsUIs[equipment].OnButtonPress -= ShopItemButtonPress;
            shopItemsUIs[equipment].Setup(shopItem);
            shopItemsUIs[equipment].OnButtonPress += ShopItemButtonPress;
        }

        RefreshUI();
    }


    private void UpdateSellGold()
    {
        sellGoldAmount.text = ShopManager.Instance.GetSellGoldAmount().ToString();

        UpdateButtonStates();
    }

    private void ShopItemButtonPress(ShopItemUI shopItem)
    {
        if (shopItem == selectedShopItem)
            return;

        Info.Select(shopItem.Equipment);

        if(selectedShopItem != null)
        {
            selectedShopItem.Deselect();
        }

        selectedShopItem = shopItem;

        if (selectedShopItem != null)
        {
            selectedShopItem.Select();
        }

        buyButton.interactable = ShopManager.Instance.EquipmentAndTheirShopItems[shopItem.Equipment].StockAmount > 0;

        RefreshUI();
    }

    private void UpdatePurchaseGoldAmount()
    {
        if (selectedShopItem == null)
        {
            purchaseGoldAmount.text = "NA";
            return;
        }

        purchaseGoldAmount.text = selectedShopItem.ShopItem.GetPurchaseGoldAmount().ToString();
    }

    public void PurchaseButton()
    {
        ShopManager.Instance.Purchase(selectedShopItem.ShopItem);

        RefreshUI();
    }

    public void SellButton()
    {
        ShopManager.Instance.Sell();

        if(selectedShopItem != null && selectedShopItem.Equipment != null)
            buyButton.interactable = ShopManager.Instance.EquipmentAndTheirShopItems[selectedShopItem.Equipment].StockAmount > 0;

        RefreshUI();
    }

    private void UpdateButtonStates()
    {
        buyButton.interactable = selectedShopItem != null
            && ShopManager.Instance.CanPurchase(selectedShopItem.ShopItem);

        sellButton.interactable =
            ShopManager.Instance.EquipmentInventory.NonNullEquipment.Any();
    }
}

public enum Rarity
{
    Common = 200,
    Uncommon = 500,
    Rare = 1000,
    Epic = 5000,
    Legendary = 20000,
    Unique = 1000000,
}


