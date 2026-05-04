using Equipment;
using System;
using UnityEngine;

public class ShopItem
{
    public readonly EquipmentSO Equipment;

    private int stockAmount;
    public int StockAmount
    {
        get { return stockAmount; }
        set
        {
            // Prevent negative stock
            int clamped = Mathf.Max(0, value);
            if (stockAmount == clamped) return;

            stockAmount = clamped;
            onStockAmountChange?.Invoke(stockAmount);
        }
    }

    private int stockDifference;
    /// <summary>
    /// Positive = sold items (stock increased), Negative = purchased items (stock decreased).
    /// </summary>
    public int FromOriginalStockDifference
    {
        get => stockDifference;
        set
        {
            stockDifference = value;
            onRefreshPurchasePrice?.Invoke();
        }
    }

    public bool HasPurchasedItems => stockDifference < 0;
    public bool HasSoldItems => stockDifference > 0;


    private float discountAmount;
    /// <summary>
    /// Discount applied to this item (0–1 range). For example, 0.2 = 20% off.
    /// </summary>
    public float DiscountAmount
    {
        get => discountAmount;
        private set => discountAmount = Mathf.Clamp01(value);
    }


    public delegate void OnStockAmountChange(int newStockAmount);
    public OnStockAmountChange onStockAmountChange;

    public delegate void OnRefreshPurchasePrice();
    public OnRefreshPurchasePrice onRefreshPurchasePrice;

    public ShopItem(EquipmentSO equipment, int stockAmount, float discountAmount = 0f)
    {
        Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
        DiscountAmount = discountAmount;
        StockAmount = stockAmount;
    }

    public int GetNormalPriceWithoutMultiplier()
    {
        return Mathf.RoundToInt(Equipment.goldCost - (Equipment.goldCost * DiscountAmount));
    }

    public int GetPurchaseGoldAmount()
    {
        float multiplier = HasSoldItems ? 0.5f : 1f;


        return Mathf.RoundToInt((Equipment.goldCost - (Equipment.goldCost * DiscountAmount)) * multiplier);
    }

    public int GetSellGoldAmount()
    {
        // If FromOriginalStockDifference < 0, you purchased it; sell at full price.
        // Otherwise (>= 0) you sold it to shop; sell (buy back) at half price.
        float multiplier = HasPurchasedItems ? 1f : 0.5f;
        int basePrice = Mathf.RoundToInt(Equipment.goldCost - (Equipment.goldCost * DiscountAmount));
        return Mathf.RoundToInt(basePrice * multiplier);
    }

    [Serializable]
    public class ShopItemSave
    {
        public EquipmentSO equipment;

        public int stockAmount;

        public float discountAmount;

        public int stockDifference;

        public ShopItemSave(ShopItem shopItem)
        {
            equipment = shopItem.Equipment;
            stockAmount = shopItem.StockAmount;
            discountAmount = shopItem.DiscountAmount;
            stockDifference = shopItem.FromOriginalStockDifference;
        }

        public ShopItem GetShopItem()
        {
            return new ShopItem(equipment, stockAmount, discountAmount)
            {
                FromOriginalStockDifference = stockDifference
            };
        }
    }
}

