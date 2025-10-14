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

    private void Start()
    {
        ShopManager.Instance.onShopSetup -= Setup;
        ShopManager.Instance.onUpdateSellGold -= UpdateSellGold;

        ShopManager.Instance.onShopSetup += Setup;
        ShopManager.Instance.onUpdateSellGold += UpdateSellGold;
        Setup(ShopManager.Instance.EquipmentAndTheirShopItems);
    }

    private void OnDestroy()
    {
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
        foreach (var item in shopItems.Values)
        {
            item.OnButtonPress -= ShopItemButtonPress;
            Destroy(item.gameObject);
        }
        shopItems.Clear();
    }

    public void Setup(Dictionary<EquipmentSO, ShopItem> equipmentAndStock)
    {
        Clear();

        foreach (KeyValuePair<EquipmentSO, ShopItem> equipment in equipmentAndStock)
        {
            ShopItemUI item = Instantiate(ShopItemPrefab, shopItemsContentTransform);
            item.Setup(equipment.Value);
            shopItems.Add(equipment.Key, item);
            item.OnButtonPress += ShopItemButtonPress;
        }
    }

    private void UpdateSellGold()
    {
        if (selectedShopItem == null)
        {
            sellGoldAmount.text = "NA";
            return;
        }

        sellGoldAmount.text = ShopManager.Instance.GetSellGoldAmount().ToString();

        UpdateButtonStates();
    }

    private void ShopItemButtonPress(ShopItemUI shopItem)
    {
        IInfo.onInfoSelected?.Invoke(shopItem.Equipment);

        selectedShopItem = shopItem;

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


