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
public class Shop : MonoBehaviour
{
    private EquipmentInventory equipmentInventory;

    private Dictionary<EquipmentSO, int> equipmentAndStock = new Dictionary<EquipmentSO, int>();

    private Dictionary<EquipmentSO, int> boughtEquipment = new Dictionary<EquipmentSO, int>();
    private Dictionary<EquipmentSO, int> soldEquipment = new Dictionary<EquipmentSO, int>();

    [SerializeField]
    private ShopItem ShopItemPrefab;

    private Dictionary<EquipmentSO, ShopItem> shopItems = new Dictionary<EquipmentSO, ShopItem>();

    [SerializeField]
    private Transform shopItemsContentTransform;

    [SerializeField]
    private Text purchaseGoldAmount;

    [SerializeField]
    private Text sellGoldAmount;

    private EquipmentSO selectedEquipment;

    [SerializeField]
    private Button buyButton;

    [SerializeField]
    private Button sellButton;

    private void Awake()
    {
        equipmentInventory = GetComponent<EquipmentInventory>();
        equipmentInventory.onEquipmentChanged += OnEquipmentChanged;
    }

    private void Start()
    {
        ShopManager.Instance.onSetupShop += Setup;
    }

    private void Clear()
    {
       foreach(ShopItem item in shopItems.Values)
        {
            Destroy(item.gameObject);
        }

        shopItems.Clear();
        equipmentAndStock.Clear();
        boughtEquipment.Clear();
        soldEquipment.Clear();
    }

    private void Setup(Dictionary<EquipmentSO, int> equipmentAndStock)
    {
        Clear();

        foreach (KeyValuePair<EquipmentSO, int> equipment in equipmentAndStock)
        {
            ShopItem item = Instantiate(ShopItemPrefab, shopItemsContentTransform);
            item.Setup(equipment.Key, equipment.Value);
            shopItems.Add(equipment.Key, item);
            item.onButtonPress += ShopItemButtonPress;
        }
    }

    private void ShopItemButtonPress(EquipmentSO equipment)
    {
        IInfo.onInfoSelected?.Invoke(equipment);

        UpdatePuchaseGoldAmount();

        selectedEquipment = equipment;

        buyButton.interactable = equipmentAndStock[selectedEquipment] > 0;
    }

    private void UpdatePuchaseGoldAmount()
    {
        if (selectedEquipment == null)
        {
            purchaseGoldAmount.text = "NA";
            return;
        }

        if (soldEquipment.ContainsKey(selectedEquipment))
        {
            purchaseGoldAmount.text = Mathf.RoundToInt(selectedEquipment.goldCost / 2f).ToString();
        }
        else
        {
            purchaseGoldAmount.text = selectedEquipment.goldCost.ToString();
        }
    }

    public void PurchaseButton()
    {
        if (selectedEquipment == null || ResourceManager.Instance.Gold < selectedEquipment.goldCost || equipmentAndStock[selectedEquipment] <= 0)
            return;

        if (!equipmentInventory.CanAddEquipment(selectedEquipment))
            return;

        if (soldEquipment.ContainsKey(selectedEquipment))
        {
            ResourceManager.Instance.ModifyGold(-Mathf.RoundToInt(selectedEquipment.goldCost / 2f));
            soldEquipment[selectedEquipment]--;

            if (soldEquipment[selectedEquipment] <= 0)
                soldEquipment.Remove(selectedEquipment);

            UpdatePuchaseGoldAmount();
        }
        else
        {
            ResourceManager.Instance.ModifyGold(-selectedEquipment.goldCost);

            boughtEquipment.AddOrUpdateByOne(selectedEquipment);
        }

        if (equipmentInventory.TryAddEquipment(selectedEquipment))
        {
            equipmentAndStock[selectedEquipment]--;

            shopItems[selectedEquipment].SetStockAmount(equipmentAndStock[selectedEquipment]);

            buyButton.interactable = equipmentAndStock[selectedEquipment] > 0;
        }
    }

    public void SellButton()
    {
        float sellGold = 0f;

        foreach (EquipmentSO equipment in equipmentInventory.Equipment)
        {
            if (equipmentAndStock.ContainsKey(equipment))
            {
                equipmentAndStock[equipment]++;

                shopItems[equipment].SetStockAmount(equipmentAndStock[equipment]);
            }
            else
            {
                equipmentAndStock.Add(equipment, 1);

                ShopItem item = Instantiate(ShopItemPrefab, shopItemsContentTransform);
                item.Setup(equipment, 1);
                shopItems.Add(equipment, item);
                item.onButtonPress += ShopItemButtonPress;
            }

            if (boughtEquipment.ContainsKey(equipment))
            {
                sellGold += equipment.goldCost;

                boughtEquipment[equipment]--;

                if (boughtEquipment[equipment] <= 0)
                    boughtEquipment.Remove(equipment);
            }
            else
            {
                sellGold += equipment.goldCost / 2f;

                soldEquipment.AddOrUpdateByOne(equipment);
            }
        }

        UpdatePuchaseGoldAmount();

        equipmentInventory.Clear();

        ResourceManager.Instance.ModifyGold(Mathf.RoundToInt(sellGold));

        if(selectedEquipment != null)
            buyButton.interactable = equipmentAndStock[selectedEquipment] > 0;
    }

    private void OnEquipmentChanged()
    {
        float sellGold = 0f;

        Dictionary<EquipmentSO, int> copyOfBoughtEquipment = new Dictionary<EquipmentSO, int>();

        foreach(KeyValuePair<EquipmentSO, int> pair in boughtEquipment)
        {
            copyOfBoughtEquipment.Add(pair);
        }

        foreach(EquipmentSO equipment in equipmentInventory.Equipment)
        {
            if (copyOfBoughtEquipment.ContainsKey(equipment))
            {
                sellGold += equipment.goldCost;

                copyOfBoughtEquipment[equipment]--;

                if (copyOfBoughtEquipment[equipment] <= 0)
                    copyOfBoughtEquipment.Remove(equipment);
            }
            else
            {
                sellGold += equipment.goldCost / 2f;
            }
        }

        sellGoldAmount.text = Mathf.RoundToInt(sellGold).ToString();
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


