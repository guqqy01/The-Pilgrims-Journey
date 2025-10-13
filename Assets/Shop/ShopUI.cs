using Equipment;
using Map;
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



[RequireComponent(typeof(EquipmentInventory))]
public class ShopUI : MonoBehaviour, ISingletonToBeInitiatedFromRoot
{
    public static ShopUI Instance { get; private set; }

    [SerializeField]
    private ShopItemUI ShopItemPrefab;

    private Dictionary<EquipmentSO, ShopItemUI> shopItems = new Dictionary<EquipmentSO, ShopItemUI>();

    [SerializeField]
    private Transform shopItemsContentTransform;

    [SerializeField]
    private Text purchaseGoldAmount;

    [SerializeField]
    private Text sellGoldAmount;

    private ShopItemUI selectedShopItem;

    [SerializeField]
    private Button buyButton;

    [SerializeField]
    private Button sellButton;

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

    private void Clear()
    {
        foreach(ShopItemUI item in shopItems.Values)
        {
            Destroy(item.gameObject);
        }

        shopItems.Clear();
    }

    public void Setup(Dictionary<EquipmentSO, int> equipmentAndStock)
    {
        Clear();

        foreach (KeyValuePair<EquipmentSO, int> equipment in equipmentAndStock)
        {
            ShopItemUI item = Instantiate(ShopItemPrefab, shopItemsContentTransform);
            item.Setup(equipment.Key, equipment.Value, equipment.Key.discounts.GetRandom());
            shopItems.Add(equipment.Key, item);
            item.onButtonPress += ShopItemButtonPress;
        }
    }

    private void ShopItemButtonPress(ShopItemUI shopItem)
    {
        IInfo.onInfoSelected?.Invoke(shopItem.Equipment);

        selectedShopItem = shopItem;

        UpdatePuchaseGoldAmount();

        buyButton.interactable = ShopManager.Instance.EquipmentAndTheirShopItems[shopItem.Equipment].StockAmount > 0;
    }

    private void UpdatePuchaseGoldAmount()
    {
        if (selectedShopItem == null)
        {
            purchaseGoldAmount.text = "NA";
            return;
        }

        if (ShopManager.Instance.SoldEquipment.ContainsKey(selectedShopItem.Equipment))
        {
            purchaseGoldAmount.text = Mathf.RoundToInt(selectedShopItem.FinalGoldCost / 2f).ToString();
        }
        else
        {
            purchaseGoldAmount.text = selectedShopItem.FinalGoldCost.ToString();
        }
    }

    public void PurchaseButton()
    {
        ShopManager.Instance.Purchase(selectedShopItem.Equipment);

        UpdatePuchaseGoldAmount();
    }

    public void SellButton()
    {
        ShopManager.Instance.Sell();

        UpdatePuchaseGoldAmount();

        if(selectedShopItem != null && selectedShopItem.Equipment != null)
            buyButton.interactable = ShopManager.Instance.EquipmentAndTheirShopItems[selectedShopItem.Equipment].StockAmount > 0;
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


