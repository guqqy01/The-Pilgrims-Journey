using Equipment;
using UnityEngine;

public class ShopItem
{
    public readonly EquipmentSO Equipment;

    public int StockAmount;

    public float DiscountAmount { private set; get; } = 0;
    public int FinalGoldCost => Mathf.RoundToInt(Equipment.goldCost - (Equipment.goldCost * DiscountAmount));

    public ShopItem(EquipmentSO equipment, int stockAmount)
    {
        Equipment = equipment;
        StockAmount = stockAmount;
    }
}

